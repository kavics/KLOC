using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLOC
{
    internal class SourceFiles : PathEnumerable
    {
        private IEnumerable<string> _files;

        public SourceFiles(IEnumerable<string> files)
        {
            _files = files;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            foreach (var file in _files)
                yield return file;
        }
    }
}
