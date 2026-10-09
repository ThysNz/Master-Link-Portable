using System.Reflection;

namespace MasterLink.Desktop.Services;

// Single source for the version shown in About - it comes from <Version> in
// the csproj, so bumping it there is the only edit needed for a release.
public static class AppInfo
{
    public static string Version { get; } = ReadVersion();

    private static string ReadVersion()
    {
        var info = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (string.IsNullOrEmpty(info)) return "1.0.0";
        var plus = info.IndexOf('+');          // drop the build-metadata suffix (commit hash)
        return plus >= 0 ? info[..plus] : info;
    }
}
