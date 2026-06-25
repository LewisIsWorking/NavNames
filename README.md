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

## Roadmap (fast-follow)

- Additional generators behind `IShellConfigGenerator`: bash/zsh aliases, a `zoxide` seed export.
- Velopack packaging + tag-driven release workflow.
- Import an existing hand-written block to bootstrap `shortcuts.json`.

Currently targets **PowerShell**; the `IShellConfigGenerator` seam keeps other shells a small addition.
