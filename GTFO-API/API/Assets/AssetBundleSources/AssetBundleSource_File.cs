using System;
using System.IO;
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
