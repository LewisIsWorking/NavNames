using System;
using Avalonia;
#if WINDOWS
using Velopack;
#endif

namespace NavNames;

internal static class Program
{
    // Avalonia configuration; the [STAThread] entry point Windows UI requires.
    [STAThread]
    public static void Main(string[] args)
    {
#if WINDOWS
        // Must run before Avalonia boots: Velopack re-invokes this exe for its
        // install/update/uninstall hooks, which must short-circuit before any
        // window would flash up during a silent update.
        VelopackApp.Build().Run();
#endif
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
