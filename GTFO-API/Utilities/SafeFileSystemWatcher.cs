using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using BepInEx.Configuration;
using GTFO.API.Utilities.Impl;

namespace GTFO.API.Utilities
{
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

    /// <summary>
    /// Wrapper of FileSystemWatcher Which Features:
    /// <list>
    /// - Ensuring Events Invocation Thread to be Unity's Main Thread<br/>
    /// - Debounces Multiple Invocation for Single Action<br/>
    /// - Ensure the File Lock Safety
    /// </list>
    /// </summary>
    public sealed class SafeFileSystemWatcher : IDisposable
    {
        /// <inheritdoc cref="FileSystemWatcher.Filters"/>
        public Collection<string> Filters => _Watcher.Filters;

        /// <inheritdoc cref="FileSystemWatcher.Filter"/>
        public string Filter
        {
            get => _Watcher.Filter;
            set => _Watcher.Filter = value;
        }

        /// <inheritdoc cref="FileSystemWatcher.Path"/>
        public string Path
        {
            get => _Watcher.Path;
            set => _Watcher.Path = value;
        }

        /// <summary>
        /// Gets or sets the type of changes to watch for.
        /// <br/>
        /// - Default: <see cref="NotifyFilters.FileName"/> | <see cref="NotifyFilters.LastWrite"/> | <see cref="NotifyFilters.CreationTime"/>
        /// </summary>
        public NotifyFilters NotifyFilter
        {
            get => _Watcher.NotifyFilter;
            set => _Watcher.NotifyFilter = value;
        }

        /// <inheritdoc cref="FileSystemWatcher.IncludeSubdirectories"/>
        public bool IncludeSubDir
        {
            get => _Watcher.IncludeSubdirectories;
            set => _Watcher.IncludeSubdirectories = value;
        }

        /// <summary>
        /// Should FileSystemWatcher Listen to Events?
        /// </summary>
        public bool Listening
        {
            get => _Watcher.EnableRaisingEvents;
            set => _Watcher.EnableRaisingEvents = value;
        }

        /// <summary>
        /// Should Event be Re-Queued when file is locked?<br/>
        /// - Default: <see langword="true"/>
        /// </summary>
        public bool RetryOnLocked { get; set; } = true;

        /// <summary>
        /// Event when File has Changed
        /// </summary>
        public event Action<FileEventArgs> OnChanged;

        /// <summary>
        /// Event when File has Created
        /// </summary>
        public event Action<FileEventArgs> OnCreated;

        /// <summary>
        /// Event when File has Deleted
        /// </summary>
        public event Action<FileEventArgs> OnDeleted;

        /// <summary>
        /// Event when File has Renamed
        /// </summary>
        public event Action<FileRenamedEventArgs> OnRenamed;

        private readonly FileSystemWatcher _Watcher = new();
        private readonly ConcurrentQueue<FileEventArgs> _QueuedEvents = [];
        private readonly HashSet<FileEventArgs> _HandledEventInFrame = [];
        private readonly Queue<FileEventArgs> _RetryQueue = [];

        private bool _DisposedValue;

        private SafeFileSystemWatcher() { }

        /// <summary>
        /// Create <see cref="SafeFileSystemWatcher"/> Instance
        /// </summary>
        /// <param name="path">Path to Watch</param>
        /// <param name="filters">List of Filter to Use</param>
        /// <param name="includeSubDir"></param>
        /// <returns>New <see cref="SafeFileSystemWatcher"/> Instance with given setting</returns>
        public static SafeFileSystemWatcher Create(string path, string[] filters = null, bool includeSubDir = false)
        {
            var sfsw = new SafeFileSystemWatcher
            {
                Path = path,
                IncludeSubDir = includeSubDir,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
            };

            if (filters != null && filters.Length > 0)
            {
                sfsw.Filters.Clear();
                foreach (var filter in filters)
                {
                    sfsw.Filters.Add(filter);
                }
            }

            sfsw._Watcher.Created += (sender, e) => sfsw.EnqueueEventArg(FileEventType.Created, e);
            sfsw._Watcher.Deleted += (sender, e) => sfsw.EnqueueEventArg(FileEventType.Deleted, e);
            sfsw._Watcher.Changed += (sender, e) => sfsw.EnqueueEventArg(FileEventType.Changed, e);
            sfsw._Watcher.Renamed += (sender, e) => sfsw.EnqueueEventArg(FileEventType.Renamed, e);
            sfsw._Watcher.Error += (sender, e) =>
            {
                APILogger.Error(nameof(SafeFileSystemWatcher), $"Path: '{path}' error was reported! - {e.GetException()}");
            };

            sfsw.Listening = true;
            APILogger.Verbose(nameof(SafeFileSystemWatcher), $"Created Watcher; Path: '{path}', Filter: {string.Join(", ", filters)}");
            SafeFileSystemWatcherUpdater_Impl.AddWatcher(sfsw);
            return sfsw;
        }

