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
        private CounterContext _ctx;
        private string _directoryPath;

        public ProjectDirectory(string directoryPath, CounterContext ctx)
        {
            _directoryPath = directoryPath;
            _ctx = ctx;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            // Enumerate all files in depth
            return new DirectoryEnumerable(_directoryPath).GetEnumerator();
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
                var dirs = Directory.GetDirectories(_path)
                    .Where(IsEnabledDirectory)
                    .ToArray();

                foreach (var dir in dirs)
                {
                    foreach (var file in new DirectoryEnumerable(dir))
                        yield return file;
                }

                foreach (var file in Directory.GetFiles(_path))
                    if (IsEnabledFile(file))
                        yield return file;
            }

            private static readonly string[] DisabledDirectoryNames = new[] { ".vs", "bin", "obj", "references", "packages", "testresults", "netstandard" };
            private bool IsEnabledDirectory(string path)
            {
                var name = Path.GetFileName(path).ToLowerInvariant();
                return !DisabledDirectoryNames.Contains(name);

            }

            private static readonly string[] DisabledExtensions = new[] { ".ico", ".jpg", ".png", ".gif" };
            private bool IsEnabledFile(string path)
            {
                var ext = Path.GetExtension(path)?.ToLowerInvariant();
                return !DisabledExtensions.Contains(ext);
            }
        }
    }
}
