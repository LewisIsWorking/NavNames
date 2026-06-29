using NavNames.Core.Models;
using NavNames.Core.Services;

namespace NavNames.Tests.Services;

public class JsonCommandStoreTests
{
    private const string StorePath = @"C:\fake\store\commands.json";

    private readonly InMemoryFileSystem _fs = new();
    private JsonCommandStore CreateSut() => new(_fs, StorePath);

    [Fact]
    public void Load_WhenFileMissing_ReturnsEmpty()
    {
        Assert.Empty(CreateSut().Load());
    }

    [Fact]
    public void SaveThenLoad_RoundTripsCommands()
    {
        var sut = CreateSut();
        var commands = new List<NavCommand>
        {
            new("bridge", @"python ""C:\bot\voice_bridge.py""", "voice relay"),
            new("build", "dotnet build")
        };

        sut.Save(commands);
        var loaded = sut.Load();

        Assert.Equal(2, loaded.Count);
        Assert.Equal("bridge", loaded[0].Name);
        Assert.Equal(@"python ""C:\bot\voice_bridge.py""", loaded[0].Command);
        Assert.Equal("voice relay", loaded[0].Description);
        Assert.Null(loaded[1].Description);
    }

    [Fact]
    public void Save_CreatesParentDirectory()
    {
        CreateSut().Save([new NavCommand("build", "dotnet build")]);

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
