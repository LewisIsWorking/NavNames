using System.Text.Json;
using NavNames.Core.Models;
using NavNames.Core.Services.Interfaces;

namespace NavNames.Core.Services;

/// <summary>
/// Stores command shortcuts as JSON under %APPDATA%\NavNames\commands.json.
/// Kept separate from shortcuts.json so the two concepts stay independent.
/// </summary>
public sealed class JsonCommandStore : ICommandStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly IFileSystem _fileSystem;

    public string StorePath { get; }

    public JsonCommandStore(IFileSystem fileSystem, string? storePath = null)
    {
        _fileSystem = fileSystem;
        StorePath = storePath ?? DefaultStorePath();
    }

    public IReadOnlyList<NavCommand> Load()
    {
        if (!_fileSystem.FileExists(StorePath))
            return [];

        var json = _fileSystem.ReadAllText(StorePath);
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return JsonSerializer.Deserialize<List<NavCommand>>(json, Options) ?? [];
    }

    public void Save(IReadOnlyList<NavCommand> commands)
    {
        var directory = Path.GetDirectoryName(StorePath);
        if (!string.IsNullOrEmpty(directory))
            _fileSystem.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(commands, Options);
        _fileSystem.WriteAllText(StorePath, json);
    }

    private static string DefaultStorePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "NavNames", "commands.json");
    }
}
