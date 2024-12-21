using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;

namespace KLOC8;

internal class KlocCommand(IFilter filter, IDisk disk)
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
        Console.WriteLine($"PATH: {disk.Path_GetFullPath(path)}");

        var ctx = new Counter(disk);
        var sourceFileEnumerable = new ProjectDirectory(path, filter, disk);
        var sloc = ctx.CountOfLines(sourceFileEnumerable, enabledExts);
        Console.Write(" ".PadRight(Console.WindowWidth - 1));
        Console.Write("\r");

        PrintResult(path, ctx);
    }
    private void PrintResult(string path, Counter ctx)
    {
        var result1 = $"PATH: {disk.Path_GetFullPath(path)}";
        var result2 = $"KLOC: {ctx.Lines / 1000:n0}";

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
        Console.WriteLine("Longest file:   {0,15:n0} lines", ctx.LongestFileLength);
        Console.WriteLine("Longest file path:        {0}", disk.Path_GetFullPath(ctx.LongestFile));
        Console.WriteLine("Longest line:   {0,15:n0} characters", ctx.LongestLineLength);
        Console.WriteLine("Longest line location:    {0}, line:{1}", disk.Path_GetFullPath(ctx.LongestLineFile), ctx.LongestLineLine);

        Console.WriteLine();
        Console.WriteLine("Count and SLOC per file types:");
        Console.WriteLine("       File type       SLOC  SLOC%      files  files%");
        Console.WriteLine("---------------- ---------- ------ ---------- -------");
        var sumFileCount = Convert.ToDouble(ctx.FileTypes.Values.Sum(x => x));
        var sumKloc = Convert.ToDouble(ctx.LinesPerFileTypes.Values.Sum(x => x));
        foreach (var (fileType, kloc) in ctx.LinesPerFileTypes.OrderByDescending(x => x.Value))
        {
            if (ctx.FileTypes.TryGetValue(fileType, out var files))
            {
                var klocPercent = 100.0d * kloc / sumKloc;
                var filesPercent = 100.0d * files / sumFileCount;
                Console.WriteLine("{0,16}{1,11:n0}{2,6:0.0}%{3,11:n0}{4,7:0.0}%", fileType, kloc, klocPercent, files, filesPercent);
            }
        }
    }

    private void ProcessContainer(string path, string[]? enabledExts)
    {
        var mainProjectDirectory = new ProjectDirectory(path, filter, disk);
        var subDirectories = mainProjectDirectory.GetDirectories();

        var colWidth = subDirectories.Max(x => disk.Path_GetFileName(x)?.Length ?? 0) + 2;
        var line = $"{new string('-', colWidth)} -------------  ------------------------------------------------------";
        WriteHead();
        Console.WriteLine("CONTAINER: " + path);
        Console.WriteLine();
        Console.WriteLine($"{"NAME".PadRight(colWidth)} Lines Of Code  Top file types");
        Console.WriteLine(line);
        var sum = 0;

        foreach (var subDirectory in subDirectories)
        {
            var sourceFileEnumerable = new ProjectDirectory(subDirectory, filter, disk);
            var ctx = new Counter(disk);
            ctx.CountOfLines(sourceFileEnumerable, enabledExts);
            var msg = $"{(disk.Path_GetFileName(subDirectory) ?? "").PadRight(colWidth)} {ctx.Lines,13:n0}  {PrintAnalysis(ctx)}";
            msg = msg.PadRight(Console.WindowWidth - 1);
            Console.WriteLine(msg);
            sum += ctx.Lines;
        }

        Console.WriteLine(line);
        Console.WriteLine($"{"SUMMARY".PadRight(colWidth)} {sum,13:n0}");
    }
    private string PrintAnalysis(Counter ctx)
    {
        var sum = ctx.FileTypes.Values.Sum();

        var topSrcTypes = ctx.FileTypes
            .OrderByDescending(x => x.Value)
            .Select(x => $"{x.Value * 100 / sum,3}% {x.Key,-12} ")
            .Take(3)
            .ToArray();

        return string.Join(" ", topSrcTypes);
    }

    private static void WriteHead()
    {
        Console.WriteLine($"<? Compute Kilo Lines Of Code.");
    }

}