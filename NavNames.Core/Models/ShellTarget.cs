using NavNames.Core.Services.Interfaces;

namespace NavNames.Core.Models;

/// <summary>
/// Bundles a shell's config generator with its profile locator under a display
/// name, so the UI can offer a single "target shell" choice.
/// </summary>
public sealed record ShellTarget(string Name, IShellConfigGenerator Generator, IProfileLocator Locator)
{
    // ComboBox shows this by default.
    public override string ToString() => Name;
}
