using System.Collections;
using System.Collections.Generic;

namespace KLOC
{
    internal abstract class PathEnumerable : IEnumerable<string>
    {
        public abstract IEnumerator<string> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
