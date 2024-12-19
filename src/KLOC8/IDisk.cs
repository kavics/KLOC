using System.Collections.Generic;

namespace KLOC8;

public interface IDisk
{
    string Path_GetFullPath(string path);
    string Path_GetFileName(string path);
    string Path_GetExtension(string path);
    IEnumerable<string> Directory_GetDirectories(string path);
    IEnumerable<string> Directory_GetFiles(string path);
}

public class Disk : IDisk
{
    public string Path_GetFullPath(string path) => System.IO.Path.GetFullPath(path);
    public string Path_GetFileName(string path) => System.IO.Path.GetFileName(path);
    public string Path_GetExtension(string path) => System.IO.Path.GetExtension(path);
    public IEnumerable<string> Directory_GetDirectories(string path) => System.IO.Directory.GetDirectories(path);
    public IEnumerable<string> Directory_GetFiles(string path) => System.IO.Directory.GetFiles(path);
}