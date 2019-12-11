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
                    .Where(x => !x.EndsWith("\\.vs", StringComparison.OrdinalIgnoreCase))
                    .Where(x => !x.EndsWith("\\bin", StringComparison.OrdinalIgnoreCase))
                    .Where(x => !x.EndsWith("\\obj", StringComparison.OrdinalIgnoreCase))
                    .Where(x => !x.ToLower().Contains("\\packagesŁ\\"))
                    .Where(x => !x.ToLower().Contains("\\testresult\\"))
                    .Where(x => !x.ToLower().Contains("\\netstandard"))
                    .ToArray();

                foreach (var dir in dirs)
                {
                    foreach (var file in new DirectoryEnumerable(dir))
                        yield return file;
                }

                foreach (var file in Directory.GetFiles(_path))
                    yield return file;
            }
        }
    }
}
