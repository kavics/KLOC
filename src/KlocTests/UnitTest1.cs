using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace KlocTests
{
    [TestClass]
    public class UnitTest1
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
        public void MissingParameter()
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
        public void WrongParameter()
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
        public void CsharpFile()
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
        public void ProjectFile()
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
        public void SolutionFile()
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

    }
}
