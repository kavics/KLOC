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
            var dir = Path.GetDirectoryName(_slnPath);
            foreach (var projectPath in GetProjectPathsFromSolution())
                foreach (var file in new ProjectFile(Path.Combine(dir, projectPath), _ctx))
                    yield return file;
        }
        private IEnumerable<string> GetProjectPathsFromSolution()
        {
            var result = new List<string>();
            string line;
            using (var reader = new StreamReader(_slnPath))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("Project("))
                    {
                        var localPath = line.Split('"').FirstOrDefault(l => l.EndsWith(".csproj"));
                        if (localPath != null)
                            result.Add(localPath);
                    }
                }
            }
            return result;
        }
    }
}
