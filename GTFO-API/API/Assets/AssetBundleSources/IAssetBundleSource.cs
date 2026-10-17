using System;
using UnityEngine;

namespace GTFO.API.Assets.AssetBundleSources;

internal interface IAssetBundleSource
{
    public AssetBundle LoadedAssetBundle { get; }
    public AssetBundleMetadata Metadata { get; }
    public void Load();
    public void LoadAsync(Action onLoaded);
    public void Unload(bool unloadAllAssets);
}
