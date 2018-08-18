using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLOC
{
    public class SourceCodeMapper
    {
        public string Message { get; protected set; }
        public virtual bool Map(CounterContext ctx)
        {
            var path = ctx.InputPath;

            PathEnumerable sourceFileEnumerable = null;

            if (Directory.Exists(path))
            {
                sourceFileEnumerable = new ProjectDirectory(path, ctx);
            }
            else if (File.Exists(path))
            {
                var ext = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
                switch (ext)
                {
                    case "cs": sourceFileEnumerable = new SourceFiles(new[] { path }); break;
                    case "csproj": sourceFileEnumerable = new ProjectFile(path, ctx); break;
                    case "sln": sourceFileEnumerable = new SolutionFile(path, ctx); break;
                    default: Message = "Not a source file."; return false;
                }
            }
            else
            {
                Message = "Location of source code files does not exist.";
                return false;
            }

            ctx.SourceFileEnumerable = sourceFileEnumerable;
            return true;
        }
    }
}
