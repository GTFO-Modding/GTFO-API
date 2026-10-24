using AssetShards;
using GTFO.API.Assets.Loaders;
using GTFO.API.EventArg;
using UnityEngine;

namespace GTFO.API.Assets.AssetSources;

internal sealed class AssetSource_AssetBundle : IAssetSource
{
    public string SourceName => $"AssetBundle ({OwnerBundle.Name})";
    public UnityEngine.Object LoadedAsset => _Loader.LoadedObject;
    public AssetBundleReference OwnerBundle { get; private set; }
    public bool CanManualLoad => true;
    public bool CanManualUnload => true;
    public string AssetName { get; private set; }

    private readonly AssetAsyncLoader _Loader = new();

    internal AssetSource_AssetBundle(string assetName, AssetBundleReference ownerBundle)
    {
        AssetName = assetName;
        OwnerBundle = ownerBundle;
        _Loader.LoadedObjectChanged += OnLoadedAssetChanged;
    }

    private void OnLoadedAssetChanged(UnityEngine.Object asset)
    {
        if (asset != null)
        {
            asset.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            AssetShardManager.s_loadedAssetsLookup[AssetName.ToUpper()] = asset;
        }
        else
        {
            AssetShardManager.s_loadedAssetsLookup.Remove(AssetName.ToUpper());
        }
    }

    public void Load()
    {
        _Loader.Load(() => OwnerBundle.LoadAsset_Internal(AssetName));
    }

    public void LoadAsync(UnityObjectRequestEvent onLoaded)
    {
        _Loader.LoadAsync(() => OwnerBundle.LoadAssetAsync_Internal(AssetName), onLoaded);
    }

    public void Unload()
    {
        _Loader.Unload();
    }
}
