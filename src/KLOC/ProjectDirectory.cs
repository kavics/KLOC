using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KLOC
{
    internal class ProjectDirectory : PathEnumerable
    {
        private readonly string _directoryPath;

        public ProjectDirectory(string directoryPath)
        {
            _directoryPath = directoryPath;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            // Enumerate all files in depth
            return new DirectoryEnumerable(_directoryPath).GetEnumerator();
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
                ".git", ".vs", "bin", "obj", "docs", "references", "packages", "testresults", "netstandard"
            };
            private static bool IsEnabledDirectory(string path)
            {
                var name = Path.GetFileName(path)?.ToLowerInvariant() ?? "";
                return !DisabledDirectoryNames.Contains(name);

            }

            private static readonly string[] DisabledExtensions =
            {
                ".ico", ".jpg", ".png", ".gif" , ".zip", ".dll", ".exe", ".pdb"
            };
            private static bool IsEnabledFile(string path)
            {
                var ext = Path.GetExtension(path)?.ToLowerInvariant();
                return !DisabledExtensions.Contains(ext);
            }
        }
    }
}
