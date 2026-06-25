namespace NavNames.Core.Services.Interfaces;

/// <summary>
/// Writes a marker-delimited managed block into a file idempotently, leaving
/// everything outside the markers untouched.
/// </summary>
public interface IManagedBlockWriter
{
    /// <summary>
    /// Inserts or replaces the managed block in <paramref name="filePath"/> with
    /// <paramref name="blockContent"/>. Creates the file if it does not exist.
    /// </summary>
    void Write(string filePath, string blockContent);
}
