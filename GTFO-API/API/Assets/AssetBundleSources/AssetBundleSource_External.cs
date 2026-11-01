using System;
using GTFO.API.EventArg;
using UnityEngine;

namespace GTFO.API.Assets.AssetBundleSources;

internal sealed class AssetBundleSource_External : IAssetBundleSource
{
    private static readonly APIModuledLogger Log = new("Asset");

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
            Assets = AssetMetadata.CreateAssetMetadatasFromBundle(assetBundle)
        };
    }

    public void Load()
    {
        Log.Error($"{nameof(Load)} is not allowed for external AssetBundle! '{LoadedAssetBundle.name}'");
        Log.Error($" - StackTrace: {Environment.StackTrace}");
    }

    public void LoadAsync(UnityObjectRequestCallback onLoaded, params AssetBundleReference[] dependency)
    {
        Log.Error($"{nameof(LoadAsync)} is not allowed for external AssetBundle! '{LoadedAssetBundle.name}'");
        Log.Error($" - StackTrace: {Environment.StackTrace}");
    }

    public void Unload()
    {
        Log.Error($"{nameof(Unload)} is not allowed for external AssetBundle! '{LoadedAssetBundle.name}'");
        Log.Error($" - StackTrace: {Environment.StackTrace}");
    }
}
