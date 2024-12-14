using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace KLOC
{
    internal class Counter
    {
        public static Dictionary<string, int> CountOfLines(string[] sourceFiles, CounterContext ctx)
        {
            using (var writer = new StreamWriter("KLOC-TEMP.txt", false))
            {
                writer.WriteLine($"Path\tSLOC");
                foreach (var sourceFile in sourceFiles)
                {
                    var lines = CountOfLines(sourceFile, ctx);
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

            return countsPerFileType;
        }
        public static int? CountOfLines(string sourceFile, CounterContext ctx)
        {
            var ext = Path.GetExtension(sourceFile)?.ToLowerInvariant() ?? "";

            var enabledExts = ctx.Arguments.EnabledFileExtensions;
            if (enabledExts != null && !enabledExts.Contains(ext))
                return null;

            if (!ctx.FileTypes.ContainsKey(ext))
                ctx.FileTypes[ext] = 1;
            else
                ctx.FileTypes[ext]++;

            var fileInfo = new FileInfo(sourceFile);
            ctx.Bytes += fileInfo.Length;

            using(var stream = fileInfo.OpenRead())
                return CountOfLines(stream, ctx);
        }
        public static int? CountOfLines(Stream stream, CounterContext ctx)
        {
            var lines = 0;

            using var reader = new StreamReader(stream);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                lines++;
                ctx.Lines++;
                if (line.Trim().Length == 0)
                    ctx.EmptyLines++;
                if (line.Length > ctx.LongestLine)
                    ctx.LongestLine = line.Length;
            }

            return lines;
        }
    }
}
