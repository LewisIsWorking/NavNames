using NavNames.Core.Models;

namespace NavNames.Core.Services.Interfaces;

/// <summary>Persists the user's command shortcuts as the source of truth.</summary>
public interface ICommandStore
{
    /// <summary>The on-disk location of the store (for display/diagnostics).</summary>
    string StorePath { get; }

    IReadOnlyList<NavCommand> Load();

    void Save(IReadOnlyList<NavCommand> commands);
}
