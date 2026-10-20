using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GTFO.API.Assets;

public sealed class AssetRegistry
{
    private readonly ConcurrentDictionary<string, AssetReference> _Registry = [];

    public ICollection<AssetReference> Assets => _Registry.Values;
    public ICollection<string> AssetNames => _Registry.Keys;
    public IEnumerable<KeyValuePair<string, AssetReference>> KeyValues => _Registry;

    public AssetReference this[string assetName]
    {
        get => _Registry[Key(assetName)];
        internal set => _Registry[Key(assetName)] = value;
    }

    public bool TryGet(string assetName, out AssetReference asset)
    {
        return _Registry.TryGetValue(Key(assetName), out asset);
    }

    public bool Contains(string assetName)
    {
        return _Registry.ContainsKey(Key(assetName));
    }

    internal bool Remove(string assetName)
    {
        return _Registry.Remove(Key(assetName), out _);
    }

    private static string Key(string assetName)
    {
        if (string.IsNullOrWhiteSpace(assetName))
            throw new ArgumentException($"Asset Name can't be whitespace or null", nameof(assetName));

        return assetName.ToUpper();
    }
}
