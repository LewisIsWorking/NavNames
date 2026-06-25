using System.Text.RegularExpressions;
using NavNames.Core.Models;
using NavNames.Core.Services.Interfaces;

namespace NavNames.Core.Services;

/// <summary>
/// Best-effort import of shortcuts from an existing PowerShell profile by parsing
/// a simple `$Var = [ordered]@{ key = 'path' }` block. Prefers $NavNames or
/// $Workspaces; falls back to the first `[ordered]@{}` assignment. Handles single-
/// and double-quoted values and unescapes PowerShell's doubled single quotes.
/// Deliberately narrow: one entry per line, literal string values only.
/// </summary>
public sealed class ShortcutImporter : IShortcutImporter
{
    private static readonly Regex PreferredBlock = new(
        @"\$(?:NavNames|Workspaces)\s*=\s*(?:\[ordered\])?\s*@\{(.*?)\}",
        RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex FallbackBlock = new(
        @"\$[A-Za-z_][A-Za-z0-9_]*\s*=\s*\[ordered\]\s*@\{(.*?)\}",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex Entry = new(
        @"^\s*(?<key>[A-Za-z][A-Za-z0-9_-]*)\s*=\s*(?<q>['""])(?<val>.*?)\k<q>\s*$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    public IReadOnlyList<NavShortcut> Import(string profileText)
    {
        if (string.IsNullOrWhiteSpace(profileText))
            return [];

        var match = PreferredBlock.Match(profileText);
        if (!match.Success)
            match = FallbackBlock.Match(profileText);
        if (!match.Success)
            return [];

        var body = match.Groups[1].Value;
        var result = new List<NavShortcut>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match entry in Entry.Matches(body))
        {
            var key = entry.Groups["key"].Value;
            if (!seen.Add(key))
                continue;

            var value = Unescape(entry.Groups["val"].Value, entry.Groups["q"].Value);
            result.Add(new NavShortcut(key, value));
        }

        return result;
    }

    // PowerShell single-quoted strings escape a quote by doubling it.
    private static string Unescape(string value, string quote) =>
        quote == "'" ? value.Replace("''", "'") : value;
}
