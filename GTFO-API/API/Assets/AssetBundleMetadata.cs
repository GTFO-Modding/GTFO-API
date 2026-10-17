using System;
using System.Collections.Generic;
using System.Globalization;
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

    /// <summary>
    /// DataTime Format string for LastModified
    /// </summary>
    public static readonly string LastModifiedFormat = "yyyy-MM-dd HH:mm:ss";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AssetBundleStartupLoadingMode StartupLoadMode { get; init; } = AssetBundleStartupLoadingMode.DontLoad;
    public string UnloadStrategy { get; init; } = nameof(AssetBundleUnloadStrategy.Unspecified);
    public bool DisableJustInTimeLoading { get; init; } = false;

    [JsonInclude] public string LastModified { get; internal set; } = DateTime.MinValue.ToString(LastModifiedFormat, CultureInfo.InvariantCulture);
    [JsonInclude] public IReadOnlyList<string> Assets { get; internal set; } = [];
}

public enum AssetBundleStartupLoadingMode
{
    DontLoad,
    LoadBundle,
    LoadAllAssets
}
