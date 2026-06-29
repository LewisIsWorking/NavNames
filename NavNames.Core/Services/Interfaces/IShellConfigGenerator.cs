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

    /// <summary>Generates the directory-shortcut script body for the managed block.</summary>
    string Generate(IReadOnlyList<NavShortcut> shortcuts);

    /// <summary>
    /// Generates the command-shortcut script body (functions that run a saved
    /// command line, forwarding extra args). Returns an empty string when there
    /// are no commands, so it can be appended unconditionally.
    /// </summary>
    string GenerateCommands(IReadOnlyList<NavCommand> commands);
}
