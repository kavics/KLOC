using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KLOC
{
    internal class ProjectFile : PathEnumerable
    {
        private string[] _relevantElementNames = new[] { "Compile", "Content", "EmbeddedResource", "None" };

        private CounterContext _ctx;
        private string _projPath;
        public ProjectFile(string path, CounterContext ctx)
        {
            _ctx = ctx;
            _projPath = path;

        }
        public override IEnumerator<string> GetEnumerator()
        {
            _ctx.Projects++;
            foreach (var sourceCodeFilePath in GetSourceCodeFilePathsFromProjectFile())
                yield return sourceCodeFilePath;
        }
        private IEnumerable<string> GetSourceCodeFilePathsFromProjectFile()
        {
            var dir = Path.GetDirectoryName(_projPath);
            var xml = XDocument.Load(_projPath);
            var fileNames = xml
                .Descendants()
                .Where(e => _relevantElementNames.Contains(e.Name.LocalName))
                .Attributes("Include")
                .Select(a => Path.Combine(dir, a.Value))
                .ToArray();
            return fileNames;
        }
    }
}
