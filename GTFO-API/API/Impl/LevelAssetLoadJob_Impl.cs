using System;
using System.Collections.Generic;
using GTFO.API.Assets;
using Il2CppInterop.Runtime.Injection;
using LevelGeneration;

namespace GTFO.API.Impl;

internal sealed class LevelAssetLoadJob_Impl : LG_FactoryJob
{
    internal enum LoadState : byte
    {
        None,
        Loading,
        Loaded
    }

    internal HashSet<AssetBundleReference> BundlesToLoad = [];
    internal HashSet<AssetReference> AssetsToLoad = [];

    private LoadState _BundlesLoadState = LoadState.None;
    private LoadState _AssetsLoadState = LoadState.None;

    public LevelAssetLoadJob_Impl(IntPtr ptr) : base(ptr) { }

    public LevelAssetLoadJob_Impl() : base(ClassInjector.DerivedConstructorPointer<LevelAssetLoadJob_Impl>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public override string GetName()
    {
        return "LoadAssetBundleAssetJob";
    }

    public override bool TakeFullFrame()
    {
        return true;
    }

    public override bool Build()
    {
        switch (_BundlesLoadState)
        {
            case LoadState.None:
                _BundlesLoadState = LoadState.Loaded;
                foreach (var bundle in BundlesToLoad)
                {
                    if (bundle.IsLoaded)
                        continue;

                    bundle.LoadBundleAsync(null);
                    _BundlesLoadState = LoadState.Loading;
                }
                return false;

            case LoadState.Loading:
                return false;
        }

        switch (_AssetsLoadState)
        {
            case LoadState.None:
                _AssetsLoadState = LoadState.Loaded;
                foreach (var asset in AssetsToLoad)
                {
                    if (asset.IsLoaded)
                        continue;

                    asset.LoadAsync(null);
                    _BundlesLoadState = LoadState.Loading;
                }
                return false;

            case LoadState.Loading:
                return false;
        }

        return true;
    }
}
