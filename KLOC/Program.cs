using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KLOC
{
    internal class Program
    {
        private static void Usage(string message = null)
        {
            if (message != null)
                Console.WriteLine(message);
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("KLOC.exe <path>");
            Console.WriteLine("<path>: Location of source code directory");
            Console.WriteLine("                    or csproj file");
            Console.WriteLine("                    or sln file");
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Kay-LOC");
            Console.WriteLine("<? Kilo Lines Of Code.");
            Console.WriteLine();

            if (args.Length != 1)
            {
                Usage("Location of source code files is missing.");
                return;
            }
            var path = args[0];

            IEnumerable<string> sourceFileEnumerable = null;
            var ctx = new CounterContext();

            if (Directory.Exists(path))
            {
                //UNDONE: get *.cs files recursively
            }
            else if (File.Exists(path))
            {
                var ext = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
                switch (ext)
                {
                    case "cs":     sourceFileEnumerable = new[] { path };              break;
                    case "csproj": sourceFileEnumerable = new ProjectFile(path, ctx);  break;
                    case "sln":    sourceFileEnumerable = new SolutionFile(path, ctx); break;
                    default:       Usage("Unknown location.");                return;
                }
            }
            else
            {
                Usage("Location of source code files does not exist.");
                return;
            }

            var sourceFiles = sourceFileEnumerable.ToArray();
            CountLines(sourceFiles, ctx);

            var result = "KLOC:           " + ctx.Lines / 1000;
            Console.WriteLine(result);
            Console.WriteLine(new string('=', result.Length));
            Console.WriteLine("Projects:       " + ctx.Projects);
            Console.WriteLine("Source files:   " + sourceFiles.Length);
            Console.WriteLine("Count of lines: " + ctx.Lines);
            Console.WriteLine("Empty lines:    " + ctx.EmptyLines);
        }

        private static void CountLines(string[] sourceFiles, CounterContext ctx)
        {
            foreach (var sourceFile in sourceFiles)
                CountLines(sourceFile, ctx);
        }
        private static void CountLines(string sourceFile, CounterContext ctx)
        {
            string line;
            using (var reader = new StreamReader(sourceFile))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    ctx.Lines++;
                    if (line.Trim().Length == 0)
                        ctx.EmptyLines++;
                }
            }
        }
    }

    internal class CounterContext
    {
        public int Projects { get; set; }
        public int Lines { get; set; }
        public int EmptyLines { get; set; }
    }

    internal abstract class PathEnumerable : IEnumerable<string>
    {
        public abstract IEnumerator<string> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class ProjectFile : PathEnumerable
    {
        private CounterContext _ctx;
        private string _projPath;
        public ProjectFile(string path, CounterContext ctx)
        {
            _ctx = ctx;
            _projPath = path;

        }
        public override IEnumerator<string> GetEnumerator()
        {
            _ctx.Projects++;
            foreach (var sourceCodeFilePath in GetSourceCodeFilePathsFromProjectFile())
                yield return sourceCodeFilePath;
        }
        private IEnumerable<string> GetSourceCodeFilePathsFromProjectFile()
        {
            var dir = Path.GetDirectoryName(_projPath);
            var xml = XDocument.Load(_projPath);
            var fileNames = xml
                .Descendants()
                .Where(e => e.Name.LocalName == "Compile")
                .Attributes("Include")
                .Select(a => Path.Combine(dir, a.Value))
                .ToArray();
            return fileNames;
        }
    }

    internal class SolutionFile : PathEnumerable
    {
        private CounterContext _ctx;
        private string _slnPath;
        public SolutionFile(string path, CounterContext ctx)
        {
            _ctx = ctx;
            _slnPath = path;
        }
        public override IEnumerator<string> GetEnumerator()
        {
            var dir = Path.GetDirectoryName(_slnPath);
            foreach (var projectPath in GetProjectPathsFromSolution())
                foreach(var file in new ProjectFile(Path.Combine(dir, projectPath), _ctx))
                    yield return file;
        }
        private IEnumerable<string> GetProjectPathsFromSolution()
        {
            var result = new List<string>();
            string line;
            using(var reader = new StreamReader(_slnPath))
            {
                while((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("Project("))
                    {
                        var localPath = line.Split('"').FirstOrDefault(l => l.EndsWith(".csproj"));
                        if (localPath != null)
                            result.Add(localPath);
                    }
                }
            }
            return result;
        }
    }
}