using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AssetShards;
using BepInEx;
using GTFO.API.Assets;
using GTFO.API.Assets.Profiles;
using GTFO.API.Attributes;
using GTFO.API.EventArg;
using GTFO.API.Impl;
using GTFO.API.Resources;
using GTFO.API.Utilities;
using GTFO.API.Utilities.IO;
using LevelGeneration;
using UnityEngine;

namespace GTFO.API;

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
    /// Invoked when <see cref="AssetReference.GetAsset"/> or <see cref="AssetReference.Asset"/> has called
    /// <para>
    /// This is not limited to JIT-loaded cases
    /// </para>
    /// </summary>
    public static event Action<OnCustomAssetRequestedEventArgs> OnCustomAssetRequested;

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

    internal static readonly Dictionary<string, AssetBundleMetadata> _BundleMetadataCache = new(StringComparer.OrdinalIgnoreCase);

    internal static void Setup()
    {
        Application.quitting += (Il2CppSystem.Action)DeleteCachedAssetBundles;
        EventAPI.OnAssetsLoaded += OnAssetsLoaded;
        OnImplReady += LoadAssetBundles;

#if DEBUG
        OnCustomAssetRequested += (args) =>
        {
            APILogger.Verbose("AssetAPI", $"CustomAsset Requested: '{args.AssetName}' from '{args.Asset.AssetSourceName}'");
        };
#endif

        AssetBundleProfile.RegisterType<AssetBundleProfile.Unspecified>();
        AssetBundleProfile.RegisterType<AssetBundleProfile.LevelAsset>();
        AssetBundleProfile.RegisterType<AssetBundleProfile.EnemyAsset>();
    }

    #region Event Invocations

    internal static void InvokeImplReady() => SafeInvoke.Invoke(OnImplReady);

    internal static void InvokeOnCustomAssetRequested(OnCustomAssetRequestedEventArgs args) => SafeInvoke.Invoke(OnCustomAssetRequested, args);

    internal static void InvokeOnLoadLevelAssets()
    {
        var loadAssetsJob = new OnLoadLevelAssetsEventArgs
        {
            Impl = new LevelAssetLoadJob_Impl()
        };
        SafeInvoke.Invoke(OnLoadLevelAssets, loadAssetsJob);

        if (!loadAssetsJob.IsLoadingQueueEmpty)
        {
            var jobQueue = LG_Factory.Current.m_batches[(int)LG_Factory.BatchName.LoadResourcesShards_Level].Jobs;
            InsertToQueue(jobQueue, 0, loadAssetsJob.Impl);
        }
    }

    #endregion Event Invocations

    #region AssetBundle API Methods

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
    /// <param name="loadingOptions"><see cref="AssetBundleLoadingOptions"/> to use</param>
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
        if (!PathingUtils.IsFileInDir(filePath, AssetBundlesPath) && !PathingUtils.IsFileInDir(filePath, AssetBundlesCachePath))
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

    #endregion AssetBundle API Methods

    #region Asset API Methods

    /// <summary>
    /// Checks if an asset is already registered in the <see cref="AssetAPI"/> or <see cref="AssetShardManager"/>
    /// </summary>
    /// <param name="assetName">Name of the asset to check</param>
    /// <returns>Whether the asset is registered or not</returns>
    public static bool ContainsAsset(string assetName)
    {
        if (AllCustomAssets.Contains(assetName))
            return true;

        if (Internal_IsShardAssetLoaded(assetName))
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
        return Internal_GetLoadedAsset(path, logErrors: true, logExceptions: true);
    }

    /// <summary>
    /// Try Obtains an asset from the currently loaded pool (<see cref="AllCustomAssets"/> and <see cref="AssetShardManager"/>)
    /// </summary>
    /// <param name="path">The path to the asset to use</param>
    /// <param name="asset">The <see cref="UnityEngine.Object"/> of the asset requested or null if it's not loaded</param>
    /// <returns><see langword="true"/> if it's loaded, otherwise <see langword="false"/></returns>
    public static bool TryGetLoadedAsset(string path, out UnityEngine.Object asset)
    {
        asset = Internal_GetLoadedAsset(path, logErrors: false, logExceptions: true);
        return asset != null;
    }

    /// <summary>
    /// Obtains an asset from the currently loaded pool (<see cref="AllCustomAssets"/> and <see cref="AssetShardManager"/>) and casts it to <typeparamref name="TAsset"/>
    /// </summary>
    /// <typeparam name="TAsset">The type of asset to load</typeparam>
    /// <param name="path">The path to the asset to use</param>
    /// <returns><typeparamref name="TAsset"/> as the asset requested or null if it's not loaded</returns>
    /// <exception cref="InvalidCastException">The loaded asset cannot be cast to <typeparamref name="TAsset"/></exception>
    public static TAsset GetLoadedAsset<TAsset>(string path) where TAsset : UnityEngine.Object
    {
        var uncastedAsset = Internal_GetLoadedAsset(path, logErrors: true, logExceptions: true);
        if (uncastedAsset == null)
        {
            return null;
        }
        return uncastedAsset.TryCast<TAsset>();
    }

    /// <summary>
    /// Try Obtains an asset from the currently loaded pool (<see cref="AllCustomAssets"/> and <see cref="AssetShardManager"/>) and casts it to <typeparamref name="TAsset"/>
    /// </summary>
    /// <typeparam name="TAsset">The type of asset to load</typeparam>
    /// <param name="path">The path to the asset to use</param>
    /// <param name="asset"><typeparamref name="TAsset"/> as the asset requested or null if it's not loaded</param>
    /// <returns><see langword="true"/> if it's loaded and can be casted into <typeparamref name="TAsset"/>, otherwise <see langword="false"/></returns>
    public static bool TryGetLoadedAsset<TAsset>(string path, out TAsset asset) where TAsset : UnityEngine.Object
    {
        var uncastedAsset = Internal_GetLoadedAsset(path, logErrors: false, logExceptions: true);
        if (uncastedAsset == null)
        {
            asset = null;
            return false;
        }

        asset = uncastedAsset.TryCast<TAsset>();
        return asset != null;
    }

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

    #endregion Asset API Methods

    private static void OnAssetsLoaded()
    {
        if (!APIStatus.Asset.Created)
        {
            APIStatus.CreateApi<AssetAPI_Impl>(nameof(APIStatus.Asset));
        }
        SafeInvoke.Invoke(OnStartupAssetsLoaded);
    }

    private static void DeleteCachedAssetBundles()
    {
        if (!Directory.Exists(AssetBundlesCachePath))
            return;

        var caches = Directory.EnumerateFiles(AssetBundlesCachePath, "*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(AssetBundleMetadata.FileExtension, StringComparison.OrdinalIgnoreCase));

        foreach (var cache in caches)
        {
            File.Delete(cache);
        }
    }

    private static void LoadAssetBundles()
    {
        var bundlesPath = Directory.CreateDirectory(AssetBundlesPath);

        var legacyPath = Path.Combine(Paths.ConfigPath, "Assets", "AssetBundles");
        if (Directory.Exists(legacyPath))
        {
            var legacyFiles = Directory.GetFiles(legacyPath, "*", SearchOption.AllDirectories);
            foreach (var legacyFile in legacyFiles)
            {
                var subPath = Path.GetRelativePath(legacyPath, legacyFile);
                var targetFile = Path.Combine(AssetBundlesPath, subPath);

                if (File.Exists(targetFile))
                    continue;

                Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
                File.Copy(legacyFile, targetFile);
                APILogger.Warn(nameof(AssetAPI), $"AssetBundle File: '{legacyFile}' has copied to '{targetFile}'");
            }
        }

        var bundlePaths = bundlesPath.EnumerateFiles("*", SearchOption.AllDirectories)
            .Where(IsBundleFile)
            .Select(file => file.FullName);

        if (!bundlePaths.Any())
            return;

        // Populate Metadata
        Parallel.ForEach(bundlePaths, (file, state) =>
        {
            try
            {
                AssetBundleMetadata metadata = AssetBundleMetadata.CheckMetadata(file);
                lock (_BundleMetadataCache)
                {
                    _BundleMetadataCache[file] = metadata;
                }
            }
            catch (Exception ex)
            {
                APILogger.Warn(nameof(AssetAPI), $"Failed to populate Metadata for bundle: '{file}' ({ex.Message})");
            }
        });

        // Do load bundle for real
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

    private static bool IsBundleFile(FileInfo info)
    {
        if (info.Extension.Equals(".manifest", StringComparison.OrdinalIgnoreCase))
            return false;

        if (info.Extension.Equals(".old", StringComparison.OrdinalIgnoreCase))
            return false;

        if (info.Name.EndsWith(AssetBundleMetadata.FileExtension, StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    internal static bool Internal_IsShardAssetLoaded(string assetName)
    {
        if (AssetShardManager.s_loadedAssetsLookup == null)
            return false;

        if (!AssetShardManager.s_loadedAssetsLookup.ContainsKey(assetName.ToUpper()))
            return false;

        return true;
    }

    internal static bool Internal_TryGetShardAsset(string assetName, out UnityEngine.Object asset)
    {
        if (AssetShardManager.s_loadedAssetsLookup == null)
        {
            asset = null;
            return false;
        }

        assetName = assetName.ToUpper();
        if (!AssetShardManager.s_loadedAssetsLookup.ContainsKey(assetName))
        {
            asset = null;
            return false;
        }

        asset = AssetShardManager.s_loadedAssetsLookup[assetName];
        return true;
    }

    internal static UnityEngine.Object Internal_GetLoadedAsset(string assetPath, bool logErrors, bool logExceptions)
    {
        APILogger.Verbose($"AssetAPI", $"Requested Asset: {assetPath}");

        try
        {
            if (AllCustomAssets.TryGet(assetPath, out var assetRef))
            {
                var loadedCustomAsset = assetRef.Asset;
                if (loadedCustomAsset == null && logErrors)
                    APILogger.Error("AssetAPI", $"Custom Asset: '{assetPath}' did not loaded!");

                return loadedCustomAsset;
            }

            if (Internal_TryGetShardAsset(assetPath, out var asset))
            {
                if (asset == null && logErrors)
                    APILogger.Error("AssetAPI", $"Asset: '{assetPath}' exists, but value is null!");

                return asset;
            }

            if (logErrors)
                APILogger.Error("AssetAPI", $"Asset: '{assetPath}' did not loaded!");

            return null;
        }
        catch (Exception e)
        {
            if (logExceptions)
            {
                APILogger.Error("AssetAPI", $"Exception thrown while loading asset: '{assetPath}'");
                APILogger.Error("AssetAPI", $"- StackTrace: {e}");
            }
            return null;
        }
    }

    private static string CopyAssetBundleCache(string assetBundleName, Stream stream)
    {
        if (!Directory.Exists(AssetBundlesCachePath))
            Directory.CreateDirectory(AssetBundlesCachePath);

        using var md5 = MD5.Create();
        var newFileName = Convert.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(assetBundleName)));
        var cacheFile = Path.Combine(AssetBundlesCachePath, newFileName);

        if (File.Exists(cacheFile))
            File.Delete(cacheFile);

        using var fs = File.Open(cacheFile, FileMode.Create, FileAccess.Write);
        stream.Seek(0, SeekOrigin.Begin);
        stream.CopyTo(fs);

        return cacheFile;
    }

    private static void InsertToQueue(Il2CppSystem.Collections.Generic.Queue<LG_FactoryJob> queue, int index, LG_FactoryJob job)
    {
        var tempList = new List<LG_FactoryJob>();
        var jobQueue = LG_Factory.Current.m_batches[(int)LG_Factory.BatchName.LoadResourcesShards_Level].Jobs;
        while (jobQueue.Count > 0)
        {
            tempList.Add(jobQueue.Dequeue());
        }

        tempList.Insert(index, job);
        foreach (var item in tempList)
        {
            jobQueue.Enqueue(item);
        }
    }
}
