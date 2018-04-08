using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLOC
{
    internal class CounterContext
    {
        public int Projects { get; set; }
        public int Lines { get; set; }
        public long Bytes { get; set; }
        public int EmptyLines { get; set; }
        public int LongestLine { get; internal set; }
        public Dictionary<string, int> FileTypes { get; } = new Dictionary<string, int>();
    }
}
