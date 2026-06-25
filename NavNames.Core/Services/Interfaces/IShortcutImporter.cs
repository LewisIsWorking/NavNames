using NavNames.Core.Models;

namespace NavNames.Core.Services.Interfaces;

/// <summary>Best-effort parse of shortcuts from an existing shell profile's text.</summary>
public interface IShortcutImporter
{
    /// <summary>
    /// Extracts shortcuts from a simple `$Var = [ordered]@{ key = 'path' }` block.
    /// Returns an empty list when no recognisable block is found.
    /// </summary>
    IReadOnlyList<NavShortcut> Import(string profileText);
}
