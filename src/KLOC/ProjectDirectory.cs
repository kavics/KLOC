using System;
using System.Collections.Generic;
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
            var files = GetRelevantFiles();
            foreach (var item in files)
                yield return item;
        }

        private PathEnumerable GetRelevantFiles()
        {
            throw new NotImplementedException();
        }
    }
}
