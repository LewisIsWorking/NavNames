using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using NavNames.Services.Interfaces;

namespace NavNames.Services;

/// <summary>
/// Opens links via the main window's platform launcher. Preferred over
/// Process.Start(UseShellExecute) because it stays cross-platform, matching the
/// StorageProvider approach in <see cref="AvaloniaFolderPickerService"/>.
/// </summary>
public sealed class AvaloniaExternalLinkService : IExternalLinkService
{
    public async Task OpenAsync(string url)
    {
        var topLevel = ResolveTopLevel();
        if (topLevel is null)
            return;

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            await topLevel.Launcher.LaunchUriAsync(uri);
    }

    private static TopLevel? ResolveTopLevel() =>
        Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
}
