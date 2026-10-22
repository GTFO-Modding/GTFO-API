using System;
using GTFO.API.Assets;

namespace GTFO.API.EventArg;

public sealed class OnCustomAssetRequestedEventArgs : EventArgs
{
    public string AssetName { get; init; }
    public AssetReference Asset { get; init; }
}
