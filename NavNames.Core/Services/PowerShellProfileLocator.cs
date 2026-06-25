using System.Diagnostics;
using NavNames.Core.Services.Interfaces;

namespace NavNames.Core.Services;

/// <summary>
/// Resolves $PROFILE.CurrentUserAllHosts. Prefers asking pwsh directly (which
/// correctly handles OneDrive-redirected Documents); falls back to the conventional
/// Documents\PowerShell\profile.ps1 path if pwsh is unavailable.
/// </summary>
public sealed class PowerShellProfileLocator : IProfileLocator
{
    public string ResolveProfilePath()
    {
        var fromShell = TryQueryPwsh();
        if (!string.IsNullOrWhiteSpace(fromShell))
            return fromShell.Trim();

        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(documents, "PowerShell", "profile.ps1");
    }

    private static string? TryQueryPwsh()
    {
        try
        {
            var psi = new ProcessStartInfo("pwsh", "-NoProfile -Command \"$PROFILE.CurrentUserAllHosts\"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process is null) return null;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);
            return output;
        }
        catch
        {
            return null;
        }
    }
}
