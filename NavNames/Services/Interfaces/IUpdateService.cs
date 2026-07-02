namespace NavNames.Services.Interfaces;

/// <summary>
/// Checks a release feed for a newer NavNames build and applies it. The implementation
/// is Velopack-backed on Windows and a no-op elsewhere (and under `dotnet run`, where the
/// app is not Velopack-installed), so callers never have to branch on platform.
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Checks for and downloads a newer release if one exists. Returns the new version
    /// string (ready to apply), or null when already up to date / not installed / offline.
    /// </summary>
    Task<string?> CheckForUpdatesAsync();

    /// <summary>Applies the update downloaded by the last check and restarts the app.</summary>
    void ApplyAndRestart();
}
