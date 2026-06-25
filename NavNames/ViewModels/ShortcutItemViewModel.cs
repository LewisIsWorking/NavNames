using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavNames.Core.Models;

namespace NavNames.ViewModels;

/// <summary>
/// One editable shortcut row. Remove/Browse are wired via callbacks supplied by
/// the parent ViewModel (the same callback pattern used by ClaudeInstanceViewModel),
/// so each row's buttons bind to its own commands without ancestor bindings.
/// </summary>
public partial class ShortcutItemViewModel : ViewModelBase
{
    private readonly Action<ShortcutItemViewModel>? _onRemove;
    private readonly Func<ShortcutItemViewModel, Task>? _onBrowse;

    [ObservableProperty] private string _name;
    [ObservableProperty] private string _path;

    public ShortcutItemViewModel(
        string name = "",
        string path = "",
        Action<ShortcutItemViewModel>? onRemove = null,
        Func<ShortcutItemViewModel, Task>? onBrowse = null)
    {
        _name = name;
        _path = path;
        _onRemove = onRemove;
        _onBrowse = onBrowse;
    }

    public NavShortcut ToModel() => new(Name.Trim(), Path.Trim());

    [RelayCommand]
    private void Remove() => _onRemove?.Invoke(this);

    [RelayCommand]
    private Task Browse() => _onBrowse?.Invoke(this) ?? Task.CompletedTask;
}