        /// <summary>
        /// Create <see cref="SafeFileSystemWatcher"/> Instance From a <see cref="ConfigFile"/> and Automatically Reloads Config upon Change<br/>
        ///  - Events are recommended to be handled by '<see cref="ConfigFile.ConfigReloaded"/>' or '<see cref="ConfigEntry{T}.SettingChanged"/>'
        /// </summary>
        /// <param name="configFile">Config to Watch</param>
        /// <returns>New <see cref="SafeFileSystemWatcher"/> Instance with given setting</returns>
        public static SafeFileSystemWatcher Create(ConfigFile configFile)
        {
            var fullPath = configFile.ConfigFilePath;
            var fileName = System.IO.Path.GetFileName(fullPath);
            var pathName = System.IO.Path.GetDirectoryName(fullPath);
            var sfsw = Create(pathName, [fileName], false);
            sfsw.OnChanged += (e) =>
            {
                configFile.Reload();
            };

            return sfsw;
        }

        private void EnqueueEventArg(FileEventType type, FileSystemEventArgs arg)
        {
            if (type == FileEventType.Renamed)
            {
                var renamed = (RenamedEventArgs)arg;
                _QueuedEvents.Enqueue(new FileRenamedEventArgs()
                {
                    Type = type,
                    FullPath = renamed.FullPath,
                    FileName = System.IO.Path.GetFileName(renamed.FullPath),
                    OldFullPath = renamed.OldFullPath,
                    OldFileName = System.IO.Path.GetFileName(renamed.OldFullPath)
                });
            }
            else
            {
                _QueuedEvents.Enqueue(new()
                {
                    Type = type,
                    FullPath = arg.FullPath,
                    FileName = System.IO.Path.GetFileName(arg.FullPath),
                });
            }
        }

        internal void HandleEvents()
        {
            lock (_QueuedEvents)
            {
                if (_QueuedEvents.Count <= 0)
                    return;

                _HandledEventInFrame.Clear();
                while (_QueuedEvents.TryDequeue(out var arg))
                {
                    if (_HandledEventInFrame.Contains(arg))
                    {
                        continue;
                    }

                    if (RetryOnLocked && (arg.Type != FileEventType.Deleted && arg.IsFileLocked()))
                    {
                        _RetryQueue.Enqueue(arg);
                        continue;
                    }

                    switch (arg.Type)
                    {
                        case FileEventType.Created: OnCreated?.Invoke(arg); break;
                        case FileEventType.Deleted: OnDeleted?.Invoke(arg); break;
                        case FileEventType.Renamed: OnRenamed?.Invoke((FileRenamedEventArgs)arg); break;
                        case FileEventType.Changed: OnChanged?.Invoke(arg); break;
                    }

                    _HandledEventInFrame.Add(arg);
                }

                while (_RetryQueue.TryDequeue(out var arg))
                {
                    _QueuedEvents.Enqueue(arg);
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_DisposedValue)
            {
                if (disposing)
                {
                    _Watcher.Dispose();
                    OnCreated = null;
                    OnDeleted = null;
                    OnRenamed = null;
                    OnChanged = null;

                    _QueuedEvents.Clear();
                    _HandledEventInFrame.Clear();
                    _RetryQueue.Clear();
                    SafeFileSystemWatcherUpdater_Impl.RemoveWatcher(this);
                }
                _DisposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
