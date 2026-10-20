using System;

namespace GTFO.API.Assets.AssetSources;

internal interface IAssetSource
{
    public string SourceName { get; }
    public UnityEngine.Object LoadedAsset { get; }
    public void Load();
    public void LoadAsync(Action onLoaded);
    public void Unload();
}
