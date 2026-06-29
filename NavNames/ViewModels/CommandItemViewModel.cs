using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavNames.Core.Models;

namespace NavNames.ViewModels;

/// <summary>
/// One editable command row: a name plus the command line it runs. Remove is wired
/// via a parent-supplied callback (same pattern as <see cref="ShortcutItemViewModel"/>),
/// but there is no Browse since a command is not a folder.
/// </summary>
public partial class CommandItemViewModel : ViewModelBase
{
    private readonly Action<CommandItemViewModel>? _onRemove;

    [ObservableProperty] private string _name;
    [ObservableProperty] private string _command;

    public CommandItemViewModel(
        string name = "",
        string command = "",
        Action<CommandItemViewModel>? onRemove = null)
    {
        _name = name;
        _command = command;
        _onRemove = onRemove;
    }

    public NavCommand ToModel() => new(Name.Trim(), Command.Trim());

    [RelayCommand]
    private void Remove() => _onRemove?.Invoke(this);
}
