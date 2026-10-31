using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using BepInEx;
using GTFO.API.JSON.Converters;
using UnityEngine;

namespace GTFO.API.Assets;

/// <summary>
/// Metadata for <see cref="AssetReference"/>
/// </summary>
public sealed class AssetMetadata
{
    /// <summary>
    /// Name of the AssetType
    /// </summary>
    [JsonInclude]
    public string AssetType { get; internal set; } = typeof(UnityEngine.Object).Name;

    /// <summary>
    /// List of Custom Tags, Case Insensitive
    /// </summary>
    [JsonInclude]
    [JsonConverter(typeof(AssetTagsConverter))]
    public IReadOnlySet<string> Tags { get; internal set; } = new HashSet<string>();

    internal static AssetMetadata Create(UnityEngine.Object asset)
    {
        return new AssetMetadata()
        {
            AssetType = asset.GetIl2CppType().Name
        };
    }

    internal static Dictionary<string, AssetMetadata> CreateAssetMetadatasFromBundle(AssetBundle bundle, bool unloadAssetAfterScan = true)
    {
        var assetMetas = new Dictionary<string, AssetMetadata>(StringComparer.OrdinalIgnoreCase);
        foreach (var assetName in bundle.GetAllAssetNames())
        {
            APILogger.Verbose("AssetAPI", $"Generating Metadata for '{assetName}'");
            var asset = bundle.LoadAsset(assetName);
            var assetMeta = Create(asset);
            assetMetas[assetName] = assetMeta;

            if (unloadAssetAfterScan)
                UnityEngine.Object.Destroy(asset);

            APILogger.Verbose("AssetAPI", $"Metadata Generated!");
        }

        return assetMetas;
    }

    private static readonly ThreadLocal<AssetsManager> s_Manager = new(() =>
    {
        var tpkFile = Path.Combine(Paths.BepInExRootPath, "cache", "classdata.tpk");
        if (!File.Exists(tpkFile))
            throw new FileNotFoundException($"TPK File: '{tpkFile}' is missing!");

        var manager = new AssetsManager();
        manager.LoadClassPackage(tpkFile);
        return manager;
    });

    internal static Dictionary<string, AssetMetadata> CreateAssetMetadatasFromBundleFile(string path)
    {
        AssetsManager manager = null;

        try
        {
            manager = s_Manager.Value;

            var bundleInstance = manager.LoadBundleFile(path, true);
            var assetFileInstance = manager.LoadAssetsFileFromBundle(bundleInstance, 0, false);

            var assetFile = assetFileInstance.file;
            var classIDlookup = new Dictionary<long, int>();
            foreach (var assetInfo in assetFile.AssetInfos)
                classIDlookup[assetInfo.PathId] = assetInfo.TypeId;

            var bundle = assetFile.GetAssetsOfType(AssetClassID.AssetBundle).Single();
            AssetTypeValueField container = manager.GetBaseField(assetFileInstance, bundle)["m_Container"]["Array"];

            var assetMetas = new Dictionary<string, AssetMetadata>(StringComparer.OrdinalIgnoreCase);
            foreach (AssetTypeValueField entry in container.Children)
            {
                string assetPath = entry["first"].AsString;
                AssetTypeValueField pptr = entry["second"]["asset"];

                int fileId = pptr["m_FileID"].AsInt;
                long pathId = pptr["m_PathID"].AsLong;
                var typeString = fileId == 0 && classIDlookup.TryGetValue(pathId, out int classId)
                    ? ((AssetClassID)classId).ToString()
                    : "Unknown";

                assetMetas[assetPath] = new AssetMetadata()
                {
                    AssetType = typeString
                };
            }

            return assetMetas;
        }
        finally
        {
            manager?.UnloadAll(unloadClassData: false);
        }
    }
}
