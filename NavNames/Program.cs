using System;
using Avalonia;

namespace NavNames;

internal static class Program
{
    // Avalonia configuration; the [STAThread] entry point Windows UI requires.
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
