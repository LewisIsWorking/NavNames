using NavNames.Core.Models;

namespace NavNames.Core.Services.Interfaces;

/// <summary>Validates shortcut names/paths before they are written to a profile.</summary>
public interface IShortcutValidator
{
    /// <summary>Returns a list of human-readable errors; an empty list means valid.</summary>
    IReadOnlyList<string> Validate(IReadOnlyList<NavShortcut> shortcuts);
}
