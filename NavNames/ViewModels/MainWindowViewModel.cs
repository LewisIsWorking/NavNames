using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavNames.Core.Models;
using NavNames.Core.Services.Interfaces;

namespace NavNames.ViewModels;

/// <summary>
/// Root coordinator: owns the shell-target choice, profile path, live preview and
/// status, and delegates the two editable lists to <see cref="Shortcuts"/> and
/// <see cref="Commands"/>. On Save it validates both, persists each store, and writes
/// the combined managed block (directory helpers + command helpers) into the profile.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IShortcutStore _store;
    private readonly ICommandStore _commandStore;
    private readonly IManagedBlockWriter _writer;
    private readonly IShortcutValidator _validator;
    private readonly IFileSystem _fileSystem;
    private readonly IShortcutImporter _importer;

    public ShortcutsSectionViewModel Shortcuts { get; }
    public CommandsSectionViewModel Commands { get; }

    // Shown in the header so the running build is always visible (tracks the csproj).
    public string AppVersion { get; } = NavNames.AppInfo.Version;

    public IReadOnlyList<ShellTarget> ShellTargets { get; }

    [ObservableProperty] private ShellTarget _selectedShellTarget;
    [ObservableProperty] private string _profilePath;
    [ObservableProperty] private string _generatedPreview = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public MainWindowViewModel(
        IShortcutStore store,
        ICommandStore commandStore,
        IReadOnlyList<ShellTarget> shellTargets,
        IManagedBlockWriter writer,
        IShortcutValidator validator,
        IFileSystem fileSystem,
        IShortcutImporter importer,
        ShortcutsSectionViewModel shortcuts,
        CommandsSectionViewModel commands)
    {
        _store = store;
        _commandStore = commandStore;
        _writer = writer;
        _validator = validator;
        _fileSystem = fileSystem;
        _importer = importer;

        Shortcuts = shortcuts;
        Commands = commands;
        Shortcuts.Changed += (_, _) => RefreshPreview();
        Commands.Changed += (_, _) => RefreshPreview();

        ShellTargets = shellTargets;
        _selectedShellTarget = shellTargets[0];
        _profilePath = _selectedShellTarget.Locator.ResolveProfilePath();

        Shortcuts.Load(_store.Load());
        Commands.Load(_commandStore.Load());

        RefreshPreview();
    }

    // Switching shells retargets the profile path and regenerates the preview.
    partial void OnSelectedShellTargetChanged(ShellTarget value)
    {
        ProfilePath = value.Locator.ResolveProfilePath();
        RefreshPreview();
    }

    // Adopt directory shortcuts already defined in the target profile (e.g. a
    // hand-written $Workspaces block), merging in only names we don't already have.
    [RelayCommand]
    private void ImportFromProfile()
    {
        if (!_fileSystem.FileExists(ProfilePath))
        {
            StatusMessage = $"No profile found at {ProfilePath}.";
            return;
        }

        var added = Shortcuts.Merge(_importer.Import(_fileSystem.ReadAllText(ProfilePath)));
        StatusMessage = added > 0
            ? $"Imported {added} new shortcut(s) from {ProfilePath}."
            : $"No new shortcuts found in {ProfilePath}.";
    }

    [RelayCommand]
    private void Save()
    {
        var shortcutModels = Shortcuts.Models();
        var commandModels = Commands.Models();

        var errors = _validator.Validate(shortcutModels, commandModels);
        if (errors.Count > 0)
        {
            StatusMessage = "Cannot save: " + string.Join(" ", errors);
            return;
        }

        _store.Save(shortcutModels);
        _commandStore.Save(commandModels);
        _writer.Write(ProfilePath, ComposeScript(shortcutModels, commandModels));

        StatusMessage =
            $"Saved {shortcutModels.Count} shortcut(s) and {commandModels.Count} command(s) to " +
            $"{ProfilePath}. Open a new terminal to use them.";
    }

    private void RefreshPreview() =>
        GeneratedPreview = ComposeScript(Shortcuts.Models(), Commands.Models());

    // Directory helpers first, then command helpers (blank-line separated when both exist).
    private string ComposeScript(
        IReadOnlyList<NavShortcut> shortcuts, IReadOnlyList<NavCommand> commands)
    {
        var generator = SelectedShellTarget.Generator;
        var dirs = generator.Generate(shortcuts);
        var cmds = generator.GenerateCommands(commands);
        return string.IsNullOrEmpty(cmds)
            ? dirs
            : dirs + Environment.NewLine + Environment.NewLine + cmds;
    }
}
