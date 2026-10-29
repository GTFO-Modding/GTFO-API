using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx;
using GTFO.API.Assets.Profiles;
using UnityEngine;

namespace GTFO.API.Assets;

/// <summary>
/// Metadata for <see cref="AssetBundleReference"/>
/// </summary>
public sealed class AssetBundleMetadata
{
    /// <summary>
    /// Extension of the Metadata File
    /// <code>.meta.json</code>
    /// </summary>
    public static readonly string FileExtension = ".meta.json";

    /// <summary>
    /// Revision of Metadata Format
    /// </summary>
    public static readonly int MetadataFormatRevision = 1; //Bump this to trigger force metadata rebuild

    private static readonly JsonSerializerOptions _JsonOpt;

    static AssetBundleMetadata()
    {
        _JsonOpt = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };

        _JsonOpt.Converters.Add(new JsonStringEnumConverter());
    }

    /// <summary>
    /// Defines how AssetBundle should be Managed. To modify, check <see cref="AssetBundleReference.Profile"/>
    /// </summary>
    [JsonInclude]
    public string Profile { get; internal set; } = nameof(AssetBundleProfile.Unspecified);

    /// <summary>
    /// Defines how AssetBundle should be Loaded after Initial Startup.
    /// </summary>
    [JsonInclude]
    public AssetBundleLoadingMode InitialMode { get; internal set; } = AssetBundleLoadingMode.DontLoad;

    /// <summary>
    /// Flag for toggling JIT-Loading
    /// </summary>
    [JsonInclude]
    public bool DisableJustInTimeLoading { get; internal set; } = false;

    /// <summary>
    /// List of Asset Name defined in <see cref="AssetBundle"/>
    /// </summary>
    [JsonInclude]
    public IReadOnlyDictionary<string, AssetMetadata> Assets { get; internal set; } = new Dictionary<string, AssetMetadata>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// List of Dependency that referenced by <see cref="AssetBundle"/>
    /// </summary>
    [JsonInclude]
    public IReadOnlyList<string> Dependencies { get; internal set; } = [];

    /// <summary>
    /// MD5 Checksum of the <see cref="AssetBundle"/> file
    /// </summary>
    [JsonInclude]
    public string Checksum { get; internal set; } = "";

    /// <summary>
    /// Revision of Metadata json file format
    /// </summary>
    [JsonInclude]
    public int FormatRevision { get; internal set; } = -1;

    /// <summary>
    /// Generate Checksum from File path
    /// </summary>
    /// <param name="filePath">File path to AssetBundle</param>
    /// <returns>MD5 hashed Checksum <see langword="string"/></returns>
    public static string ComputeChecksum(string filePath)
    {
        using var fs = File.OpenRead(filePath);
        using var md5 = MD5.Create();
        return Convert.ToHexString(md5.ComputeHash(fs));
    }

    /// <summary>
    /// Generate Checksum from Stream
    /// </summary>
    /// <param name="stream">Stream of AssetBundle</param>
    /// <returns>MD5 hashed Checksum <see langword="string"/></returns>
    public static string ComputeChecksum(Stream stream)
    {
        using var md5 = MD5.Create();
        return Convert.ToHexString(md5.ComputeHash(stream));
    }

    /// <summary>
    /// Deserialize Json String into <see cref="AssetBundleMetadata"/>
    /// </summary>
    /// <param name="json">Json String</param>
    /// <returns>Deserialized <see cref="AssetBundleMetadata"/></returns>
    public static AssetBundleMetadata ReadJson(string json)
    {
        return JsonSerializer.Deserialize<AssetBundleMetadata>(json, _JsonOpt);
    }

    /// <summary>
    /// Deserialize Json Stream into <see cref="AssetBundleMetadata"/>
    /// </summary>
    /// <param name="jsonStream">Json String Stream</param>
    /// <returns>Deserialized <see cref="AssetBundleMetadata"/></returns>
    public static AssetBundleMetadata ReadJson(Stream jsonStream)
    {
        return JsonSerializer.Deserialize<AssetBundleMetadata>(jsonStream, _JsonOpt);
    }

    /// <summary>
    /// Serialize <see cref="AssetBundleMetadata"/> into Json String
    /// </summary>
    /// <param name="metadata">Metadata to Serialize</param>
    /// <returns>Serialized Json String</returns>
    public static string WriteJson(AssetBundleMetadata metadata)
    {
        return JsonSerializer.Serialize(metadata, _JsonOpt);
    }

    /// <summary>
    /// Serialize <see cref="AssetBundleMetadata"/> into Stream
    /// </summary>
    /// <param name="metadata">Metadata to Serialize</param>
    /// <param name="stream">Stream to write Serialized String</param>
    public static void WriteJson(AssetBundleMetadata metadata, Stream stream)
    {
        JsonSerializer.Serialize(stream, metadata, _JsonOpt);
    }

    internal static AssetBundleMetadata CheckMetadata(string filePath)
    {
        var metaFilePath = filePath + FileExtension;
        var filePathRelToBIE = Path.GetRelativePath(Paths.BepInExRootPath, filePath);
        var metaPathRelToBIE = Path.GetRelativePath(Paths.BepInExRootPath, metaFilePath);

        AssetBundleMetadata metadata;

        try
        {
            if (File.Exists(metaFilePath))
            {
                using var fs = File.OpenRead(metaFilePath);
                metadata = ReadJson(fs);
            }
            else
            {
                metadata = new();
            }
        }
        catch (Exception e)
        {
            APILogger.Error("AssetBundle", $"Exception has thrown while loading '{metaPathRelToBIE}'");
            APILogger.Error("AssetBundle", $"StackTrace: {e}");
            metadata = new();
        }

        var checksum = ComputeChecksum(filePath);
        bool shouldRewrite = false;
        if (metadata.FormatRevision != MetadataFormatRevision)
        {
            if (metadata.FormatRevision < 0)
                APILogger.Info("AssetBundle", $"New AssetBundle File: '{filePathRelToBIE}' detected!");
            else
                APILogger.Warn("AssetBundle", $"'{metaPathRelToBIE}' {nameof(FormatRevision)} mismatch with GAPI version (JSON:{metadata.FormatRevision} != GAPI:{MetadataFormatRevision})");

            metadata.FormatRevision = MetadataFormatRevision;
            shouldRewrite = true;
        }

        if (!metadata.Checksum.Equals(checksum, StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrEmpty(metadata.Checksum))
                APILogger.Info("AssetBundle", $"'{filePathRelToBIE}' has modified. Re-scanning asset lists... (JSON:{metadata.Checksum} != CURRENT:{checksum})");

            metadata.Checksum = checksum;
            metadata.Assets = AssetMetadata.CreateAssetMetadatasFromBundleFile(filePath);
            shouldRewrite = true;
        }

        if (shouldRewrite)
        {
            using var metaFileStream = File.Create(metaFilePath);
            WriteJson(metadata, metaFileStream);
            APILogger.Info("AssetBundle", $"New Metadata has written to disk: {metaPathRelToBIE}");
        }

        return metadata;
    }
}

/// <summary>
/// Enum for Specify how AssetBundle should be loaded.
/// </summary>
public enum AssetBundleLoadingMode
{
    /// <summary>
    /// Don't Load AssetBundle
    /// </summary>
    DontLoad,

    /// <summary>
    /// Load AssetBundle but Assets
    /// </summary>
    LoadBundle,

    /// <summary>
    /// Load both AssetBundle and Assets
    /// </summary>
    LoadAllAssets
}
