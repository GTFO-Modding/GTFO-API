using UnityEngine;

namespace GTFO.API.Assets.Loaders;

internal sealed class AssetBundleAsyncLoader : ObjectAsyncLoader<AssetBundle, AssetBundleCreateRequest>
{
    protected override void DoUnload(AssetBundle unloadingObject)
    {
        unloadingObject.Unload(unloadAllLoadedObjects: false);
    }

    protected override AssetBundle GetResult(AssetBundleCreateRequest operation)
    {
        return operation.assetBundle;
    }
}
