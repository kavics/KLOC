using System;
using System.Collections.Generic;
using System.Linq;

namespace KLOC8;

internal class KlocCommand(IDisk disk)
{
    public void Execute(string path, bool isContainer, string? fileTypes)
    {
        string[]? enabledExts = fileTypes?.Split(',').Select(x=>x.Trim()).ToArray();

        if (isContainer)
        {
            ProcessContainer(path, enabledExts);
            return;
        }

        WriteHead();
        var result1 = $"PATH: {disk.Path_GetFullPath(path)}";
        Console.WriteLine(result1);

        var ctx = new CounterContext();
        var sourceFileEnumerable = new ProjectDirectory(path, disk);
        var counts = CountOfLines(sourceFileEnumerable, enabledExts, ctx);
        Console.Write(" ".PadRight(Console.WindowWidth - 1));
        Console.Write("\r");

        var result2 = $"KLOC: {ctx.Lines / 1000:n0}";
        //var result1 = $"Kay-LOC: {(ctx.Lines * 1.0 / 1000.0).ToString("0.###", CultureInfo.InvariantCulture)}";

        Console.WriteLine(result2);
        Console.WriteLine(new string('=', Math.Max(result1.Length, result2.Length)));
        Console.WriteLine();
        Console.WriteLine("DETAILS");
        Console.WriteLine("-------");
        Console.WriteLine();
        Console.WriteLine("Count of lines: {0,15:n0}", ctx.Lines);
        Console.WriteLine("Source files:   {0,15:n0}", ctx.FileCount);
        Console.WriteLine("Bytes length:   {0,15:n0}", ctx.Bytes);
        Console.WriteLine("Empty lines:    {0,15:n0}", ctx.EmptyLines);
        Console.WriteLine("Longest file:   {0,15:n0} lines, {1}", ctx.LongestFileLength, disk.Path_GetFullPath(ctx.LongestFile));
        Console.WriteLine("Longest line:   {0,15:n0} characters, {1}, line:{2}", ctx.LongestLineLength, disk.Path_GetFullPath(ctx.LongestLineFile), ctx.LongestLineLine);

        Console.WriteLine();
        Console.WriteLine("Count and KLOC per file types:");
        Console.WriteLine("       File type       KLOC  KLOC%      files  files%");
        Console.WriteLine("---------------- ---------- ------ ---------- -------");
        var sumFileCount = Convert.ToDouble(ctx.FileTypes.Values.Sum(x => x));
        var sumKloc = Convert.ToDouble(counts.Values.Sum(x => x));
        foreach (var (fileType, kloc) in counts.OrderByDescending(x => x.Value))
        {
            if (ctx.FileTypes.TryGetValue(fileType, out var files))
            {
                var klocPercent = 100.0d * kloc / sumKloc;
                var filesPercent = 100.0d * files / sumFileCount;
                Console.WriteLine("{0,16}{1,11:n0}{2,6:0.0}%{3,11:n0}{4,7:0.0}%", fileType, kloc, klocPercent, files, filesPercent);
            }
        }
    }

