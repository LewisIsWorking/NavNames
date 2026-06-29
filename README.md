# NavNames

**Name your folders and commands once, use them from any shell.**

NavNames is a small cross-platform desktop app (Avalonia / .NET 10) that lets you curate
**name → folder path** *and* **name → command** shortcuts in a GUI, then generates the matching shell
config for you. Type `rider` instead of `cd C:\Users\you\RiderProjects`, `proj` to list folders,
`info` for a cheat-sheet — or bind `voicebridge` to `python "C:\...\voice_bridge.py"` and just type
`voicebridge` (extra arguments are forwarded), with `cmds` to list them.

## Why not just use a CLI tool?

Terminal directory-bookmarkers already exist — [`zoxide`](https://github.com/ajeetdsouza/zoxide),
[`shellmark`](https://github.com/artempyanykh/shellmark), `shellcuts`. NavNames' wedge is different:
it's a **GUI + shell-config generator**, not another terminal-only bookmarker. You manage shortcuts
visually and NavNames writes the correct shell code into your profile — with your own
`shortcuts.json` as the source of truth.

## Two kinds of shortcut

- **Folder shortcuts** — `name → folder path`. Typing the name `cd`s there; stored in
  `%APPDATA%\NavNames\shortcuts.json`.
- **Command shortcuts** — `name → command line`. Typing the name *runs* the command and forwards any
  extra arguments (e.g. `voicebridge --debug`); stored in `%APPDATA%\NavNames\commands.json`. List
  them with `cmds`/`commands`.

Names share one shell-function namespace, so a name can't be both a folder and a command — the app
validates that for you. (Zoxide export covers folders only and tells you when commands were skipped.)

## How it works

1. Shortcuts live in `%APPDATA%\NavNames\shortcuts.json` and `commands.json` — the single sources of truth.
2. On **Save**, NavNames generates PowerShell helpers: an ordered `$NavNames` map with a `proj`
   dispatcher + tab-completion, one bare-word function per folder shortcut, an `info`/`shortcuts`
   table, plus a `$NavCommands` map with one run-function per command and a `cmds` listing.
3. That script is written into a **marker-delimited managed block** in your PowerShell profile:

   ```powershell
   # >>> NavNames (generated - do not edit) >>>
   ...generated helpers...
   # <<< NavNames <<<
   ```

   Everything **outside** the markers is preserved byte-for-byte, and a one-time `.navnames.bak`
   backup is taken before the first edit. Saving twice replaces the block rather than duplicating it.

> **Tip:** the app lets you set the target profile path. Point it at a throwaway temp file first to
> see exactly what gets written before touching your real profile.

## Project layout

| Project | Purpose |
|---------|---------|
| `NavNames` | Avalonia desktop UI (MVVM via CommunityToolkit.Mvvm, Microsoft DI). |
| `NavNames.Core` | Pure business logic (store, generator, managed-block writer, validator). No Avalonia refs — fully unit-testable. |
| `NavNames.Tests` | xUnit + NSubstitute. Covers the generator, writer (idempotency/backup/preservation), validator, store. |

## Build & run

```powershell
dotnet build NavNames.slnx
dotnet test  NavNames.slnx
dotnet run --project NavNames\NavNames.csproj
```

## Targets

Pick a **target shell** in the app; each is an `IShellConfigGenerator`:

- **PowerShell** — `$NavNames` map, `proj` dispatcher + tab-completion, bare-word functions, `info` table.
- **Bash / Zsh** — associative `NAVNAMES` array, `proj`, bare-word functions, completion.
- **Zoxide (seed)** — `zoxide add` lines so `z <name>` jumps to your curated folders.

Use **Import from profile** to adopt shortcuts already defined in a `$NavNames`/`$Workspaces` block.
Releases are built with `scripts/release.ps1` (Velopack → `Setup.exe`).

## Roadmap (fast-follow)

- Auto-update on startup (install/update hooks are already wired via Velopack).
- Linux/macOS packaging (the app already builds cross-platform).
