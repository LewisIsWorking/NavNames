using NavNames.Core.Services.Interfaces;

namespace NavNames.Tests;

/// <summary>
/// Dictionary-backed <see cref="IFileSystem"/> for tests — lets us assert on
/// read-after-write behaviour (idempotency, backups) without touching the disk.
/// </summary>
internal sealed class InMemoryFileSystem : IFileSystem
{
    public Dictionary<string, string> Files { get; } = new(StringComparer.Ordinal);
    public List<string> CreatedDirectories { get; } = new();

    public bool FileExists(string path) => Files.ContainsKey(path);

    public string ReadAllText(string path) => Files[path];

    public void WriteAllText(string path, string contents) => Files[path] = contents;

    public void CreateDirectory(string path) => CreatedDirectories.Add(path);
}
