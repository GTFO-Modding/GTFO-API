using GTFO.API.Assets.UnloadStrategies;

namespace GTFO.API.Assets;

public class AssetBundleLoadingOptions
{
    public AssetBundleLoadingMode? InitialLoadMode { get; set; } = null;
    public AssetBundleUnloadStrategy UnloadStrategy { get; set; } = null;
    public bool? DisableJustInTimeLoading { get; set; } = null;
}
