using System.Text.Json;
using NavNames.Core.Models;
using NavNames.Core.Services.Interfaces;

namespace NavNames.Core.Services;

/// <summary>
/// Stores shortcuts as JSON under %APPDATA%\NavNames\shortcuts.json.
/// This file is the single source of truth; the shell profile is generated from it.
/// </summary>
public sealed class JsonShortcutStore : IShortcutStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly IFileSystem _fileSystem;

    public string StorePath { get; }

    public JsonShortcutStore(IFileSystem fileSystem, string? storePath = null)
    {
        _fileSystem = fileSystem;
        StorePath = storePath ?? DefaultStorePath();
    }

    public IReadOnlyList<NavShortcut> Load()
    {
        if (!_fileSystem.FileExists(StorePath))
            return [];

        var json = _fileSystem.ReadAllText(StorePath);
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return JsonSerializer.Deserialize<List<NavShortcut>>(json, Options) ?? [];
    }

    public void Save(IReadOnlyList<NavShortcut> shortcuts)
    {
        var directory = Path.GetDirectoryName(StorePath);
        if (!string.IsNullOrEmpty(directory))
            _fileSystem.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(shortcuts, Options);
        _fileSystem.WriteAllText(StorePath, json);
    }

    private static string DefaultStorePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "NavNames", "shortcuts.json");
    }
}
