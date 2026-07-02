using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavNames.Services.Interfaces;

namespace NavNames.ViewModels;

/// <summary>
/// Drives the header's update banner. Checks the release feed in the background; when a
/// newer build is downloaded it exposes a message + a "Restart &amp; update" command.
/// Kept separate from the root coordinator so all update/threading concerns live here.
/// </summary>
public partial class UpdateViewModel : ViewModelBase
{
    private readonly IUpdateService _updateService;

    [ObservableProperty] private string _message = string.Empty;
    [ObservableProperty] private bool _isUpdateAvailable;

    public UpdateViewModel(IUpdateService updateService) => _updateService = updateService;

    // Launched fire-and-forget from the UI thread; the await resumes there, so the
    // observable properties are set on the UI thread as Avalonia bindings require.
    public async Task CheckAsync()
    {
        var version = await _updateService.CheckForUpdatesAsync();
        if (string.IsNullOrEmpty(version)) return;

        Message = $"Update {version} downloaded.";
        IsUpdateAvailable = true;
    }

    [RelayCommand]
    private void Apply() => _updateService.ApplyAndRestart();
}
