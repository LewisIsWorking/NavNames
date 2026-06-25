using NavNames.Core.Services;

namespace NavNames.Tests.Services;

public class ManagedBlockWriterTests
{
    private const string ProfilePath = @"C:\fake\profile.ps1";

    private readonly InMemoryFileSystem _fs = new();
    private ManagedBlockWriter CreateSut() => new(_fs);

    [Fact]
    public void Write_NewFile_WrapsContentInMarkers()
    {
        CreateSut().Write(ProfilePath, "echo hi");

        var result = _fs.Files[ProfilePath];
        Assert.Contains(ManagedBlockWriter.BeginMarker, result);
        Assert.Contains("echo hi", result);
        Assert.Contains(ManagedBlockWriter.EndMarker, result);
    }

    [Fact]
    public void Write_PreservesContentOutsideTheBlock()
    {
        _fs.Files[ProfilePath] = "# my PATHEXT fix\n$env:PATHEXT = '.EXE'\n";

        CreateSut().Write(ProfilePath, "generated body");

        var result = _fs.Files[ProfilePath];
        Assert.Contains("# my PATHEXT fix", result);
        Assert.Contains("$env:PATHEXT = '.EXE'", result);
        Assert.Contains("generated body", result);
    }

    [Fact]
    public void Write_Twice_ReplacesBlockInsteadOfDuplicating()
    {
        var sut = CreateSut();
        sut.Write(ProfilePath, "first version");
        sut.Write(ProfilePath, "second version");

        var result = _fs.Files[ProfilePath];
        Assert.DoesNotContain("first version", result);
        Assert.Contains("second version", result);

        var beginCount = CountOccurrences(result, ManagedBlockWriter.BeginMarker);
        Assert.Equal(1, beginCount);
    }

    [Fact]
    public void Write_PreservesContentAfterTheBlockAcrossRewrites()
    {
        _fs.Files[ProfilePath] =
            $"before\n{ManagedBlockWriter.BeginMarker}\nold\n{ManagedBlockWriter.EndMarker}\nafter";

        CreateSut().Write(ProfilePath, "new");

        var result = _fs.Files[ProfilePath];
        Assert.Contains("before", result);
        Assert.Contains("after", result);
        Assert.Contains("new", result);
        Assert.DoesNotContain("old", result);
    }

    [Fact]
    public void Write_TakesBackupOfExistingFileOnce()
    {
        _fs.Files[ProfilePath] = "original";
        var backupPath = ProfilePath + ".navnames.bak";

        var sut = CreateSut();
        sut.Write(ProfilePath, "v1");
        sut.Write(ProfilePath, "v2");

        Assert.True(_fs.Files.ContainsKey(backupPath));
        Assert.Equal("original", _fs.Files[backupPath]);
    }

    [Fact]
    public void Write_NewFile_TakesNoBackup()
    {
        CreateSut().Write(ProfilePath, "body");

        Assert.False(_fs.Files.ContainsKey(ProfilePath + ".navnames.bak"));
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        var count = 0;
        var index = 0;
        while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }

        return count;
    }
}
