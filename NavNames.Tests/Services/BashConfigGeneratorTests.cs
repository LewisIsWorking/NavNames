using NavNames.Core.Models;
using NavNames.Core.Services;

namespace NavNames.Tests.Services;

public class BashConfigGeneratorTests
{
    private readonly BashConfigGenerator _sut = new();

    [Fact]
    public void ShellName_IsBashZsh()
    {
        Assert.Equal("Bash / Zsh", _sut.ShellName);
    }

    [Fact]
    public void Generate_EmitsOrderedKeysArray()
    {
        var script = _sut.Generate([
            new NavShortcut("rider", "/home/lewis/rider"),
            new NavShortcut("web", "/home/lewis/web")
        ]);

        Assert.Contains("NAVNAMES_KEYS=('rider' 'web')", script);
    }

    [Fact]
    public void Generate_EmitsAssociativeArrayEntries()
    {
        var script = _sut.Generate([new NavShortcut("rider", "/home/lewis/rider")]);

        Assert.Contains("declare -A NAVNAMES=(", script);
        Assert.Contains("['rider']='/home/lewis/rider'", script);
    }

    [Fact]
    public void Generate_EscapesSingleQuotesInPath()
    {
        var script = _sut.Generate([new NavShortcut("odd", "/home/lewis/o'brien")]);

        // Bash single-quote escaping: o'brien -> o'\''brien
        Assert.Contains(@"['odd']='/home/lewis/o'\''brien'", script);
    }

    [Fact]
    public void Generate_IncludesProjBareWordLoopAndInfo()
    {
        var script = _sut.Generate([new NavShortcut("rider", "/x")]);

        Assert.Contains("proj() {", script);
        Assert.Contains("for _nn_key in \"${NAVNAMES_KEYS[@]}\"", script);
        Assert.Contains("info() { proj; }", script);
        Assert.Contains("shortcuts() { proj; }", script);
    }

    [Fact]
    public void Generate_EmptyList_StillEmitsHelpers()
    {
        var script = _sut.Generate([]);

        Assert.Contains("NAVNAMES_KEYS=()", script);
        Assert.Contains("proj() {", script);
    }
}
