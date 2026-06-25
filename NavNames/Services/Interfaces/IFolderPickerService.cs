namespace NavNames.Services.Interfaces;

/// <summary>Shows a folder-picker dialog and returns the chosen path, or null if cancelled.</summary>
public interface IFolderPickerService
{
    Task<string?> PickFolderAsync();
}
