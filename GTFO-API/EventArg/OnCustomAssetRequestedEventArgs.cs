using System;
using GTFO.API.Assets;

namespace GTFO.API.EventArg;

/// <summary>
/// EventArgs for <see cref="AssetAPI.OnCustomAssetRequested"/>
/// </summary>
public sealed class OnCustomAssetRequestedEventArgs : EventArgs
{
    /// <summary>
    /// Full Name of the Asset that been Requested
    /// </summary>
    public string AssetName { get; init; }

    /// <summary>
    /// Requested <see cref="AssetReference"/>
    /// </summary>
    public AssetReference Asset { get; init; }
}
