using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLOC
{
    internal class ProjectDirectory : PathEnumerable
    {
        //private CounterContext _ctx;
        private string _directoryPath;

        public ProjectDirectory(string directoryPath, CounterContext ctx)
        {
            _directoryPath = directoryPath;
            //_ctx = ctx;
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

        class DirectoryEnumerable : PathEnumerable
        {
            private string _path;
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

            private static readonly string[] DisabledDirectoryNames = new[]
            {
                ".git", ".vs", "bin", "obj", "docs", "references", "packages", "testresults", "netstandard"
            };
            private static bool IsEnabledDirectory(string path)
            {
                var name = Path.GetFileName(path).ToLowerInvariant();
                return !DisabledDirectoryNames.Contains(name);

            }

            private static readonly string[] DisabledExtensions = new[]
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
