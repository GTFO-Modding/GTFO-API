using GTFO.API.Assets.ObjectLoaders;
using GTFO.API.EventArg;
using UnityEngine;

namespace GTFO.API.Assets.AssetBundleSources;

internal sealed class AssetBundleSource_File : IAssetBundleSource
{
    public string SourceName => $"File ({FilePath})";
    public AssetBundle LoadedAssetBundle => _Loader.LoadedObject;
    public AssetBundleMetadata Metadata { get; private set; }
    public bool CanManualLoad => true;
    public bool CanManualUnload => true;
    public string FilePath { get; private set; }
    public string MetaFilePath { get; private set; }

    private readonly AssetBundleAsyncLoader _Loader = new();

    internal AssetBundleSource_File(string filePath)
    {
        FilePath = filePath;
        MetaFilePath = filePath + AssetBundleMetadata.FileExtension;

        if (!AssetAPI._BundleMetadataCache.TryGetValue(filePath, out var metadata))
        {
            metadata = AssetBundleMetadata.CheckMetadata(filePath);
        }
        Metadata = metadata;
    }

    public void Load()
    {
        _Loader.Load(() => AssetBundle.LoadFromFile(FilePath));
    }

    public void LoadAsync(UnityObjectRequestCallback onLoaded)
    {
        _Loader.LoadAsync(() => AssetBundle.LoadFromFileAsync(FilePath), onLoaded);
    }

    public void Unload()
    {
        _Loader.Unload();
    }
}
