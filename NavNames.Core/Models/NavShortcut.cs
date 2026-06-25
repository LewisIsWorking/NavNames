namespace NavNames.Core.Models;

/// <summary>
/// A single navigation shortcut: a short name that resolves to a folder path.
/// </summary>
/// <param name="Name">The shorthand the user types; becomes a shell function name.</param>
/// <param name="Path">The absolute folder path the shortcut jumps to.</param>
/// <param name="Description">Optional human note (reserved for future UI use).</param>
public sealed record NavShortcut(string Name, string Path, string? Description = null);
