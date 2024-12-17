using System.Globalization;

namespace KLOC8;

internal class KlocCommand
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
        var result1 = $"PATH: {Path.GetFullPath(path)}";
        Console.WriteLine(result1);

        var ctx = new CounterContext();
        var sourceFileEnumerable = new ProjectDirectory(path);
        var (fileCount, counts) = CountOfLines(sourceFileEnumerable, enabledExts, ctx);
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
        Console.WriteLine("Source files:   {0,15:n0}", fileCount);
        Console.WriteLine("Bytes length:   {0,15:n0}", ctx.Bytes);
        Console.WriteLine("Empty lines:    {0,15:n0}", ctx.EmptyLines);
        Console.WriteLine("Longest file:   {0,15:n0} lines, {1}", ctx.LongestFileLength, Path.GetFullPath(ctx.LongestFile));
        Console.WriteLine("Longest line:   {0,15:n0} characters, {1}, line:{2}", ctx.LongestLineLength, Path.GetFullPath(ctx.LongestLineFile), ctx.LongestLineLine);

        //Console.WriteLine();
        //Console.WriteLine("File types:");
        //var sorted = ctx.FileTypes.OrderByDescending(x => x.Value);
        //foreach (var item in sorted)
        //    Console.WriteLine("{0,16}{1,15:n0}", item.Key, item.Value);

        //Console.WriteLine();
        //Console.WriteLine("KLOC per file types:");
        //var sorted2 = counts.OrderByDescending(x => x.Value);
        //foreach (var item in sorted2)
        //    Console.WriteLine("{0,16}{1,15:n0}", item.Key, item.Value);

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

    private (int fileCount, Dictionary<string, int> counts) CountOfLines(IEnumerable<string> sourceFiles, string[]? enabledExts, CounterContext ctx)
    {
        var file = new FileInfo("KLOC-TEMP.txt");
        file.Delete();

        var fileCount = 0;
        using (var writer = new StreamWriter("KLOC-TEMP.txt", false))
        {
            writer.WriteLine($"Path\tSLOC");
            foreach (var sourceFile in sourceFiles)
            {
                fileCount++;
                var lines = CountOfLines(sourceFile, enabledExts, ctx);
                if (lines != null)
                    writer.WriteLine($"{sourceFile}\t{lines}");
            }
        }

        var countsPerFileType = new Dictionary<string, int>();
        using (var reader = new StreamReader("KLOC-TEMP.txt"))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Path \t SLOC
                var parts = line.Split('\t');
                if (parts.Length != 2)
                    continue;

                var ext = Path.GetExtension(parts[0]);
                if (int.TryParse(parts[1], out var count))
                {
                    countsPerFileType.TryGetValue(ext, out var previousValue);
                    countsPerFileType[ext] = previousValue + count;
                }
            }
        }

        return (fileCount, countsPerFileType);
    }
    private int? CountOfLines(string sourceFile, string[]? enabledExts, CounterContext ctx)
    {
        var ext = Path.GetExtension(sourceFile)?.ToLowerInvariant() ?? "";

        if (enabledExts != null && !enabledExts.Contains(ext))
            return null;

        if (!ctx.FileTypes.ContainsKey(ext))
            ctx.FileTypes[ext] = 1;
        else
            ctx.FileTypes[ext]++;

        var fileInfo = new FileInfo(sourceFile);
        ctx.Bytes += fileInfo.Length;

        using (var stream = fileInfo.OpenRead())
            return CountOfLines(stream, sourceFile, ctx);
    }
    private int? CountOfLines(Stream stream, string filePath, CounterContext ctx)
    {
        var lines = 0;

        using var reader = new StreamReader(stream);

        string line;
        var lineIndex = 0;
        while ((line = reader.ReadLine()) != null)
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
        var mainProjectDirectory = new ProjectDirectory(path);
        var subDirectories = mainProjectDirectory.GetDirectories();

        var colWidth = subDirectories.Max(x => Path.GetFileName(x)?.Length ?? 0) + 2;
        var line = $"{new string('-', colWidth)} -------------  ------------------------------------------------------";
        WriteHead();
        Console.WriteLine("CONTAINER: " + path);
        Console.WriteLine();
        Console.WriteLine($"{"NAME".PadRight(colWidth)} Lines Of Code  Count of source top file types");
        Console.WriteLine(line);
        var sum = 0;

        foreach (var subDirectory in subDirectories)
        {
            var sourceFileEnumerable = new ProjectDirectory(subDirectory);
            var ctx = new CounterContext();
            CountOfLines(sourceFileEnumerable, enabledExts, ctx);
            Console.WriteLine($"{(Path.GetFileName(subDirectory) ?? "").PadRight(colWidth)} " +
                              $"{ctx.Lines,13:n0}  " +
                              $"{PrintAnalysis(ctx)}".PadRight(Console.WindowWidth-1));
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