namespace NavNames.Core.Services.Interfaces;

/// <summary>
/// Thin filesystem abstraction so file-touching services stay unit-testable
/// (swap in an NSubstitute fake in tests instead of hitting the real disk).
/// </summary>
public interface IFileSystem
{
    bool FileExists(string path);
    string ReadAllText(string path);
    void WriteAllText(string path, string contents);
    void CreateDirectory(string path);
}
