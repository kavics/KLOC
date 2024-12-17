namespace KLOC8;

internal class ProjectDirectory : PathEnumerable
{
    private readonly string _directoryPath;

    public ProjectDirectory(string directoryPath)
    {
        _directoryPath = directoryPath;
    }

    public override IEnumerator<string> GetEnumerator()
    {
        var count = 0;
        var lastProgressAt = DateTime.Now;
        // Enumerate all files in depth
        foreach (var path in new DirectoryEnumerable(_directoryPath))
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
        //return new DirectoryEnumerable(_directoryPath).GetEnumerator();
    }

    public string[] GetDirectories()
    {
        return DirectoryEnumerable.GetEnabledDirectories(_directoryPath);
    }

    private class DirectoryEnumerable : PathEnumerable
    {
        private readonly string _path;
        public DirectoryEnumerable(string path)
        {
            _path = path;
        }
        public override IEnumerator<string> GetEnumerator()
        {
            foreach (var dir in GetEnabledDirectories(_path))
            {
                foreach (var file in new DirectoryEnumerable(dir))
                    yield return file;
            }

            foreach (var file in Directory.GetFiles(_path))
                if (IsEnabledFile(file))
                    yield return file;
        }

        /// <summary>
        /// Returns all enabled child directories.
        /// </summary>
        public static string[] GetEnabledDirectories(string path)
        {
            return Directory.GetDirectories(path)
                .Where(IsEnabledDirectory)
                .ToArray();
        }

        private static readonly string[] DisabledDirectoryNames =
        {
            ".git", ".vs", "bin", "obj", "docs", "references", "packages", "testresults", "netstandard",
            "node_modules", "runtimes" /* TaskExecutors/AsposePreviewGenerator */, 
            "app_data", "nuget", "install-services", "install-services-core",
            "bootstrap"
        };
        private static bool IsEnabledDirectory(string path)
        {
            var name = Path.GetFileName(path)?.ToLowerInvariant() ?? "";
            return !DisabledDirectoryNames.Contains(name);

        }

        private static readonly string[] DisabledExtensions =
        {
            ".ico", ".jpg", ".png", ".gif", ".svg", ".zip", ".dll", ".exe", ".pdb", ".aab" /*Android Application Bundles*/,
            ".so" /* AsposePreviewGenerator */
        };
        private static bool IsEnabledFile(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();
            return !DisabledExtensions.Contains(ext);
        }
    }
}