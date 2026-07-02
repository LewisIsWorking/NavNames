using NavNames.Services.Interfaces;
#if WINDOWS
using Velopack;
using Velopack.Sources;
#endif

namespace NavNames.Services;

/// <summary>
/// Velopack auto-updater pointed at the NavNames GitHub Releases feed. On non-Windows
/// builds (where Velopack is not referenced) every method is an inert no-op, so the app
/// still compiles and runs cross-platform.
/// </summary>
public sealed class UpdateService : IUpdateService
{
    public const string RepoUrl = "https://github.com/LewisIsWorking/NavNames";

#if WINDOWS
    private readonly UpdateManager _manager = new(new GithubSource(RepoUrl, null, false));
    private UpdateInfo? _pending;

    public async Task<string?> CheckForUpdatesAsync()
    {
        // Not Velopack-installed (e.g. `dotnet run`) -> nothing to update against.
        if (!_manager.IsInstalled) return null;

        var updates = await _manager.CheckForUpdatesAsync().ConfigureAwait(false);
        if (updates is null) return null;

        await _manager.DownloadUpdatesAsync(updates).ConfigureAwait(false);
        _pending = updates;
        return updates.TargetFullRelease.Version.ToString();
    }

    public void ApplyAndRestart()
    {
        if (_pending is not null)
            _manager.ApplyUpdatesAndRestart(_pending);
    }
#else
    public Task<string?> CheckForUpdatesAsync() => Task.FromResult<string?>(null);

    public void ApplyAndRestart()
    {
    }
#endif
}
