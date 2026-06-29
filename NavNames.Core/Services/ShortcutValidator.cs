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

    public IReadOnlyList<string> Validate(
        IReadOnlyList<NavShortcut> shortcuts,
        IReadOnlyList<NavCommand> commands)
    {
        var errors = new List<string>();
        // One namespace for both kinds: a name used as a directory cannot also be a command.
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var shortcut in shortcuts)
        {
            var name = ValidateName(shortcut.Name, "shortcut", seen, errors);
            if (name.Length > 0 && string.IsNullOrWhiteSpace(shortcut.Path))
                errors.Add($"Shortcut '{name}' has no path.");
        }

        foreach (var command in commands)
        {
            var name = ValidateName(command.Name, "command", seen, errors);
            if (name.Length > 0 && string.IsNullOrWhiteSpace(command.Command))
                errors.Add($"Command '{name}' has no command line.");
        }

        return errors;
    }

    // Trims and validates a name token, recording errors; returns "" if the name was empty.
    private static string ValidateName(
        string? rawName, string kind, HashSet<string> seen, List<string> errors)
    {
        var name = rawName?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            errors.Add($"A {kind} has an empty name.");
            return string.Empty;
        }

        if (!NamePattern.IsMatch(name))
            errors.Add($"'{name}' is not a valid name (use letters, digits, _ or -, starting with a letter).");

        if (!seen.Add(name))
            errors.Add($"Duplicate name '{name}' (names must be unique across shortcuts and commands).");

        return name;
    }
}
