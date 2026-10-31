using System;
using AssetShards;
using BepInEx.Unity.IL2CPP.Hook;
using Il2CppInterop.Runtime.Runtime;

namespace GTFO.API.NativePatches;

/// <summary>
/// Managed surface for Harmony Patching NativeDetoured Method
/// </summary>
public static class AssetShardManager_Native
{
    private static bool s_IsPatched = false;

    private static INativeDetour s_GetLoadedAssetDetour;
    private static GetLoadedAssetDelegate s_GetLoadedAssetOriginal;
    private delegate IntPtr GetLoadedAssetDelegate(IntPtr assetPath, byte* forceAssetDatabase, Il2CppMethodInfo* methodInfo);

    internal static unsafe void CreateNativePatch()
    {
        if (s_IsPatched)
            return;

        var methodName = nameof(AssetShardManager.GetLoadedAsset);
        var returnName = "T";

        APILogger.Verbose(nameof(AssetShardManager_Native), "Applying GetLoadedAsset<T> Patch");
        s_GetLoadedAssetDetour = Il2CppAPI.CreateGenericDetour<AssetShardManager, GetLoadedAssetDelegate>(methodName, returnName,
        [
            typeof(string).FullName,
            typeof(bool).MakeByRefType().FullName
        ], [typeof(UnityEngine.Object)], GetLoadedAsset__T__String_Boolean, out s_GetLoadedAssetOriginal);

        s_IsPatched = true;
    }

    /// <summary>
    /// Detoured Method of <see cref="AssetShardManager.GetLoadedAsset{T}"/>
    /// </summary>
    public static unsafe IntPtr GetLoadedAsset__T__String_Boolean(IntPtr n_assetPath, byte* n_forceAssetDatabase, Il2CppMethodInfo* n_methodInfo)
    {
        return s_GetLoadedAssetOriginal(n_assetPath, n_forceAssetDatabase, n_methodInfo);
    }
}
