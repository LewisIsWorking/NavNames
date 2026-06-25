using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NavNames.Core.Services;
using NavNames.Core.Services.Interfaces;
using NavNames.Services;
using NavNames.Services.Interfaces;
using NavNames.ViewModels;
using NavNames.Views;

namespace NavNames;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        _serviceProvider = ConfigureServices().BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    // Single composition root. Singletons for stateless services; the ViewModel
    // is transient so a fresh one is built per window.
    private static ServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IFileSystem, SystemFileSystem>();
        services.AddSingleton<IShortcutStore>(sp =>
            new JsonShortcutStore(sp.GetRequiredService<IFileSystem>()));
        services.AddSingleton<IShellConfigGenerator, PowerShellConfigGenerator>();
        services.AddSingleton<IManagedBlockWriter, ManagedBlockWriter>();
        services.AddSingleton<IProfileLocator, PowerShellProfileLocator>();
        services.AddSingleton<IShortcutValidator, ShortcutValidator>();
        services.AddSingleton<IFolderPickerService, AvaloniaFolderPickerService>();

        services.AddTransient<MainWindowViewModel>();

        return services;
    }
}
