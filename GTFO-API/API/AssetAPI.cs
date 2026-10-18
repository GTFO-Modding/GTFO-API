using System;
using System.IO;
using System.Linq;
using AssetShards;
using BepInEx;
using GTFO.API.Assets;
using GTFO.API.Assets.UnloadStrategies;
using GTFO.API.Attributes;
using GTFO.API.Impl;
using GTFO.API.Resources;
using GTFO.API.Utilities;
using LevelGeneration;
using UnityEngine;

namespace GTFO.API
{
    [API("Asset")]
    public static class AssetAPI
    {
        /// <summary>
        /// Status info for the <see cref="AssetAPI"/>
        /// </summary>
        public static ApiStatusInfo Status => APIStatus.Asset;

        /// <summary>
        /// Invoked when the game's startup assets have been fully loaded
        /// </summary>
        public static event Action OnStartupAssetsLoaded;

        /// <summary>
        /// Invoked when all external asset bundles have been loaded (Not invoked if no bundles)
        /// </summary>
        public static event Action OnAssetBundlesLoaded;

        public static event Action<LevelAssetLoadJob> PopulateLevelAssetLoadJob;

        /// <summary>
        /// Invoked when the internal handler is ready
        /// </summary>
        public static event Action OnImplReady;

        /// <summary>
        /// Checks if an asset is already registered in the <see cref="AssetAPI"/>
        /// </summary>
        /// <param name="assetName">Name of the asset to check</param>
        /// <returns>Whether the asset is registered or not</returns>
        public static bool ContainsAsset(string assetName)
        {
            return AssetReference.IsAssetRegistered(assetName);
        }

