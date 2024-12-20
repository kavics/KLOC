using KLOC8;
using static System.Net.Mime.MediaTypeNames;

namespace Kloc8Tests;

[TestClass]
public class CounterTests
{
    [TestMethod]
    public void Counter_OneTextFile()
    {
        var disk = new TestDisk();
        var counter = new Counter(disk);
        var stream = GetStream(@"
Line1
Longest line
Line 3
");
        var path = "test.txt";

        // ACT
        counter.CountOfLines(stream, path);

        // ASSERT
        Assert.AreEqual(4, counter.Lines);
        Assert.AreEqual(1, counter.EmptyLines);
        Assert.AreEqual(12, counter.LongestLineLength);
        Assert.AreEqual(path, counter.LongestLineFile);
        Assert.AreEqual(3, counter.LongestLineLine);
        Assert.AreEqual(31, counter.Bytes);
        Assert.AreEqual(1, counter.FileTypes.Count);
        Assert.AreEqual(1, counter.FileTypes[".txt"]);
        Assert.AreEqual(1, counter.LinesPerFileTypes.Count);
        Assert.AreEqual(4, counter.LinesPerFileTypes[".txt"]);
    }

    private Stream GetStream(string sourceCode)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(sourceCode);
        writer.Flush();
        stream.Position = 0L;
        return stream;
    }
}

public class TestDisk : IDisk
{
    public string Path_GetFullPath(string path)
    {
        throw new NotImplementedException();
    }

    public string Path_GetFileName(string path)
    {
        throw new NotImplementedException();
    }

    public string Path_GetExtension(string path)
    {
        return Path.GetExtension(path);
    }

    public IEnumerable<string> Directory_GetDirectories(string path)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> Directory_GetFiles(string path)
    {
        throw new NotImplementedException();
    }

    public FileDescriptor CreateFileDescriptor(string path)
    {
        throw new NotImplementedException();
    }
}