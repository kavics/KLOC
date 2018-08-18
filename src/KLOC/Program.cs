using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Console.WriteLine("                    or .sln file");
            Console.WriteLine("                    or .csproj file");
            Console.WriteLine("                    or .cs file");
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

            var ctx = new CounterContext
            {
                InputPath = args[0]
            };

            Run(ctx);

            if (Debugger.IsAttached)
            {
                Console.Write("-------- Press any key to exit --------");
                if (!System.Reflection.Assembly.GetExecutingAssembly().Location.Contains(@"\KlocTests\"))
                    Console.ReadKey();
            }
        }
        public static void Run(CounterContext ctx)
        {
            var mapper = new SourceCodeMapper();
            if (!mapper.Map(ctx))
            {
                Usage(mapper.Message);
                return;
            }

            new Counter().Count(ctx);

            Render(ctx);
        }
        private static void Render(CounterContext ctx)
        {
            var result = string.Format("KLOC:           {0:n0}", ctx.Lines / 1000);
            Console.WriteLine(result);
            Console.WriteLine(new string('=', result.Length));
            Console.WriteLine();
            Console.WriteLine("DETAILS");
            Console.WriteLine("-------");
            Console.WriteLine();
            Console.WriteLine("Projects:       {0,15:n0}", ctx.Projects);
            Console.WriteLine("Source files:   {0,15:n0}", ctx.SourceFileCount);
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