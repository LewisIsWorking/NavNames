using System.Text;
using NavNames.Core.Models;
using NavNames.Core.Services.Interfaces;

namespace NavNames.Core.Services;

/// <summary>
/// Generates Bash/Zsh navigation helpers: an associative NAVNAMES array plus an
/// ordered NAVNAMES_KEYS list (Bash assoc arrays are unordered), a `proj`
/// dispatcher, one function per shortcut, and info/shortcuts. Pure string logic.
/// </summary>
public sealed class BashConfigGenerator : IShellConfigGenerator
{
    public string ShellName => "Bash / Zsh";

    public string Generate(IReadOnlyList<NavShortcut> shortcuts)
    {
        var sb = new StringBuilder();

        sb.Append("NAVNAMES_KEYS=(");
        sb.Append(string.Join(' ', shortcuts.Select(s => Quote(s.Name))));
        sb.AppendLine(")");

        sb.AppendLine("declare -A NAVNAMES=(");
        foreach (var shortcut in shortcuts)
            sb.AppendLine($"    [{Quote(shortcut.Name)}]={Quote(shortcut.Path)}");
        sb.AppendLine(")");
        sb.AppendLine();
        sb.Append(Helpers);

        return sb.ToString();
    }

    public string GenerateCommands(IReadOnlyList<NavCommand> commands)
    {
        if (commands.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();

        sb.Append("NAVCOMMANDS_KEYS=(");
        sb.Append(string.Join(' ', commands.Select(c => Quote(c.Name))));
        sb.AppendLine(")");

        sb.AppendLine("declare -A NAVCOMMANDS=(");
        foreach (var command in commands)
            sb.AppendLine($"    [{Quote(command.Name)}]={Quote(command.Command)}");
        sb.AppendLine(")");
        sb.AppendLine();
        sb.Append(CommandHelpers);

        return sb.ToString();
    }

    // Bash single-quote escaping: close quote, emit an escaped quote, reopen.
    private static string Quote(string value) => "'" + value.Replace("'", "'\\''") + "'";

    private const string Helpers =
        """
        # proj <name> jumps to a shortcut; bare `proj` lists them all.
        proj() {
            if [ -z "$1" ]; then
                local k
                for k in "${NAVNAMES_KEYS[@]}"; do
                    printf '%-16s %s\n' "$k" "${NAVNAMES[$k]}"
                done
                printf '%-16s %s\n' 'proj <key>' 'jump by key; bare proj lists all'
                return
            fi
            local target="${NAVNAMES[$1]}"
            if [ -n "$target" ]; then
                cd "$target" || return
            else
                echo "Unknown shortcut '$1'. Try: ${NAVNAMES_KEYS[*]}" >&2
            fi
        }

        # One bare-word function per shortcut; looks up the array at call time so
        # paths containing quotes are handled by the array's own escaping.
        for _nn_key in "${NAVNAMES_KEYS[@]}"; do
            eval "${_nn_key}() { cd \"\${NAVNAMES[${_nn_key}]}\" || return; }"
        done
        unset _nn_key

        # Tab-completion for `proj` (Bash; harmless when `complete` is unavailable).
        if type complete >/dev/null 2>&1; then
            complete -W "${NAVNAMES_KEYS[*]}" proj
        fi

        info() { proj; }
        shortcuts() { proj; }
        """;

    private const string CommandHelpers =
        """
        # One bare-word function per command; the array lookup runs at call time and
        # "$@" forwards any extra arguments you pass.
        for _nc_key in "${NAVCOMMANDS_KEYS[@]}"; do
            eval "${_nc_key}() { eval \"\${NAVCOMMANDS[${_nc_key}]}\" \"\$@\"; }"
        done
        unset _nc_key

        # cmds / commands -> print every command shortcut and what it runs.
        cmds() {
            local k
            for k in "${NAVCOMMANDS_KEYS[@]}"; do
                printf '%-16s %s\n' "$k" "${NAVCOMMANDS[$k]}"
            done
        }
        commands() { cmds; }
        """;
}
