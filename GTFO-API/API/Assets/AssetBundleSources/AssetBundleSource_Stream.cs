using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace GTFO.API.Assets.AssetBundleSources;

internal sealed class AssetBundleSource_Stream : IAssetBundleSource
{
    public AssetBundle LoadedAssetBundle => _Loader.LoadedObject;
    public AssetBundleMetadata Metadata { get; private set; }
    public Stream Stream { get; private set; }

    private readonly AsyncLoader<AssetBundle> _Loader = new();

    internal AssetBundleSource_Stream(Stream stream, AssetBundleMetadata metadata = null)
    {
        Stream = stream;
        Metadata = metadata ?? new AssetBundleMetadata();
    }

    public void Load()
    {
        if (_Loader.IsLoaded)
            return;

        using var ms = new MemoryStream();
        Stream.Position = 0;
        Stream.CopyTo(ms);

        _Loader.LoadedObject = AssetBundle.LoadFromMemory(ms.ToArray());
        _Loader.SynchronouslyLoaded();
    }

    public void LoadAsync(Action onLoaded)
    {
        _Loader.OnLoaded += onLoaded;
        _Loader.Load(DoLoadAsync());
    }

    private IEnumerator DoLoadAsync()
    {
        using var ms = new MemoryStream();
        Stream.Position = 0;
        var copyTask = Stream.CopyToAsync(ms);
        yield return new WaitUntil((Func<bool>)(() => copyTask.IsCompleted));

        var request = AssetBundle.LoadFromMemoryAsync(ms.ToArray());
        yield return request;

        _Loader.LoadedObject = request.assetBundle;
    }

    public void Unload()
    {
        if (_Loader.LoadedObject != null)
        {
            _Loader.LoadedObject.Unload(unloadAllLoadedObjects: false);
        }
        _Loader.Unload();
    }
}
