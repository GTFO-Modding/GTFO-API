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
    private AssetBundleCreateRequest _OnGoingRequest = null;

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
        bool shouldRewrite = false;
        if (Metadata.Revision != AssetBundleMetadata.MetadataFormatRevision)
        {
            APILogger.Debug("AssetBundle", $"Metadata Revision mismatch with GAPI version (JSON:{Metadata.Revision} != GAPI:{AssetBundleMetadata.MetadataFormatRevision})");
            Metadata.Revision = AssetBundleMetadata.MetadataFormatRevision;
            shouldRewrite = true;
        }

        if (!Metadata.Checksum.Equals(checksum, StringComparison.OrdinalIgnoreCase))
        {
            APILogger.Debug("AssetBundle", $"File has modified! Fetching AssetNames! (JSON:{Metadata.Checksum} != CURRENT:{checksum})");
            Metadata.Checksum = checksum;

            var bundle = AssetBundle.LoadFromFile(FilePath);
            Metadata.Assets = [.. bundle.GetAllAssetNames()];
            bundle.Unload(unloadAllLoadedObjects: true);
            shouldRewrite = true;
        }

        if (shouldRewrite)
        {
            using var metaFileStream = File.Create(MetaFilePath);
            AssetBundleMetadata.WriteJson(Metadata, metaFileStream);
        }
    }

    public void Load()
    {
        if (_Loader.IsLoaded)
            return;


        if (_OnGoingRequest != null)
        {
            APILogger.Warn("AssetAPI", $"AssetBundle '{FilePath}' has requested while running non-blocking load!");
            _Loader.LoadedObject = _OnGoingRequest.assetBundle;
            _OnGoingRequest = null;
        }
        else
        {
            _Loader.LoadedObject = AssetBundle.LoadFromFile(FilePath);
        }

        _Loader.SynchronouslyLoaded();
    }

    public void LoadAsync(Action onLoaded)
    {
        _Loader.OnLoaded += onLoaded;
        _Loader.Load(DoLoadAsync());
    }

    private IEnumerator DoLoadAsync()
    {
        _OnGoingRequest = AssetBundle.LoadFromFileAsync(FilePath);
        yield return _OnGoingRequest;

        _Loader.LoadedObject = _OnGoingRequest.assetBundle;
        _OnGoingRequest = null;
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
