using System.Text;
using NavNames.Core.Models;
using NavNames.Core.Services.Interfaces;

namespace NavNames.Core.Services;

/// <summary>
/// Generates PowerShell navigation helpers from a shortcut list: an ordered
/// $NavNames hashtable (the only dynamic part), a `proj` dispatcher with
/// tab-completion, one bare-word function per shortcut, and an `info`/`shortcuts`
/// table. Pure string logic — no file IO — so it is exhaustively unit-testable.
/// </summary>
public sealed class PowerShellConfigGenerator : IShellConfigGenerator
{
    public string ShellName => "PowerShell";

    public string Generate(IReadOnlyList<NavShortcut> shortcuts)
    {
        var sb = new StringBuilder();

        sb.AppendLine("$NavNames = [ordered]@{");
        foreach (var shortcut in shortcuts)
            sb.AppendLine($"    {shortcut.Name} = '{EscapeSingleQuotes(shortcut.Path)}'");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.Append(Helpers);

        return sb.ToString();
    }

    public string GenerateCommands(IReadOnlyList<NavCommand> commands)
    {
        if (commands.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();

        sb.AppendLine("$NavCommands = [ordered]@{");
        foreach (var command in commands)
            sb.AppendLine($"    {command.Name} = '{EscapeSingleQuotes(command.Command)}'");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.Append(CommandHelpers);

        return sb.ToString();
    }

    // PowerShell single-quoted strings escape a quote by doubling it.
    private static string EscapeSingleQuotes(string value) => value.Replace("'", "''");

    private const string Helpers =
        """
        # proj <name> jumps to a shortcut; bare `proj` lists them all.
        function proj {
            param([string]$Name)
            if (-not $Name) {
                $NavNames.GetEnumerator() | ForEach-Object {
                    [pscustomobject]@{ Shortcut = $_.Key; 'Goes To' = $_.Value }
                } | Format-Table -AutoSize
                return
            }
            $target = $NavNames[$Name]
            if ($target) { Set-Location $target }
            else { Write-Warning "Unknown shortcut '$Name'. Try: $($NavNames.Keys -join ', ')" }
        }

        # Tab-completion for `proj <key>` driven by the same map.
        Register-ArgumentCompleter -CommandName proj -ParameterName Name -ScriptBlock {
            param($commandName, $parameterName, $wordToComplete)
            $NavNames.Keys | Where-Object { $_ -like "$wordToComplete*" } |
                ForEach-Object { [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $NavNames[$_]) }
        }

        # Bare-word shortcut per key. GetNewClosure snapshots $key so each function
        # keeps its own target instead of all capturing the loop's final value.
        foreach ($key in $NavNames.Keys) {
            Set-Item "function:global:$key" ([scriptblock]::Create("Set-Location '$($NavNames[$key])'")).GetNewClosure()
        }

        # info / shortcuts -> print every shortcut and where it goes.
        function Show-NavNames {
            $rows = $NavNames.GetEnumerator() | ForEach-Object {
                [pscustomobject]@{ Shortcut = $_.Key; 'Goes To' = $_.Value }
            }
            $rows += [pscustomobject]@{ Shortcut = 'proj <key>'; 'Goes To' = 'jump by key (Tab-completes); bare proj lists all' }
            $rows += [pscustomobject]@{ Shortcut = 'info / shortcuts'; 'Goes To' = 'show this table' }
            $rows | Format-Table -AutoSize
        }
        function info      { Show-NavNames }
        function shortcuts { Show-NavNames }
        """;

    private const string CommandHelpers =
        """
        # One bare-word function per command. The command text is baked into the
        # scriptblock and @args forwards any extra arguments you pass.
        foreach ($key in $NavCommands.Keys) {
            Set-Item "function:global:$key" ([scriptblock]::Create("$($NavCommands[$key]) @args"))
        }

        # cmds / commands -> print every command shortcut and what it runs.
        function Show-NavCommands {
            $NavCommands.GetEnumerator() | ForEach-Object {
                [pscustomobject]@{ Command = $_.Key; Runs = $_.Value }
            } | Format-Table -AutoSize
        }
        function cmds     { Show-NavCommands }
        function commands { Show-NavCommands }
        """;
}
