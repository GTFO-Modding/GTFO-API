using GTFO.API.Assets.Profiles;

namespace GTFO.API.Assets;

/// <summary>
/// Options to use when loading <see cref="AssetBundleReference"/>
/// </summary>
public class AssetBundleLoadingOptions
{
    /// <summary>
    /// Defines how AssetBundle should be Loaded after loaded.
    /// </summary>
    public AssetBundleLoadingMode? InitialLoadMode { get; set; } = null;

    /// <summary>
    /// Defines how AssetBundle should be Managed.
    /// </summary>
    public AssetBundleProfile Profile { get; set; } = null;

    /// <summary>
    /// Flag for toggling JIT-Loading
    /// </summary>
    public bool? DisableJustInTimeLoading { get; set; } = null;
}
