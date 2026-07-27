using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using NavNames.Core.Models;
using NavNames.Services.Interfaces;

namespace NavNames.ViewModels;

/// <summary>
/// Owns the editable command-shortcut list (collection, Add/Remove) and raises
/// <see cref="Changed"/> on any change so the coordinator refreshes its preview.
/// Mirrors <see cref="ShortcutsSectionViewModel"/> minus the folder-browse path.
/// </summary>
public partial class CommandsSectionViewModel : ViewModelBase
{
    private readonly IExternalLinkService _links;

    public CommandsSectionViewModel(IExternalLinkService links) => _links = links;

    public ObservableCollection<CommandItemViewModel> Items { get; } = [];

    /// <summary>Raised on add, remove, or any row edit.</summary>
    public event EventHandler? Changed;

    public IReadOnlyList<NavCommand> Models() => Items.Select(c => c.ToModel()).ToList();

    public void Load(IEnumerable<NavCommand> saved)
    {
        foreach (var item in saved)
            Items.Add(NewItem(item.Name, item.Command));
    }

    [RelayCommand]
    private void Add()
    {
        Items.Add(NewItem());
        Raise();
    }

    /// <summary>
    /// Opens the recipes guide. Command shortcuts are the feature people struggle to
    /// think of uses for, so the examples are surfaced right where they're authored.
    /// </summary>
    [RelayCommand]
    private Task OpenRecipes() => _links.OpenAsync(AppLinks.RecipesUrl);

    private CommandItemViewModel NewItem(string name = "", string command = "")
    {
        var item = new CommandItemViewModel(name, command, RemoveItem);
        item.PropertyChanged += OnItemChanged;
        return item;
    }

    private void RemoveItem(CommandItemViewModel item)
    {
        item.PropertyChanged -= OnItemChanged;
        Items.Remove(item);
        Raise();
    }

    private void OnItemChanged(object? sender, PropertyChangedEventArgs e) => Raise();

    private void Raise() => Changed?.Invoke(this, EventArgs.Empty);
}
