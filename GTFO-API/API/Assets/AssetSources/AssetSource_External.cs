using System;
using GTFO.API.EventArg;
using UnityEngine;

namespace GTFO.API.Assets.AssetSources;

internal sealed class AssetSource_External : IAssetSource
{
    private static readonly APIModuledLogger Log = new("Asset");

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
    }

    public void Load()
    {
        Log.Error($"{nameof(Load)} is not allowed for external asset! '{AssetName}'");
        Log.Error($" - StackTrace: {Environment.StackTrace}");
    }

    public void LoadAsync(UnityObjectRequestCallback onLoaded)
    {
        Log.Error($"{nameof(LoadAsync)} is not allowed for external asset! '{AssetName}'");
        Log.Error($" - StackTrace: {Environment.StackTrace}");
    }

    public void Unload()
    {
        Log.Error($"{nameof(Unload)} is not allowed for external asset! '{AssetName}'");
        Log.Error($" - StackTrace: {Environment.StackTrace}");
    }
}
