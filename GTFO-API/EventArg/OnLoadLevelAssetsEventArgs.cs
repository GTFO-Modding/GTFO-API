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

    /// <summary>
    /// Check if Loading Queue is empty
    /// </summary>
    public bool IsLoadingQueueEmpty => !Impl.AssetsToLoad.Any() && !Impl.BundlesToLoad.Any();

    /// <summary>
    /// Enqueue Asset to Load
    /// </summary>
    /// <param name="assetName">Asset Name</param>
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

    /// <summary>
    /// Enqueue AssetBundle to Load
    /// </summary>
    /// <param name="assetBundleName">AssetBundle Name</param>
    public void EnqueueAssetBundleToLoad(string assetBundleName)
    {
        if (!AssetAPI.AllCustomBundles.TryGet(assetBundleName, out var assetBundleRef))
        {
            return;
        }

        Impl.BundlesToLoad.Add(assetBundleRef);
    }
}
