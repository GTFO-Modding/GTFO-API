using System;
using System.Diagnostics;
using GTFO.API.Assets.AssetSources;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace GTFO.API.Assets;

/// <summary>
/// Indirect Managed Reference to Asset
/// </summary>
public sealed class AssetReference
{
    private static readonly Stopwatch s_LoadSW = new();

    /// <summary>
    /// <see cref="AssetBundleReference"/> that owns this Asset; <see langword="null"/> If it's not from AssetBundle
    /// </summary>
    public AssetBundleReference AssetSourceBundle { get; private set; }

    /// <summary>
    /// Name of the AssetSource
    /// <para>
    /// - If Loaded by AssetBundle: <code>AssetBundle (*BundleName*)</code><br/>
    /// - If Loaded by <see cref="AssetAPI.RegisterAsset"/>: <code>External</code><br/>
    /// </para>
    /// </summary>
    public string AssetSourceName => _AssetSource.SourceName;

    /// <summary>
    /// Name of the Asset (Full Path)
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Check if Asset has loaded
    /// </summary>
    public bool IsLoaded => _AssetSource.LoadedAsset != null;

    /// <summary>
    /// Get Referencing Asset Instance
    /// <para>
    /// If it's not pre-loaded via <see cref="Load"/> or <see cref="LoadAsync"/>, it will trigger JIT loading if owning <see cref="AssetBundleReference"/> allows.
    /// </para>
    /// </summary>
    public UnityObject Asset => GetAsset();

    private readonly IAssetSource _AssetSource;

    internal AssetReference(string assetName, AssetBundleReference ownerBundle)
    {
        Name = assetName;
        AssetSourceBundle = ownerBundle;
        _AssetSource = new AssetSource_AssetBundle(assetName, ownerBundle);
    }

    internal AssetReference(string assetName, UnityObject value)
    {
        Name = assetName;
        AssetSourceBundle = null;
        _AssetSource = new AssetSource_External(assetName, value);
    }

    /// <summary>
    /// Load Referenced Asset (Blocking)
    /// </summary>
    public void Load()
    {
        if (_AssetSource.CanManualLoad)
        {
            _AssetSource.Load();
        }

    }

    /// <summary>
    /// Load Referenced Asset (Non-Blocking)
    /// </summary>
    /// <param name="onLoaded">Callback when Asset has loaded</param>
    public void LoadAsync(Action onLoaded = null)
    {
        if (_AssetSource.CanManualLoad)
        {
            _AssetSource.LoadAsync(onLoaded);
        }
    }

    /// <summary>
    /// Unload Asset
    /// </summary>
    public void Unload()
    {
        if (_AssetSource.CanManualUnload)
        {
            _AssetSource.Unload();
        }
    }

    /// <inheritdoc cref="Asset"/>
    /// <returns><see cref="UnityEngine.Object"/> Instance</returns>
    public UnityObject GetAsset()
    {
        if (_AssetSource.LoadedAsset != null)
            return _AssetSource.LoadedAsset;

        if (!_AssetSource.CanManualLoad)
            return null;

        s_LoadSW.Restart();
        _AssetSource.Load();
        s_LoadSW.Stop();

        APILogger.Debug($"AssetJIT", $"Asset '{Name}' from '{_AssetSource.SourceName}' was JIT loaded in {s_LoadSW.ElapsedMilliseconds}ms!");

        return _AssetSource.LoadedAsset;
    }

    /// <summary>
    /// Cast <see cref="Asset"/>
    /// </summary>
    /// <typeparam name="TAsset">Type to Cast, Should be <see cref="UnityObject"/></typeparam>
    /// <returns>Casted Object</returns>
    public TAsset AssetAs<TAsset>() where TAsset : UnityObject
    {
        return Asset.Cast<TAsset>();
    }

    /// <summary>
    /// Implicit Conversion for <see cref="Name"/>
    /// </summary>
    /// <param name="assetInfo">AssetInfo to Convert</param>
    public static implicit operator string(AssetReference assetInfo) => assetInfo.Name;

    /// <summary>
    /// Implicit Conversion for <see cref="Asset"/>
    /// </summary>
    /// <param name="assetInfo">AssetInfo to Convert</param>
    public static implicit operator UnityObject(AssetReference assetInfo) => assetInfo.Asset;

    /// <summary>
    /// Implicit Conversion for <see cref="Asset"/> to <see cref="GameObject"/>
    /// </summary>
    /// <param name="assetInfo">AssetInfo to Convert</param>
    public static implicit operator GameObject(AssetReference assetInfo) => assetInfo.AssetAs<GameObject>();

    /// <summary>
    /// Implicit Conversion for <see cref="Asset"/> to <see cref="Shader"/>
    /// </summary>
    /// <param name="assetInfo">AssetInfo to Convert</param>
    public static implicit operator Shader(AssetReference assetInfo) => assetInfo.AssetAs<Shader>();

    /// <summary>
    /// Implicit Conversion for <see cref="Asset"/> to <see cref="Material"/>
    /// </summary>
    /// <param name="assetInfo">AssetInfo to Convert</param>
    public static implicit operator Material(AssetReference assetInfo) => assetInfo.AssetAs<Material>();

    /// <summary>
    /// Implicit Conversion for <see cref="Asset"/> to <see cref="Texture2D"/>
    /// </summary>
    /// <param name="assetInfo">AssetInfo to Convert</param>
    public static implicit operator Texture2D(AssetReference assetInfo) => assetInfo.AssetAs<Texture2D>();

    /// <summary>
    /// Implicit Conversion for <see cref="Asset"/> to <see cref="Texture3D"/>
    /// </summary>
    /// <param name="assetInfo">AssetInfo to Convert</param>
    public static implicit operator Texture3D(AssetReference assetInfo) => assetInfo.AssetAs<Texture3D>();
}
