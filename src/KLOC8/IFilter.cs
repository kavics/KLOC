using System;
using System.Collections.Generic;
using System.IO;
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

public class KlocIgnoreFileFilter(IDisk disk) : IFilter
{
    public static readonly string IgnoreFileName = ".klocignore";

    public bool IsEnabledDirectory(string path)
    {
        var subFilter = GetSubFilter(path);
        var directoryName = Path.GetFileName(path);
        var enabled = !subFilter.IgnoredDirectories.Contains(directoryName, StringComparer.InvariantCultureIgnoreCase);
        return enabled;
    }

    public bool IsEnabledFile(string path)
    {
        var subFilter = GetSubFilter(path);
        var extension = Path.GetExtension(path);
        var enabled = !subFilter.IgnoredFiles.Contains(extension, StringComparer.InvariantCultureIgnoreCase);
        return enabled;
    }

    public readonly List<(string path, OneLevelFilter filter)> SubFilters = new();
    private OneLevelFilter GetSubFilter(string path)
    {
        var keyPath = Path.GetDirectoryName(path);
        var ignoreFilePath = Path.Combine(keyPath!, IgnoreFileName);

        var existingRecord = SubFilters.FirstOrDefault(x => x.path == keyPath);
        if (existingRecord != default)
            return existingRecord.filter;

        var ignoreFile = disk.CreateFileDescriptor(ignoreFilePath);
        if (ignoreFile.Exists())
        {
            var fileContent = ignoreFile.GetReader().ReadToEnd();
            var filter = new KlocIgnoreFileParser().Parse(fileContent, ignoreFilePath);
            SubFilters.Add((keyPath!, filter));
            return filter;
        }

        var filterRecord = SubFilters
            .Where(x => path.StartsWith(x.path, StringComparison.InvariantCultureIgnoreCase))
            .OrderByDescending(x => x.path.Length)
            .FirstOrDefault();

        return filterRecord != default ? filterRecord.filter : OneLevelFilter.Empty;
    }
}
