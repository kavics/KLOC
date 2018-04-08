using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

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
            var console = new StringWriter();
            Console.SetOut(console);

            KLOC.Program.Main(new[] { GetPath("README.md") });

            var output = console.GetStringBuilder().ToString();
            string line = null;
            using (var reader = new StringReader(output))
            {
                line = reader.ReadLine();
                line = reader.ReadLine();
                line = reader.ReadLine();
                line = reader.ReadLine();
            }
            Assert.AreEqual("Not a source file.", line);
        }
        [TestMethod]
        public void File_UnknownType_dll()
        {
            var console = new StringWriter();
            Console.SetOut(console);

            KLOC.Program.Main(new[] { GetPath(@"src\KlocTests\bin\Debug\KlocTests.dll") });

            var output = console.GetStringBuilder().ToString();
            string line = null;
            using (var reader = new StringReader(output))
            {
                line = reader.ReadLine();
                line = reader.ReadLine();
                line = reader.ReadLine();
                line = reader.ReadLine();
            }
            Assert.AreEqual("Not a source file.", line);
        }
        [TestMethod]
        public void File_Csharp()
        {
            var console = new StringWriter();
            Console.SetOut(console);

            KLOC.Program.Main(new[] { GetPath(@"src\KLOC\Program.cs") });

            var output = console.GetStringBuilder().ToString();
            string line = null;
            var hasLine = false;
            using (var reader = new StringReader(output))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("Source files:"))
                    {
                        hasLine = true;
                        Assert.IsTrue(line.EndsWith(" 1"));
                        break;
                    }
                }
            }
            Assert.IsTrue(hasLine);
        }
        [TestMethod]
        public void File_Project()
        {
            var console = new StringWriter();
            Console.SetOut(console);

            KLOC.Program.Main(new[] { GetPath(@"src\KLOC\KLOC.csproj") });

            var output = console.GetStringBuilder().ToString();
            string line = null;
            var hasLine = false;
            using (var reader = new StringReader(output))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("Projects:"))
                    {
                        hasLine = true;
                        Assert.IsTrue(line.EndsWith(" 1"));
                        break;
                    }
                }
            }
            Assert.IsTrue(hasLine);
        }
        [TestMethod]
        public void File_Solution()
        {
            var console = new StringWriter();
            Console.SetOut(console);

            KLOC.Program.Main(new[] { GetPath(@"src\KLOC.sln") });

            var output = console.GetStringBuilder().ToString();
            string line = null;
            var hasLine = false;
            using (var reader = new StringReader(output))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("Projects:"))
                    {
                        hasLine = true;
                        Assert.IsTrue(line.EndsWith(" 2"));
                        break;
                    }
                }
            }
            Assert.IsTrue(hasLine);
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
            var console = new StringWriter();
            Console.SetOut(console);

            KLOC.Program.Main(new[] { _rootPath });

            var output = console.GetStringBuilder().ToString();
            string line = null;
            var lines = 0;
            using (var reader = new StringReader(output))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("Projects:"))
                    {
                        lines++;
                        Assert.IsTrue(line.EndsWith(" 0"));
                    }
                    if (line.StartsWith("Source files:"))
                    {
                        lines++;
                        Assert.IsTrue(line.EndsWith(" 0"));
                    }
                }
            }
            Assert.AreEqual(2, lines);
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
            var console = new StringWriter();
            Console.SetOut(console);

            KLOC.Program.Main(new[] { GetPath(@"src") });

            var output = console.GetStringBuilder().ToString();
            string line = null;
            var hasLine = false;
            using (var reader = new StringReader(output))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("Projects:"))
                    {
                        hasLine = true;
                        Assert.IsTrue(line.EndsWith(" 2"));
                        break;
                    }
                }
            }
            Assert.IsTrue(hasLine);
        }
        [TestMethod]
        public void Directory_WithKnownFiles_Csproj()
        {
            var console = new StringWriter();
            Console.SetOut(console);

            KLOC.Program.Main(new[] { GetPath(@"src\KLOC") });

            var output = console.GetStringBuilder().ToString();
            string line = null;
            var hasLine = false;
            using (var reader = new StringReader(output))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("Projects:"))
                    {
                        hasLine = true;
                        Assert.IsTrue(line.EndsWith(" 1"));
                        break;
                    }
                }
            }
            Assert.IsTrue(hasLine);
        }
        [TestMethod]
        public void Directory_WithKnownFiles_Cs()
        {
            Assert.Inconclusive();
        }
    }
}
