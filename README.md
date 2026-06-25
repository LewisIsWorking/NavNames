# NavNames

**Name your folders once, jump to them from any shell.**

NavNames is a small cross-platform desktop app (Avalonia / .NET 10) that lets you curate
**name → folder path** shortcuts in a GUI, then generates the matching shell config for you. Type
`rider` instead of `cd C:\Users\you\RiderProjects`, `proj` to list everything, `info` for a cheat-sheet.

## Why not just use a CLI tool?

Terminal directory-bookmarkers already exist — [`zoxide`](https://github.com/ajeetdsouza/zoxide),
[`shellmark`](https://github.com/artempyanykh/shellmark), `shellcuts`. NavNames' wedge is different:
it's a **GUI + shell-config generator**, not another terminal-only bookmarker. You manage shortcuts
visually and NavNames writes the correct shell code into your profile — with your own
`shortcuts.json` as the source of truth.

## How it works

1. Shortcuts live in `%APPDATA%\NavNames\shortcuts.json` — the single source of truth.
2. On **Save**, NavNames generates PowerShell helpers (an ordered `$NavNames` map, a `proj`
   dispatcher with tab-completion, one bare-word function per shortcut, and an `info`/`shortcuts`
   table).
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
