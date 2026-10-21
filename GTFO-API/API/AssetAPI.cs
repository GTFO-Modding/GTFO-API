using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AssetShards;
using BepInEx;
using GTFO.API.Assets;
using GTFO.API.Assets.Profiles;
using GTFO.API.Attributes;
using GTFO.API.EventArg;
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

        /// <summary>
        /// Invoked when start loading Level generation Assets
        /// <para><seealso cref="OnLoadLevelAssetsEventArgs"/> is provided to load assets before starting level generation.</para>
        /// </summary>
        public static event Action<OnLoadLevelAssetsEventArgs> OnLoadLevelAssets;

        /// <summary>
        /// Invoked when new <see cref="AssetBundleReference"/> has loaded
        /// <para><seealso cref="OnAssetBundleLoadedEventArgs"/> is provided to modify <see cref="AssetBundleLoadingOptions"/> to use.</para>
        /// </summary>
        public static event Action<OnAssetBundleLoadedEventArgs> OnAssetBundleLoaded;

        /// <summary>
        /// Invoked when the internal handler is ready
        /// </summary>
        public static event Action OnImplReady;

        /// <summary>
        /// List of every <u>custom</u> assets. <u>This does NOT include Vanilla Assets!</u>
        /// </summary>
        public static AssetRegistry AllCustomAssets { get; private set; } = new();

        /// <summary>
        /// List of every <u>custom</u> <see cref="AssetBundle"/>. <u>This does NOT include Vanilla AssetBundles!</u>
        /// </summary>
        public static AssetBundleRegistry AllCustomBundles { get; private set; } = new();

        /// <summary>
        /// Path to AssetBundles: 'BepInEx/Assets/AssetBundles'
        /// </summary>
        public static readonly string AssetBundlesPath = Path.Combine(Paths.BepInExRootPath, "Assets", "AssetBundles");

        /// <summary>
        /// Path to AssetBundle Caches: 'BepInEx/cache/assetbundles'
        /// </summary>
        public static readonly string AssetBundlesCachePath = Path.Combine(Paths.BepInExRootPath, "cache", "assetbundles");

        /// <summary>
        /// Checks if an <see cref="AssetBundle"/> is already registered in the <see cref="AssetAPI"/>
        /// </summary>
        /// <param name="assetBundleName">Name of the <see cref="AssetBundle"/> to check</param>
        /// <returns>Whether the <see cref="AssetBundle"/> is registered or not</returns>
        public static bool ContainsAssetBundle(string assetBundleName)
        {
            if (AllCustomBundles.Contains(assetBundleName))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if an asset is already registered in the <see cref="AssetAPI"/> or <see cref="AssetShardManager"/>
        /// </summary>
        /// <param name="assetName">Name of the asset to check</param>
        /// <returns>Whether the asset is registered or not</returns>
        public static bool ContainsAsset(string assetName)
        {
            if (AssetShardManager.s_loadedAssetsLookup.ContainsKey(assetName.ToUpper()))
                return true;

            if (AllCustomAssets.Contains(assetName))
                return true;

            return false;
        }

        /// <summary>
        /// Obtains an asset from the currently loaded pool (<see cref="AllCustomAssets"/> and <see cref="AssetShardManager"/>)
        /// </summary>
        /// <param name="path">The path to the asset to use</param>
        /// <returns>The <see cref="UnityEngine.Object"/> of the asset requested or null if it's not loaded</returns>
        public static UnityEngine.Object GetLoadedAsset(string path)
        {
            APILogger.Verbose($"Asset", $"Requested Asset: {path}");
            try
            {
                if (AllCustomAssets.TryGet(path, out var assetRef))
                    return assetRef.Asset;

                return AssetShardManager.GetLoadedAsset(path);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Obtains an asset from the currently loaded pool (<see cref="AllCustomAssets"/> and <see cref="AssetShardManager"/>) and casts it to <typeparamref name="TAsset"/>
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
            if (ContainsAsset(name))
                throw new ArgumentException($"The asset '{name}' has already been registered.", nameof(name));

            APILogger.Verbose(nameof(AssetAPI_Impl), $"Registering asset: {name}");
            AllCustomAssets[name] = new AssetReference(name, gameObject);
        }

        /// <summary>
        /// Registers an <see cref="AssetBundle"/> to Game
        /// <para>
        /// Alternatives:<br/>
        /// - <seealso cref="RegisterAssetBundle(string, string, AssetBundleLoadingOptions)"/><br/>
        /// - <seealso cref="RegisterAssetBundle(string, byte[], AssetBundleLoadingOptions)"/><br/>
        /// - <seealso cref="RegisterAssetBundle(string, Stream, AssetBundleLoadingOptions)"/><br/>
        /// </para>
        /// </summary>
        /// <param name="assetBundleName">Unique Name of Bundle</param>
        /// <param name="bundle">AssetBundle to Reference</param>
        /// <returns><seealso cref="AssetBundleReference"/> Instance (Loading/Unloading Is NOT supported)</returns>
        /// <exception cref="ArgumentException"></exception>
        public static AssetBundleReference RegisterAssetBundle(string assetBundleName, AssetBundle bundle, AssetBundleLoadingOptions loadingOptions = null)
        {
            if (ContainsAssetBundle(assetBundleName))
                throw new ArgumentException($"The assetbundle '{assetBundleName}' has already been registered.", nameof(assetBundleName));

            var assetBundleReference = new AssetBundleReference(assetBundleName, bundle);
            AllCustomBundles[assetBundleName] = assetBundleReference;

            var eventArgs = new OnAssetBundleLoadedEventArgs(assetBundleName, loadingOptions);
            SafeInvoke.Invoke(OnAssetBundleLoaded, eventArgs);
            assetBundleReference.Initialize(loadingOptions);

            return assetBundleReference;
        }

        /// <summary>
        /// Registers an <see cref="AssetBundle"/> to <see cref="AssetBundleRegistry"/>
        /// </summary>
        /// <param name="assetBundleName">Unique Name of Bundle</param>
        /// <param name="bytes">Bundle file bytes</param>
        /// <param name="loadingOptions"><see cref="AssetBundleLoadingOptions"/> to use</param>
        /// <returns><seealso cref="AssetBundleReference"/> Instance</returns>
        /// <exception cref="ArgumentException"></exception>
        public static AssetBundleReference RegisterAssetBundle(string assetBundleName, byte[] bytes, AssetBundleLoadingOptions loadingOptions = null)
        {
            if (ContainsAssetBundle(assetBundleName))
                throw new ArgumentException($"The assetbundle '{assetBundleName}' has already been registered.", nameof(assetBundleName));

            var filePath = CopyAssetBundleCache(assetBundleName, new MemoryStream(bytes));
            var assetBundleReference = new AssetBundleReference(assetBundleName, filePath);
            AllCustomBundles[assetBundleName] = assetBundleReference;

            var eventArgs = new OnAssetBundleLoadedEventArgs(assetBundleName, loadingOptions);
            SafeInvoke.Invoke(OnAssetBundleLoaded, eventArgs);
            assetBundleReference.Initialize(loadingOptions);

            return assetBundleReference;
        }

        /// <summary>
        /// Registers an <see cref="AssetBundle"/> to <see cref="AssetBundleRegistry"/>
        /// </summary>
        /// <param name="assetBundleName">Unique Name of Bundle</param>
        /// <param name="stream">Stream of Bundle file</param>
        /// <param name="loadingOptions"><see cref="AssetBundleLoadingOptions"/> to use</param>
        /// <returns><seealso cref="AssetBundleReference"/> Instance</returns>
        /// <exception cref="ArgumentException"></exception>
        public static AssetBundleReference RegisterAssetBundle(string assetBundleName, Stream stream, AssetBundleLoadingOptions loadingOptions = null)
        {
            if (ContainsAssetBundle(assetBundleName))
                throw new ArgumentException($"The assetbundle '{assetBundleName}' has already been registered.", nameof(assetBundleName));

            var filePath = CopyAssetBundleCache(assetBundleName, stream);
            var assetBundleReference = new AssetBundleReference(assetBundleName, filePath);
            AllCustomBundles[assetBundleName] = assetBundleReference;

            var eventArgs = new OnAssetBundleLoadedEventArgs(assetBundleName, loadingOptions);
            SafeInvoke.Invoke(OnAssetBundleLoaded, eventArgs);
            assetBundleReference.Initialize(loadingOptions);

            return assetBundleReference;
        }

        /// <summary>
        /// Registers an <see cref="AssetBundle"/> to <see cref="AssetBundleRegistry"/>
        /// </summary>
        /// <param name="assetBundleName">Unique Name of Bundle</param>
        /// <param name="filePath">Location of the Bundle file</param>
        /// <param name="loadingOptions"><see cref="AssetBundleLoadingOptions"/> to use</param>
        /// <returns><seealso cref="AssetBundleReference"/> Instance</returns>
        /// <exception cref="ArgumentException"></exception>
        public static AssetBundleReference RegisterAssetBundle(string assetBundleName, string filePath, AssetBundleLoadingOptions loadingOptions = null)
        {
            if (ContainsAssetBundle(assetBundleName))
                throw new ArgumentException($"The assetbundle '{assetBundleName}' has already been registered.", nameof(assetBundleName));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The assetbundle file '{filePath}' doesn't exists.");

            // Handle External Files
            // Copy them to .cache folder
            if (!IsFileInDir(filePath, AssetBundlesPath) && !IsFileInDir(filePath, AssetBundlesCachePath))
            {
                using var fs = File.OpenRead(filePath);
                filePath = CopyAssetBundleCache(assetBundleName, fs);
            }

            var assetBundleReference = new AssetBundleReference(assetBundleName, filePath);
            AllCustomBundles[assetBundleName] = assetBundleReference;

            var eventArgs = new OnAssetBundleLoadedEventArgs(assetBundleName, loadingOptions);
            SafeInvoke.Invoke(OnAssetBundleLoaded, eventArgs);
            assetBundleReference.Initialize(loadingOptions);

            return assetBundleReference;
        }

        private static bool IsFileInDir(string filePath, string dirPath)
        {
            dirPath = Path.GetFullPath(dirPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            filePath = Path.GetFullPath(filePath);
            return filePath.StartsWith(dirPath, StringComparison.OrdinalIgnoreCase);
        }

        private static string CopyAssetBundleCache(string assetBundleName, Stream stream)
        {
            if (!Directory.Exists(AssetBundlesCachePath))
                Directory.CreateDirectory(AssetBundlesCachePath);

            var newFileName = GetHashedName(assetBundleName);
            var cacheFile = Path.Combine(AssetBundlesCachePath, newFileName);

            if (File.Exists(cacheFile))
                File.Delete(cacheFile);

            using var fs = File.Open(cacheFile, FileMode.Create, FileAccess.Write);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fs);

            return cacheFile;
        }

        private static string GetHashedName(string name)
        {
            using var md5 = MD5.Create();
            return Convert.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(name)));
        }

        #region Obsolete
        private const string REGISTERBUNDLE_OBSOLETE = "Legacy RegisterBundle methods will register AssetBundleReference name as random UUID.";

        /// <summary>
        /// Registers an <see cref="AssetBundle"/>'s assets into the asset registry
        /// <para>Use <seealso cref="LoadAndRegisterAssetBundle(string)"/> if you want to load from a path</para>
        /// </summary>
        /// <param name="bundle">The bundle to register</param>
        [Obsolete($"{REGISTERBUNDLE_OBSOLETE} Use AssetAPI.RegisterAssetBundle(string assetBundleName, AssetBundle bundle)")]
        public static void RegisterAssetBundle(AssetBundle bundle)
        {
            APILogger.Warn(nameof(AssetAPI), $"Bundle has registered via Legacy '{nameof(RegisterAssetBundle)}' This is not recommended!");
            APILogger.Warn(nameof(AssetAPI), $"StackTrace: {Environment.StackTrace}");
            RegisterAssetBundle(Guid.NewGuid().ToString(), bundle);
        }

        /// <summary>
        /// Loads an asset bundle by path and registers its assets into the asset registry
        /// </summary>
        /// <param name="pathToBundle">The location of the asset bundle</param>
        /// <exception cref="Exception">Failed to load the bundle</exception>
        [Obsolete($"{REGISTERBUNDLE_OBSOLETE} Use AssetAPI.RegisterAssetBundle(string assetBundleName, string filePath)")]
        public static void LoadAndRegisterAssetBundle(string pathToBundle)
        {
            APILogger.Warn(nameof(AssetAPI), $"Bundle has registered via Legacy '{nameof(LoadAndRegisterAssetBundle)}' This is not recommended!");
            APILogger.Warn(nameof(AssetAPI), $"StackTrace: {Environment.StackTrace}");
            RegisterAssetBundle(Guid.NewGuid().ToString(), pathToBundle);
        }

        /// <summary>
        /// Loads an asset bundle from bytes and registers its assets into the asset registry
        /// </summary>
        /// <param name="bundleBytes">The raw bytes of the asset bundle</param>
        /// <exception cref="Exception">Failed to load the bundle</exception>
        [Obsolete($"{REGISTERBUNDLE_OBSOLETE} Use AssetAPI.RegisterAssetBundle(string assetBundleName, byte[] bytes)")]
        public static void LoadAndRegisterAssetBundle(byte[] bundleBytes)
        {
            APILogger.Warn(nameof(AssetAPI), $"Bundle has registered via Legacy '{nameof(LoadAndRegisterAssetBundle)}' This is not recommended!");
            APILogger.Warn(nameof(AssetAPI), $"StackTrace: {Environment.StackTrace}");
            RegisterAssetBundle(Guid.NewGuid().ToString(), bundleBytes);
        }
        #endregion

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
            Application.quitting += (Il2CppSystem.Action)(() =>
            {
                var caches = Directory.EnumerateFiles(AssetBundlesCachePath, "*", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(AssetBundleMetadata.FileExtension, StringComparison.OrdinalIgnoreCase));

                foreach (var cache in caches)
                {
                    File.Delete(cache);
                }
            });

            LevelAPI.OnBuildStart += OnLevelBuildStart;
            EventAPI.OnAssetsLoaded += OnAssetsLoaded;
            OnImplReady += LoadAssetBundles;

            AssetBundleProfile.RegisterType<AssetBundleProfile.Unspecified>();
            AssetBundleProfile.RegisterType<AssetBundleProfile.LevelAsset>();
        }

        private static void OnLevelBuildStart()
        {
            var loadAssetsJob = new OnLoadLevelAssetsEventArgs
            {
                Impl = new LevelAssetLoadJob_Impl()
            };
            SafeInvoke.Invoke(OnLoadLevelAssets, loadAssetsJob);

            if (!loadAssetsJob.IsLoadingQueueEmpty)
            {
                LG_Factory.InjectJob(loadAssetsJob.Impl, LG_Factory.BatchName.LoadResourcesShards_Level);
            }
        }

        private static void LoadAssetBundles()
        {
            if (!Directory.Exists(AssetBundlesPath))
                Directory.CreateDirectory(AssetBundlesPath);

            var bundlePaths = Directory.GetFiles(AssetBundlesPath, "*", SearchOption.AllDirectories)
                .Where(x => !x.EndsWith(".manifest", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.EndsWith(".old", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.EndsWith(AssetBundleMetadata.FileExtension, StringComparison.OrdinalIgnoreCase));

            if (!bundlePaths.Any())
                return;

            int loadedBundle = 0;
            foreach (var file in bundlePaths)
            {
                try
                {
                    var name = Path.GetRelativePath(AssetBundlesPath, file).Replace('\\', '/');
                    if (ContainsAssetBundle(name))
                    {
                        APILogger.Debug(nameof(AssetAPI), $"Duplicated AssetBundle Name! '{name}'");
                        continue;
                    }

                    var assetBundleReference = RegisterAssetBundle(name, file);
                    var assetCounts = assetBundleReference.Metadata.Assets.Count;

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
