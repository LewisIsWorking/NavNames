using System.Text.RegularExpressions;
using NavNames.Core.Models;
using NavNames.Core.Services.Interfaces;

namespace NavNames.Core.Services;

/// <summary>
/// Enforces that shortcut names are safe shell function tokens, unique
/// (case-insensitively), and have a non-empty path. A compiled Regex is used
/// instead of [GeneratedRegex] to avoid introducing a partial class.
/// </summary>
public sealed class ShortcutValidator : IShortcutValidator
{
    private static readonly Regex NamePattern =
        new("^[A-Za-z][A-Za-z0-9_-]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public IReadOnlyList<string> Validate(IReadOnlyList<NavShortcut> shortcuts)
    {
        var errors = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var shortcut in shortcuts)
        {
            var name = shortcut.Name?.Trim() ?? string.Empty;

            if (name.Length == 0)
            {
                errors.Add("A shortcut has an empty name.");
                continue;
            }

            if (!NamePattern.IsMatch(name))
                errors.Add($"'{name}' is not a valid name (use letters, digits, _ or -, starting with a letter).");

            if (!seen.Add(name))
                errors.Add($"Duplicate shortcut name '{name}'.");

            if (string.IsNullOrWhiteSpace(shortcut.Path))
                errors.Add($"Shortcut '{name}' has no path.");
        }

        return errors;
    }
}
