using System.Reflection;

namespace NavNames;

/// <summary>
/// Exposes the running app's version for display. Reads
/// <see cref="AssemblyInformationalVersionAttribute"/> (fed by &lt;Version&gt; in the
/// csproj) and strips any "+{git-sha}" build metadata the SDK appends, so the UI shows
/// a clean "v0.2.0". Reflection-sourced on purpose: the label tracks the csproj with no
/// second place to bump per release.
/// </summary>
internal static class AppInfo
{
    public static string Version { get; } = Resolve();

    private static string Resolve()
    {
        var asm = Assembly.GetExecutingAssembly();
        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (string.IsNullOrWhiteSpace(info))
            info = asm.GetName().Version?.ToString() ?? "0.0.0";

        var plus = info.IndexOf('+');
        return "v" + (plus >= 0 ? info[..plus] : info);
    }
}
