using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace KLOC8;

public class Counter(IDisk disk)
{
    public int Lines { get; private set; }
    public long Bytes { get; private set; }
    public int EmptyLines { get; private set; }
    public int LongestLineLength { get; private set; }
    public string LongestLineFile { get; private set; }
    public int LongestLineLine { get; private set; }
    public int LongestFileLength { get; private set; }
    public string LongestFile { get; private set; }
    public Dictionary<string, int> FileTypes { get; } = new Dictionary<string, int>();
    public Dictionary<string, int> LinesPerFileTypes { get; } = new Dictionary<string, int>();
    public int FileCount { get; private set; }

    public int CountOfLines(IEnumerable<string> sourceFiles, string[]? enabledExts)
    {
        disk.CreateFileDescriptor("KLOC-TEMP.txt")
            .Delete();

        var tempFile = disk.CreateFileDescriptor("KLOC-TEMP.txt");
        using (var writer = tempFile.GetWriter())
        {
            writer.WriteLine($"Path\tSLOC");
            foreach (var sourceFile in sourceFiles)
            {
                FileCount++;
                var lines = CountOfLines(sourceFile, enabledExts);
                if (lines != null)
                    writer.WriteLine($"{sourceFile}\t{lines}");
            }
        }

        return Lines;
    }
    private int? CountOfLines(string sourceFile, string[]? enabledExts)
    {
        var ext = disk.Path_GetExtension(sourceFile)?.ToLowerInvariant() ?? "";

        if (enabledExts != null && !enabledExts.Contains(ext))
            return null;

        if (!FileTypes.ContainsKey(ext))
            FileTypes[ext] = 1;
        else
            FileTypes[ext]++;

        var fileDescriptor = disk.CreateFileDescriptor(sourceFile);

        using (var stream = fileDescriptor.OpenRead())
            return CountOfLines(stream, sourceFile);
    }
    public int? CountOfLines(System.IO.Stream stream, string filePath)
    {
        var lines = 0;

        Bytes += stream.Length;

        using var reader = new System.IO.StreamReader(stream);

        var lineIndex = 0;
        while (reader.ReadLine() is { } line)
        {
            lineIndex++;
            lines++;
            if (line.Trim().Length == 0)
                EmptyLines++;
            if (line.Length > LongestLineLength)
            {
                LongestLineLength = line.Length;
                LongestLineFile = filePath;
                LongestLineLine = lineIndex;
            }
        }

        if (lines > LongestFileLength)
        {
            LongestFileLength = lines;
            LongestFile = filePath;
        }

        // Actualize per file counts
        Lines += lines;

        var ext = disk.Path_GetExtension(filePath)?.ToLowerInvariant() ?? "";

        LinesPerFileTypes.TryGetValue(ext, out var previousValue);
        LinesPerFileTypes[ext] = previousValue + lines;

        //
        return lines;
    }

}