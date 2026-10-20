using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GTFO.API.Assets;

public sealed class AssetBundleRegistry
{
    private readonly ConcurrentDictionary<string, AssetBundleReference> _Registry = [];

    public ICollection<AssetBundleReference> AssetBundles => _Registry.Values;
    public ICollection<string> AssetBundleNames => _Registry.Keys;
    public IEnumerable<KeyValuePair<string, AssetBundleReference>> KeyValues => _Registry;

    internal AssetBundleRegistry() { }

    public AssetBundleReference this[string assetName]
    {
        get => _Registry[Key(assetName)];
        internal set => _Registry[Key(assetName)] = value;
    }

    public bool TryGet(string assetBundleName, out AssetBundleReference assetBundle)
    {
        return _Registry.TryGetValue(Key(assetBundleName), out assetBundle);
    }

    public bool Contains(string assetBundleName)
    {
        return _Registry.ContainsKey(Key(assetBundleName));
    }

    internal bool Remove(string assetBundleName)
    {
        return _Registry.Remove(Key(assetBundleName), out _);
    }

    private static string Key(string assetBundleName)
    {
        if (string.IsNullOrWhiteSpace(assetBundleName))
            throw new ArgumentException($"AssetBundle Name can't be whitespace or null", nameof(assetBundleName));

        return assetBundleName.ToUpper();
    }
}
