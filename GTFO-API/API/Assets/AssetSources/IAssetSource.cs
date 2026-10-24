using GTFO.API.EventArg;

namespace GTFO.API.Assets.AssetSources;

internal interface IAssetSource
{
    public string SourceName { get; }
    public UnityEngine.Object LoadedAsset { get; }
    public bool CanManualLoad { get; }
    public bool CanManualUnload { get; }
    public void Load();
    public void LoadAsync(UnityObjectRequestEvent onLoaded);
    public void Unload();
}
