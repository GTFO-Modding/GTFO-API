using System;
using System.Collections.Generic;
using System.Diagnostics;
using GTFO.API.Assets.AssetBundleSources;
using GTFO.API.Assets.Profiles;
using UnityEngine;

namespace GTFO.API.Assets;

/// <summary>
/// Indirect Managed Reference to <see cref="AssetBundle"/>
/// </summary>
public sealed class AssetBundleReference
{
    private static readonly Stopwatch s_LoadSW = new();

    /// <summary>
    /// Metadata of given <see cref="AssetBundle"/>
    /// </summary>
    public AssetBundleMetadata Metadata { get; private set; }

    /// <summary>
    /// <see cref="AssetBundleProfile"/> to use
    /// </summary>
    public AssetBundleProfile Profile
    {
        get => _Profile;
        set
        {
            if (_Profile != null)
            {
                _Profile.Unapply();
                _Profile.ManagingBundle = null;
            }

            _Profile = value ?? new AssetBundleProfile.Unspecified();
            _Profile.ManagingBundle = this;
            _Profile.Apply();
        }
    }

    /// <summary>
    /// Name of the <see cref="AssetBundle"/>
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Check if <see cref="AssetBundle"/> has loaded
    /// </summary>
    public bool IsLoaded => _BundleSource.LoadedAssetBundle != null;

    /// <summary>
    /// Collection of <see cref="AssetReference"/> that Registered by this <see cref="AssetBundleReference"/>
    /// </summary>
    public AssetRegistry Assets { get; private set; } = new();

    private AssetBundleProfile _Profile;
    private readonly IAssetBundleSource _BundleSource;

    internal AssetBundleReference(string name, string filePath)
    {
        _BundleSource = new AssetBundleSource_File(filePath);
        Metadata = _BundleSource.Metadata;
        Name = name;

        RegisterAssets();
    }

    internal AssetBundleReference(string name, AssetBundle assetBundle)
    {
        _BundleSource = new AssetBundleSource_External(assetBundle);
        Metadata = _BundleSource.Metadata;
        Name = name;

        RegisterAssets();
    }

    private void RegisterAssets()
    {
        foreach (var asset in Metadata.Assets)
        {
            if (AssetAPI.ContainsAsset(asset))
            {
                if (AssetAPI.AllCustomAssets.TryGet(asset, out var assetRef))
                {
                    APILogger.Error("Asset", $"AssetName: '{asset}' is already registered by '{assetRef.AssetSourceName}'!");
                }
                else
                {
                    APILogger.Error("Asset", $"AssetName: '{asset}' is already registered!");
                }
                continue;
            }


            var assetReference = new AssetReference(asset, this);
            AssetAPI.AllCustomAssets[asset] = assetReference;
            Assets[asset] = assetReference;
        }
    }

    internal void Initialize(AssetBundleLoadingOptions loadingOptions)
    {
        if (loadingOptions != null)
        {
            if (loadingOptions.InitialMode.HasValue)
                Metadata.InitialMode = loadingOptions.InitialMode.Value;

            if (loadingOptions.DisableJustInTimeLoading.HasValue)
                Metadata.DisableJustInTimeLoading = loadingOptions.DisableJustInTimeLoading.Value;

            Profile = loadingOptions.Profile;
        }

        Profile ??= AssetBundleProfile.CreateInstance(Metadata.Profile);

        switch (Metadata.InitialMode)
        {
            case AssetBundleLoadingMode.LoadBundle:
                LoadBundle();
                break;

            case AssetBundleLoadingMode.LoadAllAssets:
                LoadBundle();
                LoadAllAssets();
                break;
        }
    }

    /// <summary>
    /// Load Referenced AssetBundle (Blocking)
    /// </summary>
    public void LoadBundle()
    {
        if (_BundleSource.CanManualLoad)
        {
            _BundleSource.Load();
        }
    }

    /// <summary>
    /// Load Referenced AssetBundle (Non-Blocking)
    /// </summary>
    /// <param name="onLoaded">Callback when AssetBundle has loaded</param>
    public void LoadBundleAsync(Action onLoaded = null)
    {
        if (_BundleSource.CanManualLoad)
        {
            _BundleSource.LoadAsync(onLoaded);
        }
    }

    /// <summary>
    /// Load Every <see cref="AssetReference"/> from this Bundle
    /// </summary>
    /// <returns>Collection of AssetReference</returns>
    public IEnumerable<AssetReference> LoadAllAssets()
    {
        ValidateAssetBundle();

        foreach (var asset in Assets.Assets)
        {
            asset.Load();
        }

        return Assets.Assets;
    }

    /// <summary>
    /// Load Every <see cref="AssetReference"/> from this Bundle (Non-Blocking)
    /// </summary>
    /// <param name="onLoaded">Callback when every Asset has loaded</param>
    public void LoadAllAssetsAsync(Action<IEnumerable<AssetReference>> onLoaded = null)
    {
        ValidateAssetBundle();

        int progressCounter = Assets.Assets.Count;
        foreach (var asset in Assets.Assets)
        {
            asset.LoadAsync(() =>
            {
                progressCounter--;

                if (progressCounter <= 0)
                {
                    onLoaded?.Invoke(Assets.Assets);
                }
            });
        }
    }

    /// <summary>
    /// Unload AssetBundle <u>This does NOT automatically unload all loaded assets!</u>
    /// </summary>
    public void UnloadBundle()
    {
        if (_BundleSource.CanManualUnload)
        {
            _BundleSource.Unload();
        }
    }

    /// <summary>
    /// Unload Every loaded Assets
    /// <para>
    /// For Unloading specific Assets, Access them via <see cref="Assets"/> and do manual <see cref="AssetReference.Unload"/>
    /// </para>
    /// </summary>
    public void UnloadAllAssets()
    {
        foreach (var asset in Assets.Assets)
        {
            asset.Unload();
        }
    }

    internal UnityEngine.Object LoadAsset_Internal(string assetName)
    {
        ValidateAssetBundle();
        return _BundleSource.LoadedAssetBundle.LoadAsset(assetName);
    }

    internal AssetBundleRequest LoadAssetAsync_Internal(string assetName)
    {
        ValidateAssetBundle();
        return _BundleSource.LoadedAssetBundle.LoadAssetAsync(assetName);
    }

    private void ValidateAssetBundle()
    {
        if (IsLoaded)
        {
            return;
        }

        if (Metadata.DisableJustInTimeLoading)
        {
            APILogger.Error($"AssetJIT", $"AssetBundle '{Name}' was requested but JIT loading is disabled!");
            throw new InvalidOperationException($"Bundle '{Name}' is not loaded!");
        }

        s_LoadSW.Restart();
        LoadBundle();
        s_LoadSW.Stop();

        APILogger.Debug($"AssetJIT", $"AssetBundle '{Name}' was JIT loaded in {s_LoadSW.ElapsedMilliseconds}ms!");
    }
}
