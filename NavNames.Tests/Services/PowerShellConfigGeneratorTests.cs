using NavNames.Core.Models;
using NavNames.Core.Services;

namespace NavNames.Tests.Services;

public class PowerShellConfigGeneratorTests
{
    private readonly PowerShellConfigGenerator _sut = new();

    [Fact]
    public void ShellName_IsPowerShell()
    {
        Assert.Equal("PowerShell", _sut.ShellName);
    }

    [Fact]
    public void Generate_EmitsOrderedHashtableEntryForEachShortcut()
    {
        var script = _sut.Generate([
            new NavShortcut("rider", @"C:\Users\Lewis\RiderProjects"),
            new NavShortcut("web", @"C:\Users\Lewis\WebstormProjects")
        ]);

        Assert.Contains("$NavNames = [ordered]@{", script);
        Assert.Contains(@"rider = 'C:\Users\Lewis\RiderProjects'", script);
        Assert.Contains(@"web = 'C:\Users\Lewis\WebstormProjects'", script);
    }

    [Fact]
    public void Generate_PreservesShortcutOrder()
    {
        var script = _sut.Generate([
            new NavShortcut("first", @"C:\a"),
            new NavShortcut("second", @"C:\b")
        ]);

        Assert.True(script.IndexOf("first", StringComparison.Ordinal)
            < script.IndexOf("second", StringComparison.Ordinal));
    }

    [Fact]
    public void Generate_EscapesSingleQuotesInPath()
    {
        var script = _sut.Generate([new NavShortcut("odd", @"C:\Lewis's Folder")]);

        Assert.Contains(@"odd = 'C:\Lewis''s Folder'", script);
    }

    [Fact]
    public void Generate_IncludesProjDispatcherAndInfoTable()
    {
        var script = _sut.Generate([new NavShortcut("rider", @"C:\x")]);

        Assert.Contains("function proj {", script);
        Assert.Contains("Register-ArgumentCompleter", script);
        Assert.Contains("function info", script);
        Assert.Contains("function shortcuts", script);
    }

    [Fact]
    public void Generate_IncludesBareWordClosureLoop()
    {
        var script = _sut.Generate([new NavShortcut("rider", @"C:\x")]);

        Assert.Contains("foreach ($key in $NavNames.Keys)", script);
        Assert.Contains("GetNewClosure()", script);
    }

    [Fact]
    public void Generate_EmptyList_StillEmitsHelpers()
    {
        var script = _sut.Generate([]);

        Assert.Contains("$NavNames = [ordered]@{", script);
        Assert.Contains("function proj {", script);
    }

    [Fact]
    public void Generate_HelpersAreLeftAligned_NotIndentedByRawStringLiteral()
    {
        // Guards the raw-string indentation: column-0 PowerShell must not be indented.
        var script = _sut.Generate([]);

        Assert.Contains("\nfunction proj {", script.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void GenerateCommands_EmptyList_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, _sut.GenerateCommands([]));
    }

    [Fact]
    public void GenerateCommands_EmitsOrderedMapAndRunLoop()
    {
        var script = _sut.GenerateCommands([
            new NavCommand("bridge", @"python ""C:\bot\voice_bridge.py""")
        ]);

        Assert.Contains("$NavCommands = [ordered]@{", script);
        Assert.Contains(@"bridge = 'python ""C:\bot\voice_bridge.py""'", script);
        Assert.Contains("foreach ($key in $NavCommands.Keys)", script);
        // @args forwards extra arguments to the saved command.
        Assert.Contains("@args", script);
    }

    [Fact]
    public void GenerateCommands_IncludesCmdsListing()
    {
        var script = _sut.GenerateCommands([new NavCommand("bridge", "python x.py")]);

        Assert.Contains("function cmds", script);
        Assert.Contains("function commands", script);
    }

    [Fact]
    public void GenerateCommands_EscapesSingleQuotesInCommand()
    {
        var script = _sut.GenerateCommands([new NavCommand("greet", "echo 'hi'")]);

        Assert.Contains("greet = 'echo ''hi'''", script);
    }
}
