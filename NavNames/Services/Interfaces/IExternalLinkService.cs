namespace NavNames.Services.Interfaces;

/// <summary>Opens a URL in the user's default browser.</summary>
public interface IExternalLinkService
{
    Task OpenAsync(string url);
}
