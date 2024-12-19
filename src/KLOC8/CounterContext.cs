using System.Collections.Generic;

namespace KLOC8;

internal class CounterContext
{
    public int Lines { get; set; }
    public long Bytes { get; set; }
    public int EmptyLines { get; set; }
    public int LongestLineLength { get; set; }
    public string LongestLineFile { get; set; }
    public int LongestLineLine { get; set; }
    public int LongestFileLength { get; set; }
    public string LongestFile { get; set; }
    public Dictionary<string, int> FileTypes { get; } = new Dictionary<string, int>();
}