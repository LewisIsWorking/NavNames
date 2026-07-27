# Recipes

Real command shortcuts that earn their keep. Each one is a **name → command line** entry you add in
the app (see [Two kinds of shortcut](../README.md#two-kinds-of-shortcut)); typing the name runs it and
forwards any extra arguments.

## `PsUpdate` — upgrade PowerShell itself

```text
Name:    PsUpdate
Command: winget upgrade --id Microsoft.PowerShell -e --accept-source-agreements
```

Then, from any PowerShell prompt:

```powershell
PsUpdate
```

### Why this one is worth a shortcut

PowerShell nags you when a new stable release lands, but the upgrade has a sharp edge: PowerShell 7
installs **in place** into `C:\Program Files\PowerShell\7`, so the file the installer must replace is
`pwsh.exe` — the very binary running your shell. Whether that succeeds comes down to one flag.

Adding `--silent` maps to `msiexec /qn`, which strips the installer's UI. When the MSI reaches its
`InstallValidate` step and finds `pwsh.exe` locked, it has nowhere to show the "these applications
must be closed" prompt, so it aborts with a bare **exit code 1603** and rolls everything back. You are
left on the old version with no useful error.

Without `--silent`, winget uses the interactive-with-progress mode instead. The MSI and Windows
Restart Manager negotiate the locked files properly and the upgrade completes. The difference is
stark in the Windows Installer event log:

| Invocation | Duration | Result |
|------------|----------|--------|
| `winget upgrade … --silent` | ~21s | `1603`, failed at `InstallValidate`, rolled back |
| `winget upgrade …` (interactive) | ~2m30s | `0`, installed successfully |

So the omission of `--silent` in the recipe above is **deliberate** — do not "tidy" it back in.

### Notes

- Approve the UAC prompt when it appears; winget hands the MSI to an elevated process.
- Restart Manager may close other running `pwsh.exe` processes to release the lock. Don't run this
  from inside a long-lived session you care about (an editor terminal, a background agent) — run it
  from a throwaway prompt. This is exactly why it works well as a one-word shortcut.
- Verify afterwards with `pwsh -v`. Already-open shells keep the old binary until you reopen them.

### Debugging a failed install

A bare 1603 tells you nothing on its own, but the **last action reached** in the MSI log names the
failure class — `InstallValidate` means file locks, `InstallFiles` means disk or permissions. Logs
live under:

```text
%LOCALAPPDATA%\Packages\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\LocalState\DiagOutputDir
```

Also worth knowing: a failed MSI **rolls back**, restoring the original files. An unchanged
`LastWriteTime` on the install directory therefore does *not* prove the installer never ran. Check the
`MsiInstaller` entries in the Application event log instead:

```powershell
Get-WinEvent -FilterHashtable @{LogName='Application'; ProviderName='MsiInstaller'} -MaxEvents 20 |
    Select-Object TimeCreated, Id, Message
```

## `DiscordBot` — run a script from anywhere, with arguments

```text
Name:    DiscordBot
Command: python "C:\Users\you\Projects\telegram-pbp-reminder\discord_bridge\voice_bridge.py"
```

```powershell
DiscordBot --debug      # extra arguments are forwarded to the command
```

Quote paths containing spaces inside the command line, as above. Because the command is baked into the
generated run-function, this works from any directory without a wrapper script on your `PATH`.

## Adding your own

Good candidates share a shape: a command you run **occasionally**, that you'd otherwise have to look
up, and that has a detail you'll forget — an exact package id, a flag that must or must not be there,
a long absolute path. Binding it to a name records the correct invocation once, in a place the app
keeps generated and in sync.
