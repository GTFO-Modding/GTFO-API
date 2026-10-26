using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GTFO.API.Assets;

/// <summary>
/// Read-Only Dictionary of <see cref="AssetBundleReference"/>
/// </summary>
public sealed class AssetBundleRegistry : IEnumerable<KeyValuePair<string, AssetBundleReference>>
{
    private readonly ConcurrentDictionary<string, AssetBundleReference> _Registry = [];

    /// <summary>
    /// Collection of <see cref="AssetBundleReference"/>
    /// </summary>
    public ICollection<AssetBundleReference> AssetBundles => _Registry.Values;

    /// <summary>
    /// Collection of AssetName
    /// </summary>
    public ICollection<string> AssetBundleNames => _Registry.Keys;

    /// <summary>
    /// KeyValue Pairs
    /// </summary>
    public IEnumerable<KeyValuePair<string, AssetBundleReference>> KeyValues => _Registry;

    internal AssetBundleRegistry() { }

    /// <summary>
    /// Indexer for Accessing <see cref="AssetBundleReference"/> Instance
    /// </summary>
    /// <param name="assetBundleName">AssetBundle Name to search</param>
    /// <returns><see cref="AssetBundleReference"/> of given Name</returns>
    public AssetBundleReference this[string assetBundleName]
    {
        get => _Registry[Key(assetBundleName)];
        internal set => _Registry[Key(assetBundleName)] = value;
    }

    /// <summary>
    /// Try Get <see cref="AssetBundleReference"/> Instance of given Name
    /// </summary>
    /// <param name="assetBundleName">AssetBundle Name to search</param>
    /// <param name="assetBundle"><see cref="AssetBundleReference"/> of given Name</param>
    /// <returns><see langword="true"/> if found one, otherwise <see langword="false"/></returns>
    public bool TryGet(string assetBundleName, out AssetBundleReference assetBundle)
    {
        return _Registry.TryGetValue(Key(assetBundleName), out assetBundle);
    }

    /// <summary>
    /// Check if AssetBundle Name is registered
    /// </summary>
    /// <param name="assetBundleName">AssetBundle Name to search</param>
    /// <returns><see langword="true"/> if found one, otherwise <see langword="false"/></returns>
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

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, AssetBundleReference>> GetEnumerator()
    {
        return _Registry.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
