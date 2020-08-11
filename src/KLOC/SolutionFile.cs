using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLOC
{
    internal class SolutionFile : PathEnumerable
    {
        private CounterContext _ctx;
        private string _slnPath;
        public SolutionFile(string path, CounterContext ctx)
        {
            _ctx = ctx;
            _slnPath = path;
        }
        public override IEnumerator<string> GetEnumerator()
        {
            foreach (var projectPath in GetProjectPathsFromSolution())
                foreach (var file in new ProjectFile(projectPath, _ctx))
                    yield return file;
        }
        private IEnumerable<string> GetProjectPathsFromSolution()
        {
            var result = new List<string>();
            var dir = Path.GetDirectoryName(_slnPath);
            string line;
            using (var reader = new StreamReader(_slnPath))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("Project("))
                    {
                        var localPath = line.Split('"').FirstOrDefault(l => l.EndsWith(".csproj"));
                        var absPath = Path.GetFullPath(Path.Combine(dir, localPath));
                        if (absPath != null)
                        {
                            if (!_ctx.ProjectPaths.Contains(absPath))
                            {
                                _ctx.ProjectPaths.Add(absPath);
                                result.Add(absPath);
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
