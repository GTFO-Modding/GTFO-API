using System;
using AssetShards;
using UnityEngine;

namespace GTFO.API.Assets.AssetSources;

internal sealed class AssetSource_External : IAssetSource
{
    public string SourceName => "External";
    public UnityEngine.Object LoadedAsset { get; private set; }
    public bool CanManualLoad => false;
    public bool CanManualUnload => false;
    public string AssetName { get; private set; }

    public AssetSource_External(string assetName, UnityEngine.Object asset)
    {
        LoadedAsset = asset;
        LoadedAsset.hideFlags |= HideFlags.DontUnloadUnusedAsset;

        AssetName = assetName;

        if (AssetShardManager.s_loadedAssetsLookup == null)
        {
            AssetAPI.OnImplReady += Register;
        }
        else
        {
            Register();
        }
    }

    private void Register()
    {
        AssetShardManager.s_loadedAssetsLookup[AssetName.ToUpper()] = LoadedAsset;
    }

    public void Load()
    {
        APILogger.Error("Asset", $"{nameof(Load)} is not allowed for external asset! '{AssetName}'");
        APILogger.Error("Asset", $" - StackTrace: {Environment.StackTrace}");
    }

    public void LoadAsync(Action onLoaded)
    {
        APILogger.Error("Asset", $"{nameof(LoadAsync)} is not allowed for external asset! '{AssetName}'");
        APILogger.Error("Asset", $" - StackTrace: {Environment.StackTrace}");
    }

    public void Unload()
    {
        APILogger.Error("Asset", $"{nameof(Unload)} is not allowed for external asset! '{AssetName}'");
        APILogger.Error("Asset", $" - StackTrace: {Environment.StackTrace}");
    }
}
