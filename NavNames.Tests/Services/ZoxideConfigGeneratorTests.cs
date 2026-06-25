using NavNames.Core.Models;
using NavNames.Core.Services;

namespace NavNames.Tests.Services;

public class ZoxideConfigGeneratorTests
{
    private readonly ZoxideConfigGenerator _sut = new();

    [Fact]
    public void ShellName_IsZoxideSeed()
    {
        Assert.Equal("Zoxide (seed)", _sut.ShellName);
    }

    [Fact]
    public void Generate_EmitsZoxideAddLinePerShortcut()
    {
        var script = _sut.Generate([
            new NavShortcut("rider", @"C:\Users\Lewis\RiderProjects"),
            new NavShortcut("web", @"C:\Users\Lewis\WebstormProjects")
        ]);

        Assert.Contains(@"zoxide add -- 'C:\Users\Lewis\RiderProjects'", script);
        Assert.Contains(@"zoxide add -- 'C:\Users\Lewis\WebstormProjects'", script);
    }

    [Fact]
    public void Generate_UsesDoubleDashSoLeadingDashPathsAreNotFlags()
    {
        var script = _sut.Generate([new NavShortcut("x", @"C:\x")]);

        Assert.Contains("zoxide add -- ", script);
    }

    [Fact]
    public void Generate_EscapesSingleQuotesInPath()
    {
        var script = _sut.Generate([new NavShortcut("odd", @"C:\Lewis's Folder")]);

        Assert.Contains(@"zoxide add -- 'C:\Lewis''s Folder'", script);
    }

    [Fact]
    public void Generate_EmptyList_EmitsOnlyHeaderComment()
    {
        var script = _sut.Generate([]);

        Assert.StartsWith("# Seed zoxide", script);
        Assert.DoesNotContain("zoxide add", script);
    }
}
