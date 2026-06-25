using NavNames.Core.Models;
using NavNames.Core.Services;

namespace NavNames.Tests.Services;

public class JsonShortcutStoreTests
{
    private const string StorePath = @"C:\fake\store\shortcuts.json";

    private readonly InMemoryFileSystem _fs = new();
    private JsonShortcutStore CreateSut() => new(_fs, StorePath);

    [Fact]
    public void Load_WhenFileMissing_ReturnsEmpty()
    {
        Assert.Empty(CreateSut().Load());
    }

    [Fact]
    public void SaveThenLoad_RoundTripsShortcuts()
    {
        var sut = CreateSut();
        var shortcuts = new List<NavShortcut>
        {
            new("rider", @"C:\Users\Lewis\RiderProjects", "IDE projects"),
            new("web", @"C:\Users\Lewis\WebstormProjects")
        };

        sut.Save(shortcuts);
        var loaded = sut.Load();

        Assert.Equal(2, loaded.Count);
        Assert.Equal("rider", loaded[0].Name);
        Assert.Equal(@"C:\Users\Lewis\RiderProjects", loaded[0].Path);
        Assert.Equal("IDE projects", loaded[0].Description);
        Assert.Null(loaded[1].Description);
    }

    [Fact]
    public void Save_CreatesParentDirectory()
    {
        CreateSut().Save([new NavShortcut("rider", @"C:\a")]);

        Assert.Contains(_fs.CreatedDirectories, d => d.Contains("store"));
    }

    [Fact]
    public void Load_WhenFileEmpty_ReturnsEmpty()
    {
        _fs.Files[StorePath] = "   ";

        Assert.Empty(CreateSut().Load());
    }

    [Fact]
    public void StorePath_ReflectsConfiguredPath()
    {
        Assert.Equal(StorePath, CreateSut().StorePath);
    }
}
