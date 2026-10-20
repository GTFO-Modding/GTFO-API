using System.Linq;
using GTFO.API.Impl;

namespace GTFO.API.Assets;

public sealed class LevelAssetLoadJob
{
    internal LevelAssetLoadJob_Impl Impl;

    public bool IsEmpty => !Impl.AssetsToLoad.Any() && !Impl.BundlesToLoad.Any();

    public LevelAssetLoadJob()
    {
        Impl = new LevelAssetLoadJob_Impl();
    }

    public void EnqueueAssetToLoad(string assetName)
    {
        if (!AssetReference.AllAssets.TryGet(assetName, out var assetRef))
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
        if (!AssetBundleReference.AllReferences.TryGet(assetBundleName, out var assetBundleRef))
        {
            return;
        }

        Impl.BundlesToLoad.Add(assetBundleRef);
    }
}
