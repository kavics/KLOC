using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLOC
{
    internal class Counter
    {
        internal void Count(CounterContext ctx)
        {
            if(ctx.SourceFileEnumerable != null)
                foreach (var sourceFile in ctx.SourceFileEnumerable)
                    CountOfLines(sourceFile, ctx);
        }

        private static void CountOfLines(string sourceFile, CounterContext ctx)
        {
            var ext = Path.GetExtension(sourceFile).ToLowerInvariant();
            if (!ctx.FileTypes.ContainsKey(ext))
                ctx.FileTypes[ext] = 1;
            else
                ctx.FileTypes[ext]++;

            var fileInfo = new FileInfo(sourceFile);
            ctx.Bytes += fileInfo.Length;

            using (var stream = fileInfo.OpenRead())
                CountOfLines(stream, ctx);

            ctx.SourceFileCount++;
        }
        private static void CountOfLines(Stream stream, CounterContext ctx)
        {
            string line;
            using (var reader = new StreamReader(stream))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    ctx.Lines++;
                    if (line.Trim().Length == 0)
                        ctx.EmptyLines++;
                    if (line.Length > ctx.LongestLine)
                        ctx.LongestLine = line.Length;
                }
            }
        }
    }
}
