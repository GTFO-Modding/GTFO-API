using System;
using System.Linq;
using GTFO.API.Impl;

namespace GTFO.API.EventArg;

/// <summary>
/// EventArgs for <see cref="AssetAPI.OnLoadLevelAssets"/>
/// </summary>
public sealed class OnLoadLevelAssetsEventArgs : EventArgs
{
    internal LevelAssetLoadJob_Impl Impl;

    public bool IsEmpty => !Impl.AssetsToLoad.Any() && !Impl.BundlesToLoad.Any();

    public OnLoadLevelAssetsEventArgs()
    {
        Impl = new LevelAssetLoadJob_Impl();
    }

    public void EnqueueAssetToLoad(string assetName)
    {
        if (!AssetAPI.AllCustomAssets.TryGet(assetName, out var assetRef))
        {
            return;
        }

        if (assetRef.AssetSourceBundle != null)
        {
            Impl.BundlesToLoad.Add(assetRef.AssetSourceBundle);
        }

        Impl.AssetsToLoad.Add(assetRef);
    }

    public void EnqueueAssetBundleToLoad(string assetBundleName)
    {
        if (!AssetAPI.AllCustomBundles.TryGet(assetBundleName, out var assetBundleRef))
        {
            return;
        }

        Impl.BundlesToLoad.Add(assetBundleRef);
    }
}
