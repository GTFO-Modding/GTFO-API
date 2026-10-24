using UnityEngine;

namespace GTFO.API.Assets.Loaders;

internal sealed class AssetAsyncLoader : ObjectAsyncLoader<Object, AssetBundleRequest>
{
    protected override void DoUnload(Object unloadingObject)
    {
        Object.Destroy(unloadingObject);
    }

    protected override Object GetResult(AssetBundleRequest operation)
    {
        return operation.asset;
    }
}
