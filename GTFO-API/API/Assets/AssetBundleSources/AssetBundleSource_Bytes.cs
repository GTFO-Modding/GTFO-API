using System;
using System.Collections;
using UnityEngine;

namespace GTFO.API.Assets.AssetBundleSources;

internal sealed class AssetBundleSource_Bytes : IAssetBundleSource
{
    public AssetBundle LoadedAssetBundle => _Loader.LoadedObject;
    public AssetBundleMetadata Metadata { get; private set; }
    public ReadOnlyMemory<byte> Bytes { get; private set; }

    private readonly AsyncLoader<AssetBundle> _Loader = new();

    internal AssetBundleSource_Bytes(byte[] bytes, AssetBundleMetadata metadata = null)
    {
        Bytes = bytes;
        Metadata = metadata ?? new AssetBundleMetadata();
    }

    public void Load()
    {
        if (_Loader.IsLoaded)
            return;

        _Loader.LoadedObject = AssetBundle.LoadFromMemory(Bytes.ToArray());
        _Loader.SynchronouslyLoaded();
    }

    public void LoadAsync(Action onLoaded)
    {
        _Loader.OnLoaded += onLoaded;
        _Loader.Load(DoLoadAsync());
    }

    private IEnumerator DoLoadAsync()
    {
        var request = AssetBundle.LoadFromMemoryAsync(Bytes.ToArray());
        yield return request;

        _Loader.LoadedObject = request.assetBundle;
    }

    public void Unload(bool unloadAllAssets)
    {
        if (_Loader.LoadedObject != null)
        {
            _Loader.LoadedObject.Unload(unloadAllLoadedObjects: false);
        }
        _Loader.Unload();
    }
}
