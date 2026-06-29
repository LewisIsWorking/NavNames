using System.Text;
using NavNames.Core.Models;
using NavNames.Core.Services.Interfaces;

namespace NavNames.Core.Services;

/// <summary>
/// Generates `zoxide add` seed lines so the user's curated shortcuts are known to
/// zoxide's frecency database (then `z &lt;name&gt;` jumps to them). Lets NavNames
/// complement zoxide rather than replace it. PowerShell-style single-quote escaping;
/// paths without quotes (the common case) are identical across shells. Pure logic.
/// </summary>
public sealed class ZoxideConfigGenerator : IShellConfigGenerator
{
    public string ShellName => "Zoxide (seed)";

    public string Generate(IReadOnlyList<NavShortcut> shortcuts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Seed zoxide with your NavNames shortcuts; then `z <name>` jumps to them.");
        foreach (var shortcut in shortcuts)
            sb.AppendLine($"zoxide add -- '{Escape(shortcut.Path)}'");

        return sb.ToString().TrimEnd();
    }

    // zoxide tracks directories only; command shortcuts have nowhere to go here.
    // Emit a visible note (never a silent drop) so the user knows why.
    public string GenerateCommands(IReadOnlyList<NavCommand> commands)
    {
        if (commands.Count == 0)
            return string.Empty;

        var plural = commands.Count == 1 ? "command shortcut" : "command shortcuts";
        return $"# {commands.Count} {plural} not exported - zoxide handles directories only.";
    }

    private static string Escape(string value) => value.Replace("'", "''");
}
