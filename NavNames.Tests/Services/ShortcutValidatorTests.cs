using NavNames.Core.Models;
using NavNames.Core.Services;

namespace NavNames.Tests.Services;

public class ShortcutValidatorTests
{
    private readonly ShortcutValidator _sut = new();

    [Fact]
    public void Validate_ValidShortcuts_ReturnsNoErrors()
    {
        var errors = _sut.Validate([
            new NavShortcut("rider", @"C:\a"),
            new NavShortcut("web-2", @"C:\b")
        ], []);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_EmptyName_ReportsError()
    {
        var errors = _sut.Validate([new NavShortcut("  ", @"C:\a")], []);

        Assert.Contains(errors, e => e.Contains("empty name"));
    }

    [Fact]
    public void Validate_NameStartingWithDigit_ReportsError()
    {
        var errors = _sut.Validate([new NavShortcut("2cool", @"C:\a")], []);

        Assert.Contains(errors, e => e.Contains("not a valid name"));
    }

    [Fact]
    public void Validate_NameWithSpaces_ReportsError()
    {
        var errors = _sut.Validate([new NavShortcut("my folder", @"C:\a")], []);

        Assert.Contains(errors, e => e.Contains("not a valid name"));
    }

    [Fact]
    public void Validate_DuplicateNamesCaseInsensitive_ReportsError()
    {
        var errors = _sut.Validate([
            new NavShortcut("Rider", @"C:\a"),
            new NavShortcut("rider", @"C:\b")
        ], []);

        Assert.Contains(errors, e => e.Contains("Duplicate"));
    }

    [Fact]
    public void Validate_EmptyPath_ReportsError()
    {
        var errors = _sut.Validate([new NavShortcut("rider", "  ")], []);

        Assert.Contains(errors, e => e.Contains("no path"));
    }

    [Fact]
    public void Validate_ValidCommands_ReturnsNoErrors()
    {
        var errors = _sut.Validate([], [new NavCommand("bot", @"python ""C:\bot.py""")]);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_CommandWithNoCommandLine_ReportsError()
    {
        var errors = _sut.Validate([], [new NavCommand("bot", "  ")]);

        Assert.Contains(errors, e => e.Contains("no command line"));
    }

    [Fact]
    public void Validate_InvalidCommandName_ReportsError()
    {
        var errors = _sut.Validate([], [new NavCommand("2bot", "python x.py")]);

        Assert.Contains(errors, e => e.Contains("not a valid name"));
    }

    [Fact]
    public void Validate_NameUsedByBothShortcutAndCommand_ReportsDuplicate()
    {
        // Directory and command functions share one shell namespace, so a name
        // cannot belong to both.
        var errors = _sut.Validate(
            [new NavShortcut("bridge", @"C:\a")],
            [new NavCommand("Bridge", "python bridge.py")]);

        Assert.Contains(errors, e => e.Contains("Duplicate"));
    }
}
