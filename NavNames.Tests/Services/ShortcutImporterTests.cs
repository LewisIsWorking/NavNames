using NavNames.Core.Services;

namespace NavNames.Tests.Services;

public class ShortcutImporterTests
{
    private readonly ShortcutImporter _sut = new();

    [Fact]
    public void Import_ParsesWorkspacesOrderedBlock()
    {
        const string profile = """
            # some unrelated line
            $Workspaces = [ordered]@{
                rider = 'C:\Users\Lewis\RiderProjects'
                web   = 'C:\Users\Lewis\WebstormProjects'
            }
            function proj { }
            """;

        var result = _sut.Import(profile);

        Assert.Equal(2, result.Count);
        Assert.Equal("rider", result[0].Name);
        Assert.Equal(@"C:\Users\Lewis\RiderProjects", result[0].Path);
        Assert.Equal("web", result[1].Name);
    }

    [Fact]
    public void Import_ParsesNavNamesBlock_ForRoundTrip()
    {
        const string profile = """
            $NavNames = [ordered]@{
                coo = 'C:\Users\Lewis\RiderProjects\ComeOnOverUno'
            }
            """;

        var result = _sut.Import(profile);

        Assert.Single(result);
        Assert.Equal("coo", result[0].Name);
    }

    [Fact]
    public void Import_HandlesDoubleQuotedValues()
    {
        const string profile = "$Workspaces = @{\n    docs = \"C:\\Docs\"\n}";

        var result = _sut.Import(profile);

        Assert.Single(result);
        Assert.Equal(@"C:\Docs", result[0].Path);
    }

    [Fact]
    public void Import_UnescapesDoubledSingleQuotes()
    {
        const string profile = """
            $Workspaces = [ordered]@{
                odd = 'C:\Lewis''s Folder'
            }
            """;

        var result = _sut.Import(profile);

        Assert.Equal(@"C:\Lewis's Folder", result[0].Path);
    }

    [Fact]
    public void Import_IgnoresPsCustomObjectBlocks()
    {
        // The proj function body contains a [pscustomobject]@{...}; only the real map should parse.
        const string profile = """
            $Workspaces = [ordered]@{
                rider = 'C:\r'
            }
            function proj {
                [pscustomobject]@{ Shortcut = $_.Key; 'Goes To' = $_.Value }
            }
            """;

        var result = _sut.Import(profile);

        Assert.Single(result);
        Assert.Equal("rider", result[0].Name);
    }

    [Fact]
    public void Import_FallsBackToFirstOrderedAssignment()
    {
        const string profile = """
            $MyDirs = [ordered]@{
                home = 'C:\Users\Lewis'
            }
            """;

        var result = _sut.Import(profile);

        Assert.Single(result);
        Assert.Equal("home", result[0].Name);
    }

    [Fact]
    public void Import_EmptyOrNoBlock_ReturnsEmpty()
    {
        Assert.Empty(_sut.Import(""));
        Assert.Empty(_sut.Import("Write-Host 'no hashtable here'"));
    }
}
