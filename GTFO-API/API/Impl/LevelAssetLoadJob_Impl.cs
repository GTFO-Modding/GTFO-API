using System;
using System.Collections.Generic;
using System.Linq;
using GTFO.API.Assets;
using Il2CppInterop.Runtime.Injection;
using LevelGeneration;

namespace GTFO.API.Impl;

internal sealed class LevelAssetLoadJob_Impl : LG_FactoryJob
{
    private static readonly APIModuledLogger Log = new(moduleName: "LevelAssetLoadJob");

    internal HashSet<AssetBundleReference> BundlesToLoad = [];
    internal HashSet<AssetReference> AssetsToLoad = [];

    private bool _IsLoading = false;

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
        if (!_IsLoading)
        {
            StartLoad();
        }

        return BundlesToLoad.All(bundle => bundle.IsLoaded) && AssetsToLoad.All(asset => asset.IsLoaded);
    }

    private void StartLoad()
    {
        var bundleLoadedLookup = new Dictionary<string, Action>();
        foreach (var asset in AssetsToLoad)
        {
            if (asset.AssetSourceBundle != null)
            {
                if (bundleLoadedLookup.ContainsKey(asset.AssetSourceBundle.Name))
                    bundleLoadedLookup[asset.AssetSourceBundle.Name] += loadAsset;
                else
                    bundleLoadedLookup[asset.AssetSourceBundle.Name] = loadAsset;
            }
            else
            {
                loadAsset();
            }

            void loadAsset()
            {
                Log.Debug($"Start Loading Asset: {asset.Name}");
                asset.LoadAsync((args) =>
                {
                    if (!args.Success)
                    {
                        Log.Error($"Asset: '{asset.Name}' has failed to load! Level will not be loaded!!!");
                        return;
                    }

                    Log.Debug($"Asset Loaded: '{asset.Name}' (took {(long)args.Elapsed.TotalMilliseconds}ms)");
                });
            }
        }

        foreach (var bundle in BundlesToLoad)
        {
            Log.Debug($"Start Loading Bundle: '{bundle.Name}'");
            bundle.LoadBundleAsync((args) =>
            {
                if (!args.Success)
                {
                    Log.Error($"Bundle: '{bundle.Name}' has failed to load! Level will not be loaded!!!");
                    return;
                }

                Log.Debug($"Bundle Loaded: '{bundle.Name}' (took {(long)args.Elapsed.TotalMilliseconds}ms)");
                if (bundleLoadedLookup.TryGetValue(bundle.Name, out var onBundleLoaded))
                    onBundleLoaded?.Invoke();
            });
        }

        _IsLoading = true;
    }
}
