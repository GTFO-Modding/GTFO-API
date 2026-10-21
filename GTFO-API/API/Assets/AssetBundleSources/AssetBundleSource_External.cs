using System;
using UnityEngine;

namespace GTFO.API.Assets.AssetBundleSources;

internal sealed class AssetBundleSource_External : IAssetBundleSource
{
    public string SourceName => $"External";
    public AssetBundle LoadedAssetBundle { get; private set; }
    public AssetBundleMetadata Metadata { get; private set; }
    public bool CanManualLoad => false;
    public bool CanManualUnload => false;

    internal AssetBundleSource_External(AssetBundle assetBundle)
    {
        LoadedAssetBundle = assetBundle;
        Metadata = new AssetBundleMetadata
        {
            Assets = [.. assetBundle.GetAllAssetNames()]
        };
    }

    public void Load()
    {
        APILogger.Error("Asset", $"{nameof(Load)} is not allowed for external AssetBundle! '{LoadedAssetBundle.name}'");
        APILogger.Error("Asset", $" - StackTrace: {Environment.StackTrace}");
    }

    public void LoadAsync(Action onLoaded)
    {
        APILogger.Error("Asset", $"{nameof(LoadAsync)} is not allowed for external AssetBundle! '{LoadedAssetBundle.name}'");
        APILogger.Error("Asset", $" - StackTrace: {Environment.StackTrace}");
    }

    public void Unload()
    {
        APILogger.Error("Asset", $"{nameof(Unload)} is not allowed for external AssetBundle! '{LoadedAssetBundle.name}'");
        APILogger.Error("Asset", $" - StackTrace: {Environment.StackTrace}");
    }
}
