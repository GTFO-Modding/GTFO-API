using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GTFO.API.Assets;

/// <summary>
/// Read-Only Dictionary of <see cref="AssetBundleReference"/>
/// </summary>
public sealed class AssetRegistry : IEnumerable<KeyValuePair<string, AssetReference>>
{
    private readonly ConcurrentDictionary<string, AssetReference> _Registry = [];

    /// <summary>
    /// Collection of <see cref="AssetReference"/>
    /// </summary>
    public ICollection<AssetReference> Assets => _Registry.Values;

    /// <summary>
    /// Collection of AssetName
    /// </summary>
    public ICollection<string> AssetNames => _Registry.Keys;

    /// <summary>
    /// KeyValue Pairs
    /// </summary>
    public IEnumerable<KeyValuePair<string, AssetReference>> KeyValues => _Registry;

    internal AssetRegistry() { }

    /// <summary>
    /// Indexer for Accessing <see cref="AssetReference"/> Instance
    /// </summary>
    /// <param name="assetName">Asset Name to search</param>
    /// <returns><see cref="AssetReference"/> of given Name</returns>
    public AssetReference this[string assetName]
    {
        get => _Registry[Key(assetName)];
        internal set => _Registry[Key(assetName)] = value;
    }

    /// <summary>
    /// Try Get <see cref="AssetReference"/> Instance of given Name
    /// </summary>
    /// <param name="assetName">Asset Name to search</param>
    /// <param name="asset"><see cref="AssetReference"/> of given Name</param>
    /// <returns><see langword="true"/> if found one, otherwise <see langword="false"/></returns>
    public bool TryGet(string assetName, out AssetReference asset)
    {
        return _Registry.TryGetValue(Key(assetName), out asset);
    }

    /// <summary>
    /// Check if Asset Name is registered
    /// </summary>
    /// <param name="assetName">Asset Name to search</param>
    /// <returns><see langword="true"/> if found one, otherwise <see langword="false"/></returns>
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

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, AssetReference>> GetEnumerator()
    {
        return _Registry.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