    private Dictionary<string, int> CountOfLines(IEnumerable<string> sourceFiles, string[]? enabledExts, CounterContext ctx)
    {
        disk.CreateFileDescriptor("KLOC-TEMP.txt")
            .Delete();

        var tempFile = disk.CreateFileDescriptor("KLOC-TEMP.txt");
        using (var writer = tempFile.GetWriter())
        {
            writer.WriteLine($"Path\tSLOC");
            foreach (var sourceFile in sourceFiles)
            {
                ctx.FileCount++;
                var lines = CountOfLines(sourceFile, enabledExts, ctx);
                if (lines != null)
                    writer.WriteLine($"{sourceFile}\t{lines}");
            }
        }

        var countsPerFileType = new Dictionary<string, int>();
        using (var reader = tempFile.GetReader())
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Path \t SLOC
                var parts = line.Split('\t');
                if (parts.Length != 2)
                    continue;

                var ext = disk.Path_GetExtension(parts[0]);
                if (int.TryParse(parts[1], out var count))
                {
                    countsPerFileType.TryGetValue(ext, out var previousValue);
                    countsPerFileType[ext] = previousValue + count;
                }
            }
        }

        return countsPerFileType;
    }
    private int? CountOfLines(string sourceFile, string[]? enabledExts, CounterContext ctx)
    {
        var ext = disk.Path_GetExtension(sourceFile)?.ToLowerInvariant() ?? "";

        if (enabledExts != null && !enabledExts.Contains(ext))
            return null;

        if (!ctx.FileTypes.ContainsKey(ext))
            ctx.FileTypes[ext] = 1;
        else
            ctx.FileTypes[ext]++;

//var fileInfo = new FileInfo(sourceFile);
//ctx.Bytes += fileInfo.Length;

//using (var stream = fileInfo.OpenRead())
//    return CountOfLines(stream, sourceFile, ctx);
        var fileDescriptor = disk.CreateFileDescriptor(sourceFile);
        ctx.Bytes += fileDescriptor.Length;

        using(var stream = fileDescriptor.OpenRead())
            return CountOfLines(stream, sourceFile, ctx);
    }
    private int? CountOfLines(System.IO.Stream stream, string filePath, CounterContext ctx)
    {
        var lines = 0;

        using var reader = new System.IO.StreamReader(stream);

        var lineIndex = 0;
        while (reader.ReadLine() is { } line)
        {
            lineIndex++;
            lines++;
            ctx.Lines++;
            if (line.Trim().Length == 0)
                ctx.EmptyLines++;
            if (line.Length > ctx.LongestLineLength)
            {
                ctx.LongestLineLength = line.Length;
                ctx.LongestLineFile = filePath;
                ctx.LongestLineLine = lineIndex;
            }
        }

        if (lines > ctx.LongestFileLength)
        {
            ctx.LongestFileLength = lines;
            ctx.LongestFile = filePath;
        }

        return lines;
    }

    private static void WriteHead()
    {
        Console.WriteLine($"<? Compute Kilo Lines Of Code.");
    }



    private void ProcessContainer(string path, string[]? enabledExts)
    {
        var mainProjectDirectory = new ProjectDirectory(path, disk);
        var subDirectories = mainProjectDirectory.GetDirectories();

        var colWidth = subDirectories.Max(x => disk.Path_GetFileName(x)?.Length ?? 0) + 2;
        var line = $"{new string('-', colWidth)} -------------  ------------------------------------------------------";
        WriteHead();
        Console.WriteLine("CONTAINER: " + path);
        Console.WriteLine();
        Console.WriteLine($"{"NAME".PadRight(colWidth)} Lines Of Code  Count of source top file types");
        Console.WriteLine(line);
        var sum = 0;

        foreach (var subDirectory in subDirectories)
        {
            var sourceFileEnumerable = new ProjectDirectory(subDirectory, disk);
            var ctx = new CounterContext();
            CountOfLines(sourceFileEnumerable, enabledExts, ctx);
            var msg = $"{(disk.Path_GetFileName(subDirectory) ?? "").PadRight(colWidth)} {ctx.Lines,13:n0}  {PrintAnalysis(ctx)}";
            msg = msg.PadRight(Console.WindowWidth - 1);
            Console.WriteLine(msg);
            sum += ctx.Lines;
        }

        Console.WriteLine(line);
        Console.WriteLine($"{"SUMMARY".PadRight(colWidth)} {sum,13:n0}");
    }

    private string PrintAnalysis(CounterContext ctx)
    {
        var sum = ctx.FileTypes.Values.Sum();

        var topSrcTypes = ctx.FileTypes
            .OrderByDescending(x => x.Value)
            .Select(x => $"{x.Value * 100 / sum,3}% {x.Key,-12} ")
            .Take(3)
            .ToArray();

        return string.Join(" ", topSrcTypes);
    }

}