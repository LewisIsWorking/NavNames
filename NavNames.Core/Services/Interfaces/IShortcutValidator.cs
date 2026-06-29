using NavNames.Core.Models;

namespace NavNames.Core.Services.Interfaces;

/// <summary>Validates shortcut/command names and values before they are written to a profile.</summary>
public interface IShortcutValidator
{
    /// <summary>
    /// Returns a list of human-readable errors; an empty list means valid. Names
    /// must be unique across <em>both</em> lists, since directory and command
    /// shortcuts share one shell-function namespace.
    /// </summary>
    IReadOnlyList<string> Validate(
        IReadOnlyList<NavShortcut> shortcuts,
        IReadOnlyList<NavCommand> commands);
}
