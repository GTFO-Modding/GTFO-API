using System;
using System.Diagnostics;
using AssetShards;
using GTFO.API.Assets.AssetSources;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace GTFO.API.Assets;

public sealed class AssetReference
{
    /// <summary>
    /// List of every custom assets. This does NOT include Vanilla assets!
    /// </summary>
    public static AssetRegistry AllAssets { get; private set; } = new();
    private static readonly Stopwatch s_LoadSW = new();

    public static bool IsAssetRegistered(string assetName)
    {
        assetName = assetName.ToUpper();
        if (AssetShardManager.s_loadedAssetsLookup.ContainsKey(assetName))
            return true;

        if (AllAssets.Contains(assetName))
            return true;

        return false;
    }

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

            if (_AssetSource is not AssetSource_AssetBundle assetSource)
                return null;

            var assetBundle = assetSource.OwnerBundle;
            if (assetBundle.Metadata.DisableJustInTimeLoading)
            {
                APILogger.Error($"AssetJIT", $"Asset '{Name}' from '{assetBundle.Name}' was requested but JIT loading is disabled!");
                return null;
            }

            s_LoadSW.Restart();
            _AssetSource.Load();
            s_LoadSW.Stop();

            APILogger.Debug($"AssetJIT", $"Asset '{Name}' from '{assetBundle.Name}' was JIT loaded in {s_LoadSW.ElapsedMilliseconds}ms!");

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
        _AssetSource.Load();
    }

    public void LoadAsync(Action onLoaded = null)
    {
        _AssetSource.LoadAsync(onLoaded);
    }

    public void Unload()
    {
        _AssetSource.Unload();
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
