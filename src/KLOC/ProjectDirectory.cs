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
            var fileGroups = GetRelevantFiles();
            foreach (var fileGroup in fileGroups)
                foreach (var file in fileGroup)
                    yield return file;
        }

        private IEnumerable<PathEnumerable> GetRelevantFiles()
        {
            var slnFiles = Directory.GetFiles(_directoryPath, "*.sln");
            if (slnFiles.Length != 0)
                return slnFiles.Select(s => new SolutionFile(s, _ctx));

            var csprojFiles = Directory.GetFiles(_directoryPath, "*.csproj");
            if (csprojFiles.Length != 0)
                return csprojFiles.Select(s => new ProjectFile(s, _ctx));

            var csFiles = Directory.GetFiles(_directoryPath, "*.cs");
            if (csFiles.Length != 0)
                return new PathEnumerable[] { new SourceFiles(csFiles) };

            return new PathEnumerable[0];
        }
    }
}
