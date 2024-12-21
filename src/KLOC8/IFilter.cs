using System.Linq;

namespace KLOC8;

public interface IFilter
{
    bool IsEnabledDirectory(string path);
    bool IsEnabledFile(string path);
}

public class CommonListFilter(IDisk disk) : IFilter
{
    private static readonly string[] DisabledDirectoryNames =
    [
        ".git", ".vs", "bin", "obj", "docs", "references", "packages", "testresults", "netstandard",
        "node_modules", "runtimes" /* TaskExecutors/AsposePreviewGenerator */,
        "app_data", "nuget", "install-services", "install-services-core",
        "bootstrap"
    ];

    public bool IsEnabledDirectory(string path)
    {
        var name = disk.Path_GetFileName(path).ToLowerInvariant();
        return !DisabledDirectoryNames.Contains(name);
    }

    private static readonly string[] DisabledExtensions =
    [
        ".ico", ".jpg", ".png", ".gif", ".svg", ".zip", ".dll", ".exe", ".pdb", ".aab" /*Android Application Bundles*/,
        ".so" /* AsposePreviewGenerator */
    ];

    public bool IsEnabledFile(string path)
    {
        var ext = disk.Path_GetExtension(path).ToLowerInvariant();
        return !DisabledExtensions.Contains(ext);
    }
}