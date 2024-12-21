using System;
using System.Collections.Generic;
using System.Linq;

namespace KLOC8;

internal class ProjectDirectory(string directoryPath, IFilter filter, IDisk disk) : PathEnumerable
{
    public override IEnumerator<string> GetEnumerator()
    {
        var count = 0;
        var lastProgressAt = DateTime.Now;
        // Enumerate all files in depth
        foreach (var path in new DirectoryEnumerable(directoryPath, filter, disk))
        {
            count++;
            if ((DateTime.Now - lastProgressAt).TotalMilliseconds > 100)
            {
                var width = Console.WindowWidth-10;

                if (width > 20)
                {
                    var text = count + "  " + path;
                    if (text.Length > width)
                        text = text.Substring(0, width - 3) + "...";
                    text = text.PadRight(width+9, ' ');
                    Console.Write(text + "\r");
                }
                lastProgressAt = DateTime.Now;
            }
            yield return path;
        }
    }

    public string[] GetDirectories()
    {
        return new DirectoryEnumerable(directoryPath, filter, disk).GetEnabledDirectories();
    }

    private class DirectoryEnumerable(string path, IFilter filter, IDisk disk) : PathEnumerable
    {
        public override IEnumerator<string> GetEnumerator()
        {
            foreach (var dir in GetEnabledDirectories())
            {
                foreach (var file in new DirectoryEnumerable(dir, filter, disk))
                    yield return file;
            }

            foreach (var file in disk.Directory_GetFiles(path))
                if (filter.IsEnabledFile(file))
                    yield return file;
        }

        /// <summary>
        /// Returns all enabled child directories.
        /// </summary>
        public string[] GetEnabledDirectories()
        {
            return disk.Directory_GetDirectories(path)
                .Where(filter.IsEnabledDirectory)
                .ToArray();
        }
    }
}