        /// <summary>
        /// Obtains an asset from the currently loaded asset shards
        /// </summary>
        /// <param name="path">The path to the asset to use</param>
        /// <returns>The <see cref="UnityEngine.Object"/> of the asset requested or null if it's not loaded</returns>
        public static UnityEngine.Object GetLoadedAsset(string path)
        {
            string upperPath = path.ToUpper();
            APILogger.Verbose($"Asset", $"Requested Asset: {upperPath}");
            try
            {
                if (AssetReference.AllAssets.TryGet(upperPath, out var assetRef))
                    return assetRef.Asset;

                return AssetShardManager.GetLoadedAsset(upperPath);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Obtains an asset from the currently loaded asset shards and casts it to <typeparamref name="TAsset"/>
        /// </summary>
        /// <typeparam name="TAsset">The type of asset to load</typeparam>
        /// <param name="path">The path to the asset to use</param>
        /// <returns><typeparamref name="TAsset"/> as the asset requested or null if it's not loaded</returns>
        /// <exception cref="InvalidCastException">The loaded asset cannot be cast to <typeparamref name="TAsset"/></exception>
        public static TAsset GetLoadedAsset<TAsset>(string path) where TAsset : UnityEngine.Object
            => GetLoadedAsset(path)?.Cast<TAsset>();

        /// <summary>
        /// Registers an asset into the asset shard lookup
        /// </summary>
        /// <param name="name">The name/path of the asset to use as a lookup</param>
        /// <param name="gameObject">The <see cref="UnityEngine.Object"/> that will be used</param>
        /// <exception cref="ArgumentException">The name is already registered</exception>
        public static void RegisterAsset(string name, UnityEngine.Object gameObject)
        {
            string upperName = name.ToUpper();
            if (AssetReference.IsAssetRegistered(upperName))
                throw new ArgumentException($"The asset '{name}' has already been registered.", nameof(name));

            APILogger.Verbose(nameof(AssetAPI_Impl), $"Registering asset: {name}");
            AssetReference.AllAssets[name] = new AssetReference(name, gameObject);
        }

        /// <summary>
        /// Registers an <see cref="AssetBundle"/>'s assets into the asset registry
        /// <para>Use <seealso cref="LoadAndRegisterAssetBundle(string)"/> if you want to load from a path</para>
        /// </summary>
        /// <param name="bundle">The bundle to register</param>
        [Obsolete]
        public static void RegisterAssetBundle(AssetBundle bundle)
        {
            string[] assetNames = bundle.AllAssetNames();
            APILogger.Verbose($"Asset", $"Bundle names: [{string.Join(", ", assetNames)}]");
            foreach (string assetName in assetNames)
            {
                UnityEngine.Object asset = bundle.LoadAsset(assetName);
                if (asset != null) RegisterAsset(assetName, asset);
                else APILogger.Warn("Asset", $"Skipping asset {assetName}");
            }
        }

        /// <summary>
        /// Loads an asset bundle by path and registers its assets into the asset registry
        /// </summary>
        /// <param name="pathToBundle">The location of the asset bundle</param>
        /// <exception cref="Exception">Failed to load the bundle</exception>
        [Obsolete]
        public static void LoadAndRegisterAssetBundle(string pathToBundle)
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(pathToBundle);
            if (bundle == null) throw new Exception($"Failed to load asset bundle");
            RegisterAssetBundle(bundle);
        }

        /// <summary>
        /// Loads an asset bundle from bytes and registers its assets into the asset registry
        /// </summary>
        /// <param name="bundleBytes">The raw bytes of the asset bundle</param>
        /// <exception cref="Exception">Failed to load the bundle</exception>
        [Obsolete]
        public static void LoadAndRegisterAssetBundle(byte[] bundleBytes)
        {
            AssetBundle bundle = AssetBundle.LoadFromMemory(bundleBytes);
            if (bundle == null) throw new Exception($"Failed to load asset bundle");
            RegisterAssetBundle(bundle);
        }

        /// <summary>
        /// Clones an asset into a new one with a different name
        /// </summary>
        /// <param name="assetName">The original asset name</param>
        /// <param name="copyName">The name it should be cloned into</param>
        /// <returns>The <see cref="UnityEngine.Object"/> of the asset requested or null if it's not loaded</returns>
        /// <exception cref="ArgumentException">The name you're trying to copy into is already registered</exception>
        public static UnityEngine.Object InstantiateAsset(string assetName, string copyName)
        {
            if (ContainsAsset(copyName)) throw new ArgumentException($"The asset you're trying to copy into is already registered", nameof(copyName));
            RegisterAsset(
                copyName,
                UnityEngine.Object.Instantiate(
                    GetLoadedAsset(assetName) ?? throw new ArgumentException($"Couldn't find an asset with the name '{assetName}'", nameof(assetName))
                )
            );

            return GetLoadedAsset(copyName);
        }

        /// <summary>
        /// Clones an asset into a new one with a different name
        /// </summary>
        /// <typeparam name="TAsset">Type of asset to extract</typeparam>
        /// <param name="assetName">The original asset name</param>
        /// <param name="copyName">The name it should be cloned into</param>
        /// <returns>The asset requested as <typeparamref name="TAsset"/> or null if it's not loaded</returns>
        /// <exception cref="ArgumentException">The name you're trying to copy into is already registered</exception>
        /// <exception cref="InvalidCastException">The asset cannot be cast to <typeparamref name="TAsset"/></exception>
        public static TAsset InstantiateAsset<TAsset>(string assetName, string copyName) where TAsset : UnityEngine.Object
            => InstantiateAsset(assetName, copyName)?.Cast<TAsset>();

        /// <summary>
        /// Attempts to clone an asset into a new one with a different name
        /// </summary>
        /// <typeparam name="TAsset">Type of asset to extract</typeparam>
        /// <param name="assetName">The original asset name</param>
        /// <param name="copyName">The name it should be cloned into</param>
        /// <param name="clonedObj"></param>
        /// <returns>If the asset was successfully copied and cast to <typeparamref name="TAsset"/></returns>
        /// <exception cref="ArgumentException">The name you're trying to copy into is already registered</exception>
        public static bool TryInstantiateAsset<TAsset>(string assetName, string copyName, out TAsset clonedObj) where TAsset : UnityEngine.Object
        {
            clonedObj = InstantiateAsset(assetName, copyName)?.TryCast<TAsset>();
            return clonedObj != null;
        }

        private static void OnAssetsLoaded()
        {
            if (!APIStatus.Asset.Created)
            {
                APIStatus.CreateApi<AssetAPI_Impl>(nameof(APIStatus.Asset));
            }
            SafeInvoke.Invoke(OnStartupAssetsLoaded);
        }

        internal static void InvokeImplReady() => SafeInvoke.Invoke(OnImplReady);

        internal static void Setup()
        {
            LevelAPI.OnBuildStart += OnLevelBuildStart;
            EventAPI.OnAssetsLoaded += OnAssetsLoaded;
            OnImplReady += LoadAssetBundles;

            AssetBundleUnloadStrategy.RegisterType<AssetBundleUnloadStrategy.Unspecified>();
            AssetBundleUnloadStrategy.RegisterType<AssetBundleUnloadStrategy.OnLevelBuildDone>();
            AssetBundleUnloadStrategy.RegisterType<AssetBundleUnloadStrategy.OnLevelCleanup>();
            AssetBundleUnloadStrategy.RegisterType<AssetBundleUnloadStrategy.OnLevelSelected>();
        }

        private static void OnLevelBuildStart()
        {
            var loadAssetsJob = new LevelAssetLoadJob();
            SafeInvoke.Invoke(PopulateLevelAssetLoadJob, loadAssetsJob);

            if (!loadAssetsJob.IsEmpty)
            {
                LG_Factory.InjectJob(loadAssetsJob.Impl, LG_Factory.BatchName.LoadResourcesShards_Level);
            }
        }

        private static void LoadAssetBundles()
        {
            string assetBundleDir = Path.Combine(Paths.BepInExRootPath, "Assets", "AssetBundles");

            if (!Directory.Exists(assetBundleDir))
                Directory.CreateDirectory(assetBundleDir);

            var bundlePaths = Directory.GetFiles(assetBundleDir, "*", SearchOption.AllDirectories)
                .Where(x => !x.EndsWith(".manifest", StringComparison.InvariantCultureIgnoreCase))
                .Where(x => !x.EndsWith(".old", StringComparison.InvariantCultureIgnoreCase))
                .Where(x => !x.EndsWith(AssetBundleMetadata.FileExtension, StringComparison.InvariantCultureIgnoreCase));

            if (!bundlePaths.Any())
                return;

            int loadedBundle = 0;
            foreach (var file in bundlePaths)
            {
                try
                {
                    var name = Path.GetRelativePath(assetBundleDir, file).Replace('\\', '/');
                    if (AssetBundleReference.AllAssetBundles.Contains(name))
                    {
                        APILogger.Debug(nameof(AssetAPI), $"Duplicated AssetBundle Name! '{name}'");
                        continue;
                    }

                    var assetBundleReference = new AssetBundleReference(name, file);
                    var assetCounts = assetBundleReference.Metadata.Assets.Count;
                    AssetBundleReference.AllAssetBundles[name] = assetBundleReference;

                    APILogger.Debug(nameof(AssetAPI), $"AssetBundle Loaded! Name: '{name}' AssetCount: {assetCounts}");
                    loadedBundle++;
                }
                catch (Exception ex)
                {
                    APILogger.Warn(nameof(AssetAPI), $"Failed to load AssetBundle '{file}' ({ex.Message})");
                }
            }

            if (loadedBundle > 0)
                SafeInvoke.Invoke(OnAssetBundlesLoaded);
        }
    }
}
