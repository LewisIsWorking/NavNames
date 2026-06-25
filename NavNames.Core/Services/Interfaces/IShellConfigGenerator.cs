using NavNames.Core.Models;

namespace NavNames.Core.Services.Interfaces;

/// <summary>
/// Turns shortcuts into shell-specific config script text (no file IO).
/// Implementations target a specific shell (PowerShell today; bash/zsh later).
/// </summary>
public interface IShellConfigGenerator
{
    /// <summary>Human label for the shell this generator targets (e.g. "PowerShell").</summary>
    string ShellName { get; }

    /// <summary>Generates the script body to embed in the shell profile's managed block.</summary>
    string Generate(IReadOnlyList<NavShortcut> shortcuts);
}
