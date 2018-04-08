using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace KlocTests
{
    [TestClass]
    public class ProgramTests
    {
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
        public void File_UnknownType()
        {
            Assert.Inconclusive();
            // D:\Projects\github\kavics\KLOC\README.md
            // D:\Projects\github\kavics\KLOC\src\KlocTests\bin\Debug\KlocTests.dll
        }
        [TestMethod]
        public void File_Csharp()
        {
            var console = new StringWriter();
            Console.SetOut(console);

            KLOC.Program.Main(new[] { @"D:\Projects\github\kavics\KLOC\src\KLOC\Program.cs" });

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

            KLOC.Program.Main(new[] { @"D:\Projects\github\kavics\KLOC\src\KLOC\KLOC.csproj" });

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

            KLOC.Program.Main(new[] { @"D:\Projects\github\kavics\KLOC\src\KLOC.sln" });

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

            KLOC.Program.Main(new[] { @"D:\Projects\github\kavics\KLOC1" });

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

            KLOC.Program.Main(new[] { @"D:\Projects\github\kavics" });

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
        public void Directory_GithubRepository()
        {
            Assert.Inconclusive();
            // D:\Projects\github\kavics\KLOC
        }
        [TestMethod]
        public void Directory_WithKnownFiles()
        {
            Assert.Inconclusive();
            // D:\Projects\github\kavics\KLOC\src
        }
    }
}
