using System.Collections.Generic;
using System.Text.Json.Serialization;
using GTFO.API.Assets.UnloadStrategies;

namespace GTFO.API.Assets;

public sealed class AssetBundleMetadata
{
    /// <summary>
    /// Extension of the Metadata File
    /// <code>.meta.json</code>
    /// </summary>
    public static readonly string FileExtension = ".meta.json";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AssetBundleStartupLoadingMode StartupLoadMode { get; init; } = AssetBundleStartupLoadingMode.DontLoad;
    public string UnloadStrategy { get; init; } = nameof(AssetBundleUnloadStrategy.Unspecified);
    public bool DisableJustInTimeLoading { get; init; } = false;

    [JsonInclude] public string Checksum { get; internal set; } = "";
    [JsonInclude] public IReadOnlyList<string> Assets { get; internal set; } = [];
}

public enum AssetBundleStartupLoadingMode
{
    DontLoad,
    LoadBundle,
    LoadAllAssets
}
