using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using KLOC;

namespace KlocTests
{
    [TestClass]
    public class ProgramTests
    {
        private static string _rootPath; // ...\KLOC

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // "...\KLOC\src\KlocTests\bin\Debug\KlocTests.dll" ==> "...\KLOC"
            _rootPath = Path.GetFullPath(
                Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location,
                @"..\..\..\..\..\"));
        }
        private string GetPath(string relativePath)
        {
            return Path.Combine(_rootPath, relativePath);
        }

        TextWriter _savedOutput;
        [TestInitialize]
        public void Initialize()
        {
            _savedOutput = Console.Out;
            var console = new StringWriter();
            Console.SetOut(console);
        }
        [TestCleanup]
        public void Cleanup()
        {
            Console.SetOut(_savedOutput);
        }

        [TestMethod]
        public void Parameter_Missing()
        {
            var console = new StringWriter();
            Console.SetOut(console);

            KLOC.Program.Main(new string[0]);

            var output = console.GetStringBuilder().ToString();
            string line;
            using (var reader = new StringReader(output))
            {
                line = reader.ReadLine();
                line = reader.ReadLine();
                line = reader.ReadLine();
                line = reader.ReadLine();
            }
            Assert.AreEqual("Location of source code files is missing.", line);
        }
        [TestMethod]
        public void Parameter_Wrong()
        {
            var console = new StringWriter();
            Console.SetOut(console);

            KLOC.Program.Main(new[] { "asdf" });

            var output = console.GetStringBuilder().ToString();
            string line;
            using (var reader = new StringReader(output))
            {
                line = reader.ReadLine();
                line = reader.ReadLine();
                line = reader.ReadLine();
                line = reader.ReadLine();
            }
            Assert.AreEqual("Location of source code files does not exist.", line);
        }

        [TestMethod]
        public void File_UnknownType_markdown()
        {
            var ctx = new CounterContext { InputPath = GetPath("README.md") };
            var mapper = new SourceCodeMapper();
            mapper.Map(ctx);
            new Counter().Count(ctx);

            Assert.AreEqual("Not a source file.", mapper.Message);
            //TODO: counter assertions
        }
        [TestMethod]
        public void File_UnknownType_dll()
        {
            var ctx = new CounterContext { InputPath = GetPath(@"src\KlocTests\bin\Debug\KlocTests.dll") };
            var mapper = new SourceCodeMapper();
            mapper.Map(ctx);
            new Counter().Count(ctx);

            Assert.AreEqual("Not a source file.", mapper.Message);
            //TODO: counter assertions
        }
        [TestMethod]
        public void File_Csharp()
        {
            var ctx = new CounterContext { InputPath = GetPath(@"src\KLOC\Program.cs") };
            var mapper = new SourceCodeMapper();
            mapper.Map(ctx);
            new Counter().Count(ctx);

            Assert.AreEqual(0, ctx.Projects);
            Assert.AreEqual(1, ctx.FileTypes[".cs"]);
            Assert.AreEqual(1, ctx.SourceFileCount);
            //TODO: counter assertions
        }
        [TestMethod]
        public void File_Project()
        {
            var ctx = new CounterContext { InputPath = GetPath(@"src\KLOC\KLOC.csproj") };
            var mapper = new SourceCodeMapper();
            mapper.Map(ctx);
            new Counter().Count(ctx);

            Assert.AreEqual(1, ctx.Projects);
            Assert.AreEqual(10, ctx.FileTypes[".cs"]);
            Assert.AreEqual(1, ctx.FileTypes[".config"]);
            Assert.AreEqual(11, ctx.SourceFileCount);
            //TODO: counter assertions
        }
        [TestMethod]
        public void File_Solution()
        {
            var ctx = new CounterContext { InputPath = GetPath(@"src\KLOC.sln") };
            var mapper = new SourceCodeMapper();
            mapper.Map(ctx);
            new Counter().Count(ctx);

            Assert.AreEqual(3, ctx.Projects);
            Assert.AreEqual(15, ctx.FileTypes[".cs"]);
            Assert.AreEqual(1, ctx.FileTypes[".config"]);
            Assert.AreEqual(16, ctx.SourceFileCount);
            //TODO: counter assertions
        }

        [TestMethod]
        public void Directory_NonExistent()
        {
            var console = new StringWriter();
            Console.SetOut(console);

            KLOC.Program.Main(new[] { GetPath(@"notexistentdirectory") });

            var output = console.GetStringBuilder().ToString();
            string line;
            using (var reader = new StringReader(output))
            {
                line = reader.ReadLine();
                line = reader.ReadLine();
                line = reader.ReadLine();
                line = reader.ReadLine();
            }
            Assert.AreEqual("Location of source code files does not exist.", line);
        }
        [TestMethod]
        public void Directory_WithoutKnownFiles()
        {
            var ctx = new CounterContext { InputPath = _rootPath };
            var mapper = new SourceCodeMapper();
            mapper.Map(ctx);
            new Counter().Count(ctx);

            Assert.AreEqual(0, ctx.Projects);
            Assert.AreEqual(0, ctx.SourceFileCount);
            //TODO: counter assertions
        }
        [TestMethod]
        public void Directory_GithubRepository()
        {
            Assert.Inconclusive();
            // _rootPath
        }
        [TestMethod]
        public void Directory_WithKnownFiles_Sln()
        {
            //UNDONE: distinct project and source files
            var ctx = new CounterContext { InputPath = GetPath(@"src") };
            var mapper = new SourceCodeMapper();
            mapper.Map(ctx);
            new Counter().Count(ctx);

            Assert.AreEqual(3, ctx.Projects);
            Assert.AreEqual(16, ctx.SourceFileCount);
            //TODO: counter assertions
        }
        [TestMethod]
        public void Directory_WithKnownFiles_Csproj()
        {
            var ctx = new CounterContext { InputPath = GetPath(@"src\KLOC") };
            var mapper = new SourceCodeMapper();
            mapper.Map(ctx);
            new Counter().Count(ctx);

            Assert.AreEqual(1, ctx.Projects);
            Assert.AreEqual(11, ctx.SourceFileCount);
            //TODO: counter assertions
        }
        [TestMethod]
        public void Directory_WithKnownFiles_Cs()
        {
            Assert.Inconclusive();
        }
    }
}
