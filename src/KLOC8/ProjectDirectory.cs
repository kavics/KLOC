using System;
using System.Collections.Generic;
using System.Linq;

namespace KLOC8;

internal class ProjectDirectory(string directoryPath, IDisk disk) : PathEnumerable
{
    private readonly IDisk _disk = disk;

    public override IEnumerator<string> GetEnumerator()
    {
        var count = 0;
        var lastProgressAt = DateTime.Now;
        // Enumerate all files in depth
        foreach (var path in new DirectoryEnumerable(directoryPath, _disk))
        {
            count++;
            if ((DateTime.Now - lastProgressAt).TotalMilliseconds > 100)
            {
                var width = Console.WindowWidth-10;

                if (width > 20)
                {
                    var text = count + "  " + path;
                    if (text.Length > width)
                        text = text.Substring(0, width - 3) + "...";
                    text = text.PadRight(width+9, ' ');
                    Console.Write(text + "\r");
                }
                lastProgressAt = DateTime.Now;
            }
            yield return path;
        }
    }

    public string[] GetDirectories()
    {
        return new DirectoryEnumerable(directoryPath, _disk).GetEnabledDirectories();
    }

    private class DirectoryEnumerable(string path, IDisk disk) : PathEnumerable
    {
        private readonly IDisk _disk = disk;

        public override IEnumerator<string> GetEnumerator()
        {
            foreach (var dir in GetEnabledDirectories())
            {
                foreach (var file in new DirectoryEnumerable(dir, _disk))
                    yield return file;
            }

            foreach (var file in _disk.Directory_GetFiles(path))
                if (IsEnabledFile(file))
                    yield return file;
        }

        /// <summary>
        /// Returns all enabled child directories.
        /// </summary>
        public string[] GetEnabledDirectories()
        {
            return _disk.Directory_GetDirectories(path)
                .Where(IsEnabledDirectory)
                .ToArray();
        }

        private static readonly string[] DisabledDirectoryNames =
        [
            ".git", ".vs", "bin", "obj", "docs", "references", "packages", "testresults", "netstandard",
            "node_modules", "runtimes" /* TaskExecutors/AsposePreviewGenerator */, 
            "app_data", "nuget", "install-services", "install-services-core",
            "bootstrap"
        ];
        private static bool IsEnabledDirectory(string path)
        {
            var name = Path.GetFileName(path)?.ToLowerInvariant() ?? "";
            return !DisabledDirectoryNames.Contains(name);

        }

        private static readonly string[] DisabledExtensions =
        [
            ".ico", ".jpg", ".png", ".gif", ".svg", ".zip", ".dll", ".exe", ".pdb", ".aab" /*Android Application Bundles*/,
            ".so" /* AsposePreviewGenerator */
        ];
        private static bool IsEnabledFile(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();
            return !DisabledExtensions.Contains(ext);
        }
    }
}