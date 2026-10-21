using System;
using System.Diagnostics;
using GTFO.API.Assets.AssetSources;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace GTFO.API.Assets;

public sealed class AssetReference
{
    private static readonly Stopwatch s_LoadSW = new();

    public AssetBundleReference AssetSourceBundle { get; private set; }
    public string AssetSourceName => _AssetSource.SourceName;
    public string Name { get; private set; }
    public bool IsLoaded => _AssetSource.LoadedAsset != null;

    public UnityObject Asset
    {
        get
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
    }

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

    public void Load()
    {
        if (_AssetSource.CanManualLoad)
        {
            _AssetSource.Load();
        }

    }

    public void LoadAsync(Action onLoaded = null)
    {
        if (_AssetSource.CanManualLoad)
        {
            _AssetSource.LoadAsync(onLoaded);
        }
    }

    public void Unload()
    {
        if (_AssetSource.CanManualUnload)
        {
            _AssetSource.Unload();
        }
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
