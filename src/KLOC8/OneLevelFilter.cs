using System;

namespace KLOC8;

public class OneLevelFilter : IFilter
{
    public static OneLevelFilter Empty = new OneLevelFilter();

    public string FilePath { get; set; } = string.Empty;
    public string[] IgnoredFiles { get; set; } = Array.Empty<string>();
    public string[] IgnoredDirectories { get; set; } = Array.Empty<string>();

    public bool IsEnabledDirectory(string path)
    {
        throw new NotImplementedException();
    }

    public bool IsEnabledFile(string path)
    {
        throw new NotImplementedException();
    }
}