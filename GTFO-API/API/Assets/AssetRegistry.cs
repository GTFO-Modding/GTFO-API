using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AssetsTools.NET.Extra;

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

    /// <summary>
    /// Look for <see cref="AssetReference"/> that has specified AssetType.
    /// </summary>
    /// <param name="assetType">AssetType to search</param>
    /// <returns>
    /// <list type="bullet">
    /// <item>All Assets that matching the condition</item>
    /// <item>Empty if <paramref name="assetType"/> is <see langword="null"/> or <see cref="string.Empty"/></item>
    /// </list>
    /// </returns>
    public IEnumerable<AssetReference> FindByType(string assetType)
    {
        if (string.IsNullOrWhiteSpace(assetType))
            return [];

        return Assets.Where(a => a.Metadata.AssetType.Equals(assetType, StringComparison.Ordinal));
    }

    /// <summary>
    /// Look for <see cref="AssetReference"/> that has specified <see cref="AssetClassID"/>
    /// </summary>
    /// <param name="classID">ClassID to search</param>
    /// <returns>All Assets that matching the condition</returns>
    public IEnumerable<AssetReference> FindByType(AssetClassID classID)
    {
        var classIDName = classID.ToString();
        return Assets.Where(a => a.Metadata.AssetType.Equals(classIDName, StringComparison.Ordinal));
    }

    /// <summary>
    /// Look for <see cref="AssetReference"/> that has ANY of specified tags (Overlaps)
    /// </summary>
    /// <param name="searchingTags">Tags to search, tag can't be whitespace or null</param>
    /// <returns>
    /// <list type="bullet">
    /// <item>All Assets that matching the condition</item>
    /// <item>Empty if <paramref name="searchingTags"/> is <see langword="null"/> or empty</item>
    /// </list>
    /// </returns>
    public IEnumerable<AssetReference> FindByAnyTags(params string[] searchingTags) => FindByTags(searchingTags, matchAll: false);

    /// <summary>
    /// Look for <see cref="AssetReference"/> that has ALL of specified tags (Subset)
    /// </summary>
    /// <param name="searchingTags">Tags to search, tag can't be whitespace or null</param>
    /// <returns>
    /// <list type="bullet">
    /// <item>All Assets that matching the condition</item>
    /// <item>Empty if <paramref name="searchingTags"/> is <see langword="null"/> or empty</item>
    /// </list>
    /// </returns>
    public IEnumerable<AssetReference> FindByAllTags(params string[] searchingTags) => FindByTags(searchingTags, matchAll: true);

    internal IEnumerable<AssetReference> FindByTags(string[] searchingTags, bool matchAll)
    {
        if (searchingTags == null)
            return [];

        var tagsSet = new HashSet<string>(searchingTags.Where(tag => !string.IsNullOrWhiteSpace(tag)), StringComparer.OrdinalIgnoreCase);
        if (tagsSet.Count < 1)
            return [];

        if (matchAll)
            return Assets.Where(assetRef => tagsSet.IsSubsetOf(assetRef.Metadata.Tags));
        else
            return Assets.Where(assetRef => tagsSet.Overlaps(assetRef.Metadata.Tags));
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
