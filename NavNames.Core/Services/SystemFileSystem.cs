using NavNames.Core.Services.Interfaces;

namespace NavNames.Core.Services;

/// <summary>Default <see cref="IFileSystem"/> backed by System.IO.</summary>
public sealed class SystemFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);

    public string ReadAllText(string path) => File.ReadAllText(path);

    public void WriteAllText(string path, string contents) => File.WriteAllText(path, contents);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
}
