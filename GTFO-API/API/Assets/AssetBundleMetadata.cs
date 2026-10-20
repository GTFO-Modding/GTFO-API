using System.Collections.Generic;
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

    /// <summary>
    /// Defines how AssetBundle should be Loaded after Initial Startup.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AssetBundleStartupLoadingMode StartupLoadMode { get; init; } = AssetBundleStartupLoadingMode.DontLoad;

    /// <summary>
    /// Defines how AssetBundle should be Managed. To modify, check <see cref="AssetBundleReference.UnloadStrategy"/>
    /// </summary>
    public string UnloadStrategy { get; init; } = nameof(AssetBundleUnloadStrategy.Unspecified);

    /// <summary>
    /// Flag for toggling JIT-Loading
    /// </summary>
    public bool DisableJustInTimeLoading { get; init; } = false;

    /// <summary>
    /// MD5 Checksum of the <see cref="AssetBundle"/> file
    /// </summary>
    [JsonInclude] public string Checksum { get; internal set; } = "";

    /// <summary>
    /// List of Asset Names defined in <see cref="AssetBundle"/>
    /// </summary>
    [JsonInclude] public IReadOnlyList<string> Assets { get; internal set; } = [];
}

/// <summary>
/// Enum for Specify how AssetBundle should be loaded while startup
/// </summary>
public enum AssetBundleStartupLoadingMode
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
