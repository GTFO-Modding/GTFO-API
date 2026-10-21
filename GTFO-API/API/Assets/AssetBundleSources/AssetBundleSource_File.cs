using System;
using System.Collections;
using System.IO;
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

    private readonly AsyncLoader<AssetBundle> _Loader = new();

    internal AssetBundleSource_File(string filePath)
    {
        FilePath = filePath;
        MetaFilePath = filePath + AssetBundleMetadata.FileExtension;

        try
        {
            if (File.Exists(MetaFilePath))
            {
                using var fs = File.OpenRead(MetaFilePath);
                Metadata = AssetBundleMetadata.ReadJson(fs);
            }
            else
            {
                Metadata = new();
            }
        }
        catch (Exception e)
        {
            APILogger.Error("AssetBundle", $"Exception has thrown while loading '{MetaFilePath}'");
            APILogger.Error("AssetBundle", $"StackTrace: {e}");
            Metadata = new();
        }

        var checksum = AssetBundleMetadata.ComputeChecksum(FilePath);
        if (!Metadata.Checksum.Equals(checksum, StringComparison.OrdinalIgnoreCase))
        {
            APILogger.Verbose("AssetBundle", $"File has modified! Rewriting Metadata! ({Metadata.Checksum} != {checksum})");
            Metadata.Checksum = checksum;

            var bundle = AssetBundle.LoadFromFile(FilePath);
            Metadata.Assets = [.. bundle.GetAllAssetNames()];
            bundle.Unload(unloadAllLoadedObjects: true);

            using var metaFileStream = File.Create(MetaFilePath);
            AssetBundleMetadata.WriteJson(Metadata, metaFileStream);
        }
    }

    public void Load()
    {
        if (_Loader.IsLoaded)
            return;

        _Loader.LoadedObject = AssetBundle.LoadFromFile(FilePath);
        _Loader.SynchronouslyLoaded();
    }

    public void LoadAsync(Action onLoaded)
    {
        _Loader.OnLoaded += onLoaded;
        _Loader.Load(DoLoadAsync());
    }

    private IEnumerator DoLoadAsync()
    {
        var request = AssetBundle.LoadFromFileAsync(FilePath);
        yield return request;

        _Loader.LoadedObject = request.assetBundle;
    }

    public void Unload()
    {
        if (LoadedAssetBundle != null)
        {
            LoadedAssetBundle.Unload(unloadAllLoadedObjects: false);
        }
        _Loader.Unload();
    }
}
