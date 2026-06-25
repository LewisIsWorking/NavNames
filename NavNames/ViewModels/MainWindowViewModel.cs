using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavNames.Core.Models;
using NavNames.Core.Services.Interfaces;
using NavNames.Services.Interfaces;

namespace NavNames.ViewModels;

/// <summary>
/// Root ViewModel: edits the shortcut list, regenerates a live preview for the
/// selected target shell, and on Save validates -> persists JSON -> writes the
/// managed block into that shell's profile.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IShortcutStore _store;
    private readonly IManagedBlockWriter _writer;
    private readonly IShortcutValidator _validator;
    private readonly IFolderPickerService _folderPicker;
    private readonly IFileSystem _fileSystem;
    private readonly IShortcutImporter _importer;

    public ObservableCollection<ShortcutItemViewModel> Shortcuts { get; } = [];

    public IReadOnlyList<ShellTarget> ShellTargets { get; }

    [ObservableProperty] private ShellTarget _selectedShellTarget;
    [ObservableProperty] private string _profilePath;
    [ObservableProperty] private string _generatedPreview = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public MainWindowViewModel(
        IShortcutStore store,
        IReadOnlyList<ShellTarget> shellTargets,
        IManagedBlockWriter writer,
        IShortcutValidator validator,
        IFolderPickerService folderPicker,
        IFileSystem fileSystem,
        IShortcutImporter importer)
    {
        _store = store;
        _writer = writer;
        _validator = validator;
        _folderPicker = folderPicker;
        _fileSystem = fileSystem;
        _importer = importer;

        ShellTargets = shellTargets;
        _selectedShellTarget = shellTargets[0];
        _profilePath = _selectedShellTarget.Locator.ResolveProfilePath();

        foreach (var saved in _store.Load())
            Shortcuts.Add(NewItem(saved.Name, saved.Path));

        RefreshPreview();
    }

    // Switching shells retargets the profile path and regenerates the preview.
    partial void OnSelectedShellTargetChanged(ShellTarget value)
    {
        ProfilePath = value.Locator.ResolveProfilePath();
        RefreshPreview();
    }

    [RelayCommand]
    private void Add()
    {
        Shortcuts.Add(NewItem());
        RefreshPreview();
    }

    // Adopt shortcuts already defined in the target profile (e.g. a hand-written
    // $Workspaces block), merging in only names we don't already have.
    [RelayCommand]
    private void ImportFromProfile()
    {
        if (!_fileSystem.FileExists(ProfilePath))
        {
            StatusMessage = $"No profile found at {ProfilePath}.";
            return;
        }

        var existing = new HashSet<string>(
            Shortcuts.Select(s => s.Name), StringComparer.OrdinalIgnoreCase);

        var added = 0;
        foreach (var imported in _importer.Import(_fileSystem.ReadAllText(ProfilePath)))
        {
            if (!existing.Add(imported.Name))
                continue;

            Shortcuts.Add(NewItem(imported.Name, imported.Path));
            added++;
        }

        RefreshPreview();
        StatusMessage = added > 0
            ? $"Imported {added} new shortcut(s) from {ProfilePath}."
            : $"No new shortcuts found in {ProfilePath}.";
    }

    [RelayCommand]
    private void Save()
    {
        var models = CurrentModels();
        var errors = _validator.Validate(models);
        if (errors.Count > 0)
        {
            StatusMessage = "Cannot save: " + string.Join(" ", errors);
            return;
        }

        _store.Save(models);
        _writer.Write(ProfilePath, SelectedShellTarget.Generator.Generate(models));
        StatusMessage =
            $"Saved {models.Count} shortcut(s) to {ProfilePath}. Open a new terminal to use them.";
    }

    private ShortcutItemViewModel NewItem(string name = "", string path = "")
    {
        var item = new ShortcutItemViewModel(name, path, RemoveItem, BrowseItemAsync);
        item.PropertyChanged += OnItemChanged;
        return item;
    }

    private void RemoveItem(ShortcutItemViewModel item)
    {
        item.PropertyChanged -= OnItemChanged;
        Shortcuts.Remove(item);
        RefreshPreview();
    }

    private async Task BrowseItemAsync(ShortcutItemViewModel item)
    {
        var folder = await _folderPicker.PickFolderAsync();
        if (folder is null)
            return;

        item.Path = folder;
        if (string.IsNullOrWhiteSpace(item.Name))
            item.Name = SuggestName(folder);

        RefreshPreview();
    }

    private void OnItemChanged(object? sender, PropertyChangedEventArgs e) => RefreshPreview();

    private IReadOnlyList<NavShortcut> CurrentModels() =>
        Shortcuts.Select(s => s.ToModel()).ToList();

    private void RefreshPreview() =>
        GeneratedPreview = SelectedShellTarget.Generator.Generate(CurrentModels());

    // Suggest a shortcut name from a folder's leaf, e.g. "...\RiderProjects" -> "riderprojects".
    private static string SuggestName(string folderPath)
    {
        var leaf = Path.GetFileName(
            folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        return leaf.ToLowerInvariant();
    }
}
