using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using GTFO.API.Assets.UnloadStrategies;
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
    /// Defines how AssetBundle should be Loaded after Initial Startup.
    /// </summary>
    [JsonInclude]
    public AssetBundleLoadingMode InitialLoadMode { get; internal set; } = AssetBundleLoadingMode.DontLoad;

    /// <summary>
    /// Defines how AssetBundle should be Managed. To modify, check <see cref="AssetBundleReference.UnloadStrategy"/>
    /// </summary>
    [JsonInclude]
    public string UnloadStrategy { get; internal set; } = nameof(AssetBundleUnloadStrategy.Unspecified);

    /// <summary>
    /// Flag for toggling JIT-Loading
    /// </summary>
    [JsonInclude]
    public bool DisableJustInTimeLoading { get; internal set; } = false;

    /// <summary>
    /// MD5 Checksum of the <see cref="AssetBundle"/> file
    /// </summary>
    [JsonInclude]
    public string Checksum { get; internal set; } = "";

    /// <summary>
    /// List of Asset Names defined in <see cref="AssetBundle"/>
    /// </summary>
    [JsonInclude]
    public IReadOnlyList<string> Assets { get; internal set; } = [];

    public static string ComputeChecksum(string filePath)
    {
        using var fs = File.OpenRead(filePath);
        using var md5 = MD5.Create();
        return Convert.ToHexString(md5.ComputeHash(fs));
    }

    public static string ComputeChecksum(Stream stream)
    {
        using var md5 = MD5.Create();
        return Convert.ToHexString(md5.ComputeHash(stream));
    }

    public static AssetBundleMetadata ReadJson(string json)
    {
        return JsonSerializer.Deserialize<AssetBundleMetadata>(json, _JsonOpt);
    }

    public static AssetBundleMetadata ReadJson(Stream jsonStream)
    {
        return JsonSerializer.Deserialize<AssetBundleMetadata>(jsonStream, _JsonOpt);
    }

    public static string WriteJson(AssetBundleMetadata metadata)
    {
        return JsonSerializer.Serialize(metadata, _JsonOpt);
    }

    public static void WriteJson(AssetBundleMetadata metadata, Stream stream)
    {
        JsonSerializer.Serialize(stream, metadata, _JsonOpt);
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
