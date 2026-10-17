using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text.Json;
using UnityEngine;

namespace GTFO.API.Assets.AssetBundleSources;

internal sealed class AssetBundleSource_File : IAssetBundleSource
{
    public AssetBundle LoadedAssetBundle => _Loader.LoadedObject;
    public AssetBundleMetadata Metadata { get; private set; }
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
                Metadata = JsonSerializer.Deserialize<AssetBundleMetadata>(File.ReadAllText(MetaFilePath));
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

        var lastModified = File.GetLastWriteTimeUtc(FilePath).ToString(AssetBundleMetadata.LastModifiedFormat, CultureInfo.InvariantCulture);
        if (!Metadata.LastModified.Equals(lastModified, StringComparison.InvariantCulture))
        {
            APILogger.Verbose("AssetBundle", $"File has modified! Rewriting Metadata! ({Metadata.LastModified} != {lastModified})");
            Metadata.LastModified = lastModified;

            var bundle = AssetBundle.LoadFromFile(FilePath);
            Metadata.Assets = [.. bundle.GetAllAssetNames()];
            bundle.Unload(unloadAllLoadedObjects: true);

            using var metaFileStream = File.Create(MetaFilePath);
            JsonSerializer.Serialize(metaFileStream, Metadata, new JsonSerializerOptions() { WriteIndented = true });
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

    public void Unload(bool unloadAllAssets)
    {
        if (LoadedAssetBundle != null)
        {
            LoadedAssetBundle.Unload(unloadAllLoadedObjects: false);
        }
        _Loader.Unload();
    }
}
