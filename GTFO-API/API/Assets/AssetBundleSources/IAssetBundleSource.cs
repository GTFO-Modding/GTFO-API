using GTFO.API.EventArg;
using UnityEngine;

namespace GTFO.API.Assets.AssetBundleSources;

internal interface IAssetBundleSource
{
    public string SourceName { get; }
    public AssetBundle LoadedAssetBundle { get; }
    public AssetBundleMetadata Metadata { get; }
    public bool CanManualLoad { get; }
    public bool CanManualUnload { get; }
    public void Load();
    public void LoadAsync(UnityObjectRequestEvent onLoaded);
    public void Unload();
}
