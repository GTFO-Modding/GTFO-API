using System.Collections.Generic;
using UnityEngine;

namespace GTFO.API.Utilities.Impl;

internal sealed class SafeFileSystemWatcherUpdater_Impl : MonoBehaviour
{
    private static readonly List<SafeFileSystemWatcher> s_WatchersToUpdate = [];

    private static SafeFileSystemWatcherUpdater_Impl s_Instance = null;

    public static SafeFileSystemWatcherUpdater_Impl Instance
    {
        get
        {
            if (s_Instance == null)
            {
                SafeFileSystemWatcherUpdater_Impl existing = FindObjectOfType<SafeFileSystemWatcherUpdater_Impl>();
                if (existing != null) s_Instance = existing;
            }
            return s_Instance;
        }
    }

    private void Update()
    {
        foreach (var watchers in s_WatchersToUpdate)
        {
            watchers.HandleEvents();
        }
    }

    public static void AddWatcher(SafeFileSystemWatcher watcher)
    {
        lock (s_WatchersToUpdate)
        {
            s_WatchersToUpdate.Add(watcher);
        }
    }

    public static void RemoveWatcher(SafeFileSystemWatcher watcher)
    {
        lock (s_WatchersToUpdate)
        {
            s_WatchersToUpdate.Remove(watcher);
        }
    }

    static SafeFileSystemWatcherUpdater_Impl()
    {
        AssetAPI.OnStartupAssetsLoaded += OnAssetsLoaded;
    }

    private static void OnAssetsLoaded()
    {
        if (s_Instance != null) return;

        GameObject dispatcher = new();
        SafeFileSystemWatcherUpdater_Impl updaterComp = dispatcher.AddComponent<SafeFileSystemWatcherUpdater_Impl>();
        dispatcher.name = "GTFO-API SafeFileSystemWatcher Updater";
        dispatcher.hideFlags = HideFlags.HideAndDontSave;
        GameObject.DontDestroyOnLoad(dispatcher);

        s_Instance = updaterComp;
    }
}
