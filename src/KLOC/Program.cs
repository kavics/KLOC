using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Console.WriteLine("KLOC.exe <path> [-l|k[p]] [-c]");
            Console.WriteLine("<path>: Location of source code directory (required)");
            Console.WriteLine("        Displays Kay-LOC and statistics of the source files in depth.");
            Console.WriteLine("-k:     Displays only Kay-LOC.");
            Console.WriteLine("-l:     Displays only source line count.");
            Console.WriteLine("-kp:    Displays only Kay-LOC and path.");
            Console.WriteLine("-lp:    Displays only source line count and path.");
            Console.WriteLine("-c:     Enumerate and count sub-directories and displays a name-count pairs in a table.");
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Kay-LOC");
            Console.WriteLine("<? Kilo Lines Of Code.");
            Console.WriteLine();

            var arguments = Arguments.Parse(args);
            if (arguments == null)
            {
                Usage("Invalid arguments.");
                return;
            }
            if (arguments.IsHelp)
            {
                Usage();
                return;
            }

            var timer = Stopwatch.StartNew();
            Run(arguments);
            Console.WriteLine("Processing time: " + timer.Elapsed);

            if (Debugger.IsAttached)
            {
                Console.Write("-------- Press any key to exit --------");
                if (!System.Reflection.Assembly.GetExecutingAssembly().Location.Contains(@"\KlocTests\"))
                    Console.ReadKey();
            }
        }
        public static void Run(Arguments arguments)
        {
            IEnumerable<string> sourceFileEnumerable = null;
            var ctx = new CounterContext();

            if (Directory.Exists(arguments.ProjectDirectory))
            {
                sourceFileEnumerable = new ProjectDirectory(arguments.ProjectDirectory, ctx);
            }
            else
            {
                Usage("Location of source code directory does not exist.");
                return;
            }

            Console.WriteLine("Path: " + arguments.ProjectDirectory);

            var sourceFiles = sourceFileEnumerable.ToArray();
            Counter.CountOfLines(sourceFiles, ctx);

            var result = string.Format("KLOC:           {0:n0}", ctx.Lines / 1000);
            Console.WriteLine(result);
            Console.WriteLine(new string('=', result.Length));
            Console.WriteLine();
            Console.WriteLine("DETAILS");
            Console.WriteLine("-------");
            Console.WriteLine();
            //Console.WriteLine("Projects:       {0,15:n0}", ctx.Projects);
            Console.WriteLine("Source files:   {0,15:n0}", sourceFiles.Length);
            Console.WriteLine("Bytes length:   {0,15:n0}", ctx.Bytes);
            Console.WriteLine("Longest line:   {0,15:n0}", ctx.LongestLine);
            Console.WriteLine("Count of lines: {0,15:n0}", ctx.Lines);
            Console.WriteLine("Empty lines:    {0,15:n0}", ctx.EmptyLines);
            Console.WriteLine("File types:");
            var sorted = ctx.FileTypes.OrderByDescending(x => x.Value);
            foreach (var item in sorted)
                Console.WriteLine("{0,16}{1,15:n0}", item.Key, item.Value);
        }
    }
}