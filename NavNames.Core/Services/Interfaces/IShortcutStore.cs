using NavNames.Core.Models;

namespace NavNames.Core.Services.Interfaces;

/// <summary>Persists the user's shortcuts as the source of truth.</summary>
public interface IShortcutStore
{
    /// <summary>The on-disk location of the store (for display/diagnostics).</summary>
    string StorePath { get; }

    IReadOnlyList<NavShortcut> Load();

    void Save(IReadOnlyList<NavShortcut> shortcuts);
}
