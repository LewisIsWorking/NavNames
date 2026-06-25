using NavNames.Core.Services.Interfaces;

namespace NavNames.Core.Services;

/// <summary>
/// Resolves the Bash profile (~/.bashrc). Zsh users can repoint the target to
/// ~/.zshrc in the UI; the generated helpers work in both shells.
/// </summary>
public sealed class BashProfileLocator : IProfileLocator
{
    public string ResolveProfilePath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".bashrc");
    }
}
