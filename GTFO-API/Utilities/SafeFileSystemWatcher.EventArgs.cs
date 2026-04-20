using System.IO;
using System.Text;

namespace GTFO.API.Utilities;

/// <summary>
/// Type of File Event
/// </summary>
public enum FileEventType
{
    /// <summary>
    /// File has created
    /// </summary>
    Created,
    /// <summary>
    /// File has deleted
    /// </summary>
    Deleted,
    /// <summary>
    /// File has renamed
    /// </summary>
    Renamed,
    /// <summary>
    /// File has changed
    /// </summary>
    Changed
}

/// <summary>
/// SafeFileSystemWatcher Event Arguments
/// </summary>
public record FileEventArgs
{
    /// <summary>
    /// Triggered Event Type
    /// </summary>
    public FileEventType Type { get; init; }

    /// <summary>
    /// Full Path to File
    /// </summary>
    public string FullPath { get; init; }

    /// <summary>
    /// Name of the File (Does not include path)
    /// </summary>
    public string FileName { get; init; }

    /// <summary>
    /// Read Text Content of the Target File
    /// </summary>
    /// <param name="encoding">Encoding to use. <see cref="Encoding.Default"/> If <see langword="null"/></param>
    /// <returns>Full Text Content of the File</returns>
    public string ReadContent(Encoding encoding = null)
    {
        using FileStream fs = OpenReadStream();
        using StreamReader sr = new StreamReader(fs, encoding ?? Encoding.Default);
        return sr.ReadToEnd();
    }

    /// <summary>
    /// Open a Read-Only FileStream of the Target File
    /// </summary>
    /// <returns>Opened FileStream</returns>
    public FileStream OpenReadStream()
    {
        return new FileStream(FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    }

    /// <summary>
    /// Check If Target File is Locked by other Process
    /// </summary>
    /// <returns>true if it's locked</returns>
    public bool IsFileLocked()
    {
        try
        {
            using FileStream fs = new FileStream(FullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return false;
        }
        catch
        {
            return true;
        }
    }
}

/// <summary>
/// SafeFileSystemWatcher Event Arguments
/// </summary>
public sealed record FileRenamedEventArgs : FileEventArgs
{
    /// <summary>
    /// Full Path to Old File<br/>
    /// </summary>
    public string OldFullPath { get; init; }

    /// <summary>
    /// Name of the Old File<br/>
    /// </summary>
    public string OldFileName { get; init; }
}

