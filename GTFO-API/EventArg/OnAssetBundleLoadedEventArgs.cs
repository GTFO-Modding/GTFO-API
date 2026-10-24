using System;
using GTFO.API.Assets;

namespace GTFO.API.EventArg;

/// <summary>
/// EventArgs for <see cref="AssetAPI.OnAssetBundleLoaded"/>
/// </summary>
public sealed class OnAssetBundleLoadedEventArgs(string assetBundleName, AssetBundleLoadingOptions loadingOptions = null) : EventArgs
{
    /// <summary>
    /// Name of the AssetBundle
    /// </summary>
    public string AssetBundleName { get; init; } = assetBundleName;

    /// <summary>
    /// <see cref="AssetBundleLoadingOptions"/> to Apply, Modify this to take effect
    /// </summary>
    public AssetBundleLoadingOptions LoadingOptions { get; init; } = loadingOptions ?? new AssetBundleLoadingOptions();
}
