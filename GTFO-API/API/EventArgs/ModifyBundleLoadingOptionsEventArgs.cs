using GTFO.API.Assets;

namespace GTFO.API.EventArgs;

public sealed class ModifyBundleLoadingOptionsEventArgs : System.EventArgs
{
    public string AssetBundleName { get; init; }
    public AssetBundleLoadingOptions LoadingOptions { get; init; }
}
