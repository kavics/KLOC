using KLOC8;
using NSubstitute;

namespace Kloc8Tests
{
    internal class TestFileDescriptor(string? path, string? fileContent) : FileDescriptor(path ?? "null")
    {
        public override long Length => throw new NotImplementedException();
        public override void Delete() => throw new NotImplementedException();
        public override Stream OpenRead() => throw new NotImplementedException();
        public override StreamReader GetReader()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(fileContent);
            writer.Flush();
            stream.Position = 0;
            var reader = new StreamReader(stream);
            return reader;
        }
        public override StreamWriter GetWriter() => throw new NotImplementedException();
        public override bool Exists() => path != null;
    }

    [TestClass]
    public class KlocIgnoreTests
    {
        [TestMethod]
        public void UT_KlocIgnore_OneChain_SubFilterExistence()
        {
            var directoryPaths = new[]
            {
                @"X:\Project",
                @"X:\Project\src",
                @"X:\Project\src\Folder1",
                @"X:\Project\src\Folder1\SubFolder1",
                @"X:\Project\src\Folder1\SubFolder1\SubFolder2",
            };
            var filePaths = new[]
            {
                @"X:\Project\src\Folder1\.klocignore",
                @"X:\Project\src\Folder1\SubFolder1\File1.txt",
                @"X:\Project\src\Folder1\SubFolder1\.klocignore",
                @"X:\Project\src\Folder1\SubFolder1\SubFolder2\File2.txt",
            };

            var fileDescriptor1 = new TestFileDescriptor(@"X:\Project\src\Folder1\.klocignore", "*.xxx");
            var fileDescriptor2 = new TestFileDescriptor(@"X:\Project\src\Folder1\.klocignore", "*.yyy");

            var disk = Substitute.For<IDisk>();
            disk.CreateFileDescriptor(Arg.Any<string>()).Returns(new TestFileDescriptor(null, null));
            disk.CreateFileDescriptor(@"X:\Project\src\Folder1\.klocignore").Returns(fileDescriptor1);
            disk.CreateFileDescriptor(@"X:\Project\src\Folder1\SubFolder1\.klocignore").Returns(fileDescriptor2);

            var filter = new KlocIgnoreFileFilter(disk);

            // ACT
            var filteredPaths = directoryPaths
                .Where(path => filter.IsEnabledDirectory(path))
                .Union(filePaths.Where(path => filter.IsEnabledFile(path)))
                .OrderBy(path => path)
                .ToArray();

            // ASSERT
            var subFilters = filter.SubFilters.OrderBy(x => x.path).ToArray();
            Assert.AreEqual(2, subFilters.Length);
            Assert.AreEqual(@"X:\Project\src\Folder1", subFilters[0].path);
            Assert.AreEqual(@"X:\Project\src\Folder1\.klocignore", subFilters[0].filter.FilePath);
            Assert.AreEqual(@"X:\Project\src\Folder1\SubFolder1", subFilters[1].path);
            Assert.AreEqual(@"X:\Project\src\Folder1\SubFolder1\.klocignore", subFilters[1].filter.FilePath);
        }
        [TestMethod]
        public void UT_KlocIgnore_OneChain_SubFilterWorksForFiles()
        {
            var directoryPaths = new[]
            {
                @"X:\Project",
                @"X:\Project\src",
                @"X:\Project\src\Folder1",
                @"X:\Project\src\Folder1\SubFolder1",
                @"X:\Project\src\Folder1\SubFolder1\SubFolder2",
            };
            var filePaths = new[]
            {
                @"X:\Project\src\Folder1\.klocignore",
                @"X:\Project\src\Folder1\SubFolder1\File1.txt",
                @"X:\Project\src\Folder1\SubFolder1\File1.xxx",
                @"X:\Project\src\Folder1\SubFolder1\File1.yyy",
                @"X:\Project\src\Folder1\SubFolder1\SubFolder2\.klocignore",
                @"X:\Project\src\Folder1\SubFolder1\SubFolder2\File2.txt",
                @"X:\Project\src\Folder1\SubFolder1\SubFolder2\File2.xxx",
                @"X:\Project\src\Folder1\SubFolder1\SubFolder2\File2.yyy",
            };
            var expected = new[]
            {
                @"X:\Project",
                @"X:\Project\src",
                @"X:\Project\src\Folder1",
                @"X:\Project\src\Folder1\.klocignore",
                @"X:\Project\src\Folder1\SubFolder1",
                @"X:\Project\src\Folder1\SubFolder1\File1.txt",
                @"X:\Project\src\Folder1\SubFolder1\File1.yyy",
                @"X:\Project\src\Folder1\SubFolder1\SubFolder2",
                @"X:\Project\src\Folder1\SubFolder1\SubFolder2\.klocignore",
                @"X:\Project\src\Folder1\SubFolder1\SubFolder2\File2.txt",
                @"X:\Project\src\Folder1\SubFolder1\SubFolder2\File2.xxx",
            };

            var fileDescriptor1 = new TestFileDescriptor(@"X:\Project\src\Folder1\.klocignore", "*.xxx");
            var fileDescriptor2 = new TestFileDescriptor(@"X:\Project\src\Folder1\SubFolder1\.klocignore", "*.yyy");

            var disk = Substitute.For<IDisk>();
            disk.CreateFileDescriptor(Arg.Any<string>()).Returns(new TestFileDescriptor(null, null));
            disk.CreateFileDescriptor(@"X:\Project\src\Folder1\.klocignore").Returns(fileDescriptor1);
            disk.CreateFileDescriptor(@"X:\Project\src\Folder1\SubFolder1\SubFolder2\.klocignore").Returns(fileDescriptor2);

            var filter = new KlocIgnoreFileFilter(disk);

            // ACT
            var filteredPaths = directoryPaths
                .Where(path => filter.IsEnabledDirectory(path))
                .Union(filePaths.Where(path => filter.IsEnabledFile(path)))
                .OrderBy(path => path)
                .ToArray();

            // ASSERT
            Assert.AreEqual(string.Join(Environment.NewLine, expected), string.Join(Environment.NewLine, filteredPaths));
        }
    }
}
