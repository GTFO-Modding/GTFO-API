using System;
using System.Collections;
using AssetShards;

namespace GTFO.API.Assets.AssetSources;

internal sealed class AssetSource_AssetBundle : IAssetSource
{
    public string SourceName => $"AssetBundle ({OwnerBundle.Name})";
    public UnityEngine.Object LoadedAsset => _Loader.LoadedObject;
    public AssetBundleReference OwnerBundle { get; private set; }
    public string AssetName { get; private set; }

    private readonly AsyncLoader<UnityEngine.Object> _Loader = new();

    internal AssetSource_AssetBundle(string assetName, AssetBundleReference ownerBundle)
    {
        AssetName = assetName;
        OwnerBundle = ownerBundle;
        _Loader.OnLoadedObjectChanged += OnLoadedAssetChanged;
    }

    private void OnLoadedAssetChanged()
    {
        if (LoadedAsset != null)
        {
            AssetShardManager.s_loadedAssetsLookup[AssetName.ToUpper()] = LoadedAsset;
        }
        else
        {
            AssetShardManager.s_loadedAssetsLookup.Remove(AssetName.ToUpper());
        }
    }

    public void Load()
    {
        if (_Loader.IsLoaded)
            return;

        _Loader.LoadedObject = OwnerBundle.LoadAsset_Internal(AssetName);
        _Loader.SynchronouslyLoaded();
    }

    public void LoadAsync(Action onLoaded)
    {
        _Loader.OnLoaded += onLoaded;
        _Loader.Load(DoLoadAsync());
    }

    private IEnumerator DoLoadAsync()
    {
        var request = OwnerBundle.LoadAssetAsync_Internal(AssetName);
        yield return request;

        _Loader.LoadedObject = request.asset;
    }

    public void Unload()
    {
        if (LoadedAsset != null)
        {
            UnityEngine.Object.Destroy(LoadedAsset);
        }

        _Loader.Unload();
    }
}
