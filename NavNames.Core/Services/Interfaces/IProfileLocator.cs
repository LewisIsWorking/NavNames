namespace NavNames.Core.Services.Interfaces;

/// <summary>Resolves the shell profile file the managed block should be written to.</summary>
public interface IProfileLocator
{
    /// <summary>Absolute path to the shell profile (a sensible default the user can override).</summary>
    string ResolveProfilePath();
}
