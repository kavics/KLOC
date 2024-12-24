using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace KLOC8;

public interface IDisk
{
    string Path_GetFullPath(string path);
    string Path_GetFileName(string path);
    string Path_GetExtension(string path);
    IEnumerable<string> Directory_GetDirectories(string path);
    IEnumerable<string> Directory_GetFiles(string path);

    FileDescriptor CreateFileDescriptor(string path);
}

public abstract class FileDescriptor(string path)
{
    protected readonly string Path = path;

    public abstract long Length { get; }

    public abstract void Delete();

    public abstract Stream OpenRead();

    public abstract StreamReader GetReader();
    public abstract StreamWriter GetWriter();

    public abstract bool Exists();
}

internal class FileSystemFileDescriptor(string path) : FileDescriptor(path)
{
    private readonly FileInfo _fileInfo = new(path);
    public override long Length => _fileInfo.Length;
    public override void Delete() => new FileInfo(Path).Delete();
    public override Stream OpenRead() => _fileInfo.OpenRead();
    public override StreamReader GetReader() => new(Path);
    public override StreamWriter GetWriter() => new(Path, false);
    public override bool Exists() => _fileInfo.Exists;
}

public class Disk : IDisk
{
    public string Path_GetFullPath(string path) => System.IO.Path.GetFullPath(path);
    public string Path_GetFileName(string path) => System.IO.Path.GetFileName(path);
    public string Path_GetExtension(string path) => System.IO.Path.GetExtension(path);
    public IEnumerable<string> Directory_GetDirectories(string path) => System.IO.Directory.GetDirectories(path);
    public IEnumerable<string> Directory_GetFiles(string path) => System.IO.Directory.GetFiles(path);
    public FileDescriptor CreateFileDescriptor(string path) => new FileSystemFileDescriptor(path);
}