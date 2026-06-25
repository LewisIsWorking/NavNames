using NavNames.Core.Services.Interfaces;

namespace NavNames.Core.Services;

/// <summary>
/// Inserts or replaces a marker-delimited "managed block" in a text file, leaving
/// everything outside the markers byte-for-byte intact. Idempotent: writing twice
/// replaces the block rather than duplicating it. Takes a one-time .navnames.bak
/// backup before the first edit of an existing file.
/// </summary>
public sealed class ManagedBlockWriter : IManagedBlockWriter
{
    public const string BeginMarker = "# >>> NavNames (generated - do not edit) >>>";
    public const string EndMarker = "# <<< NavNames <<<";

    private readonly IFileSystem _fileSystem;

    public ManagedBlockWriter(IFileSystem fileSystem) => _fileSystem = fileSystem;

    public void Write(string filePath, string blockContent)
    {
        var existing = _fileSystem.FileExists(filePath)
            ? _fileSystem.ReadAllText(filePath)
            : string.Empty;

        BackupOnce(filePath, existing);

        var nl = Environment.NewLine;
        var block = $"{BeginMarker}{nl}{blockContent}{nl}{EndMarker}";
        _fileSystem.WriteAllText(filePath, ReplaceOrAppend(existing, block, nl));
    }

    private void BackupOnce(string filePath, string existing)
    {
        if (string.IsNullOrEmpty(existing)) return;

        var backupPath = filePath + ".navnames.bak";
        if (_fileSystem.FileExists(backupPath)) return;

        _fileSystem.WriteAllText(backupPath, existing);
    }

    private static string ReplaceOrAppend(string existing, string block, string nl)
    {
        var beginIndex = existing.IndexOf(BeginMarker, StringComparison.Ordinal);
        var endIndex = existing.IndexOf(EndMarker, StringComparison.Ordinal);

        if (beginIndex >= 0 && endIndex > beginIndex)
        {
            var before = existing[..beginIndex];
            var after = existing[(endIndex + EndMarker.Length)..];
            return before + block + after;
        }

        if (string.IsNullOrEmpty(existing))
            return block + nl;

        var separator = existing.EndsWith(nl, StringComparison.Ordinal) ? nl : nl + nl;
        return existing + separator + block + nl;
    }
}
