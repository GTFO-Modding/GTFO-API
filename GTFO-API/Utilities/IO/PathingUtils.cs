using System;
using System.IO;

namespace GTFO.API.Utilities.IO;

internal static class PathingUtils
{
    public static bool IsFileInDir(string filePath, string dirPath)
    {
        dirPath = Path.GetFullPath(dirPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        filePath = Path.GetFullPath(filePath);
        return filePath.StartsWith(dirPath, StringComparison.OrdinalIgnoreCase);
    }
}
