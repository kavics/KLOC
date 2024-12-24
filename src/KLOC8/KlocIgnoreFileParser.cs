using System.Collections.Generic;
using System.IO;

namespace KLOC8;

public class KlocIgnoreFileParser
{
    private readonly List<string> _files = new();
    private readonly List<string> _directories = new();

    public OneLevelFilter Parse(string fileContent, string ignoreFilePath)
    {
        using var reader = new StringReader(fileContent);
        string? line;
        while ((line = reader.ReadLine()) != null)
            ParseLine(line);
        return new OneLevelFilter
        {
            FilePath = ignoreFilePath,
            IgnoredFiles = _files.ToArray(),
            IgnoredDirectories = _directories.ToArray(),
        };
    }

    private void ParseLine(string line)
    {
        line = line.Trim();
        if (line.Length == 0)
            return;
        if (line[0] == '#')
            return;
        if (line.EndsWith('/'))
            _directories.Add(line.TrimEnd('/'));
        else
            _files.Add(line.TrimStart('*'));
    }
}