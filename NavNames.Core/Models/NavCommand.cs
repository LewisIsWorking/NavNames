namespace NavNames.Core.Models;

/// <summary>
/// A single command shortcut: a short name that runs a command line (rather than
/// jumping to a folder). Typing the name in the shell runs <see cref="Command"/>,
/// forwarding any extra arguments. Stored separately from <see cref="NavShortcut"/>.
/// </summary>
/// <param name="Name">The shorthand the user types; becomes a shell function name.</param>
/// <param name="Command">The command line to run, e.g. <c>python "C:\tools\bot.py"</c>.</param>
/// <param name="Description">Optional human note (reserved for future UI use).</param>
public sealed record NavCommand(string Name, string Command, string? Description = null);
