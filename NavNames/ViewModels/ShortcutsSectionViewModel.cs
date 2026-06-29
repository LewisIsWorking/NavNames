using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using NavNames.Core.Models;
using NavNames.Services.Interfaces;

namespace NavNames.ViewModels;

/// <summary>
/// Owns the editable directory-shortcut list (collection, Add/Remove/Browse) and
/// raises <see cref="Changed"/> whenever anything that affects the generated script
/// changes, so the coordinator can refresh its preview.
/// </summary>
public partial class ShortcutsSectionViewModel : ViewModelBase
{
    private readonly IFolderPickerService _folderPicker;

    public ObservableCollection<ShortcutItemViewModel> Items { get; } = [];

    /// <summary>Raised on add, remove, or any row edit.</summary>
    public event EventHandler? Changed;

    public ShortcutsSectionViewModel(IFolderPickerService folderPicker) => _folderPicker = folderPicker;

    public IReadOnlyList<NavShortcut> Models() => Items.Select(s => s.ToModel()).ToList();

    public void Load(IEnumerable<NavShortcut> saved)
    {
        foreach (var item in saved)
            Items.Add(NewItem(item.Name, item.Path));
    }

    /// <summary>Adds only names not already present (case-insensitive); returns how many were added.</summary>
    public int Merge(IEnumerable<NavShortcut> incoming)
    {
        var existing = new HashSet<string>(Items.Select(s => s.Name), StringComparer.OrdinalIgnoreCase);
        var added = 0;
        foreach (var item in incoming)
        {
            if (!existing.Add(item.Name))
                continue;
            Items.Add(NewItem(item.Name, item.Path));
            added++;
        }
        if (added > 0)
            Raise();
        return added;
    }

    [RelayCommand]
    private void Add()
    {
        Items.Add(NewItem());
        Raise();
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
        Items.Remove(item);
        Raise();
    }

    private async Task BrowseItemAsync(ShortcutItemViewModel item)
    {
        var folder = await _folderPicker.PickFolderAsync();
        if (folder is null)
            return;

        item.Path = folder;
        if (string.IsNullOrWhiteSpace(item.Name))
            item.Name = SuggestName(folder);

        Raise();
    }

    private void OnItemChanged(object? sender, PropertyChangedEventArgs e) => Raise();

    private void Raise() => Changed?.Invoke(this, EventArgs.Empty);

    // Suggest a name from a folder's leaf, e.g. "...\RiderProjects" -> "riderprojects".
    private static string SuggestName(string folderPath)
    {
        var leaf = Path.GetFileName(
            folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        return leaf.ToLowerInvariant();
    }
}
