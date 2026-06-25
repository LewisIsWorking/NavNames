using System.IO;
using System.Linq;

namespace NavNames.Tests.RepoHealth;

/// <summary>
/// Enforces the project's 200-line-per-file rule mechanically, so the limit can't
/// quietly erode. Mirrors the RepoHealth guard from the ClaudeDesktopLauncher repo.
/// </summary>
public class FileSizeLimitTests
{
    private const int MaxLines = 200;

    [Fact]
    public void NoCSharpFileExceedsLineLimit()
    {
        var root = FindRepoRoot();

        var offenders = Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsExempt(path))
            .Select(path => (Path: path, Lines: File.ReadAllLines(path).Length))
            .Where(file => file.Lines > MaxLines)
            .Select(file => $"{Path.GetRelativePath(root, file.Path)}: {file.Lines} lines")
            .ToList();

        Assert.True(offenders.Count == 0,
            "Files over 200 lines (extract into services):\n" + string.Join("\n", offenders));
    }

    private static bool IsExempt(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || path.EndsWith(".g.cs", StringComparison.Ordinal)
        || path.EndsWith(".g.i.cs", StringComparison.Ordinal);

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "NavNames.slnx")))
            dir = dir.Parent;

        return dir?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate repo root (NavNames.slnx).");
    }
}
