using System;
using System.Collections.Generic;
using System.Diagnostics;
using GTFO.API.Assets.AssetBundleSources;
using GTFO.API.Assets.UnloadStrategies;
using UnityEngine;

namespace GTFO.API.Assets;

public sealed class AssetBundleReference
{
    private static readonly Stopwatch s_LoadSW = new();

    public AssetBundleMetadata Metadata { get; private set; }

    public AssetBundleUnloadStrategy UnloadStrategy
    {
        get => _UnloadStrategy;
        set
        {
            if (_UnloadStrategy != null)
            {
                _UnloadStrategy.Unapply();
                _UnloadStrategy.ManagingBundle = null;
            }

            _UnloadStrategy = value ?? new AssetBundleUnloadStrategy.Unspecified();
            _UnloadStrategy.ManagingBundle = this;
            _UnloadStrategy.Apply();
        }
    }

    public string Name { get; private set; }
    public bool IsLoaded => _BundleSource.LoadedAssetBundle != null;
    public AssetRegistry Assets { get; private set; } = new();

    private AssetBundleUnloadStrategy _UnloadStrategy;
    private readonly IAssetBundleSource _BundleSource;

    internal AssetBundleReference(string name, string filePath, AssetBundleLoadingOptions loadingOptions = null)
    {
        _BundleSource = new AssetBundleSource_File(filePath);
        Metadata = _BundleSource.Metadata;
        Name = name;

        if (loadingOptions != null)
        {
            if (loadingOptions.InitialLoadMode.HasValue)
                Metadata.InitialLoadMode = loadingOptions.InitialLoadMode.Value;

            if (loadingOptions.DisableJustInTimeLoading.HasValue)
                Metadata.DisableJustInTimeLoading = loadingOptions.DisableJustInTimeLoading.Value;

            UnloadStrategy = loadingOptions.UnloadStrategy;
        }

        Initialize();
    }

    internal AssetBundleReference(string name, AssetBundle assetBundle)
    {
        _BundleSource = new AssetBundleSource_External(assetBundle);
        Metadata = _BundleSource.Metadata;
        Name = name;

        Initialize();
    }

    private void Initialize()
    {
        UnloadStrategy ??= AssetBundleUnloadStrategy.CreateInstance(Metadata.UnloadStrategy);

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

        switch (Metadata.InitialLoadMode)
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

    public void LoadBundle()
    {
        if (_BundleSource.CanManualLoad)
        {
            _BundleSource.Load();
        }
    }

    public void LoadBundleAsync(Action onLoaded = null)
    {
        if (_BundleSource.CanManualLoad)
        {
            _BundleSource.LoadAsync(onLoaded);
        }
    }

    public IEnumerable<AssetReference> LoadAllAssets()
    {
        ValidateAssetBundle();

        foreach (var asset in Assets.Assets)
        {
            asset.Load();
        }

        return Assets.Assets;
    }

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

    public void UnloadBundle()
    {
        if (_BundleSource.CanManualUnload)
        {
            _BundleSource.Unload();
        }
    }

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
