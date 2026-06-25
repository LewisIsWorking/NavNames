using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using NavNames.Services.Interfaces;

namespace NavNames.Services;

/// <summary>Avalonia folder picker backed by the main window's StorageProvider.</summary>
public sealed class AvaloniaFolderPickerService : IFolderPickerService
{
    public async Task<string?> PickFolderAsync()
    {
        var topLevel = ResolveTopLevel();
        if (topLevel is null)
            return null;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions { Title = "Pick a folder", AllowMultiple = false });

        return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
    }

    private static TopLevel? ResolveTopLevel() =>
        Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
}
