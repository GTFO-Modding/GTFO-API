using System;
using System.Reflection;
using AssetShards;
using BepInEx.Unity.IL2CPP.Hook;
using GTFO.API.Impl;
using GTFO.API.Resources;
using HarmonyLib;
using Il2CppInterop.Runtime.Runtime;

namespace GTFO.API.Patches
{
    [HarmonyPatch(typeof(AssetShardManager))]
    internal class AssetShardManager_Patches
    {
        private static bool s_PatchApplied = false;
        private static INativeDetour s_GetLoadedAssetDetour;
        private static GetLoadedAssetDelegate s_GetLoadedAssetOriginal;
        private delegate IntPtr GetLoadedAssetDelegate(IntPtr assetPath, byte* forceAssetDatabase, Il2CppMethodInfo* methodInfo);

        [HarmonyPatch(nameof(AssetShardManager.Setup))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        public static void Setup_Postfix()
        {
            if (APIStatus.Asset.Created) return;
            APIStatus.CreateApi<AssetAPI_Impl>(nameof(APIStatus.Asset));
        }

        internal static unsafe void CreateNativePatch()
        {
            if (s_PatchApplied)
                return;

            var methodName = nameof(AssetShardManager.GetLoadedAsset);
            var returnName = "T";

            s_GetLoadedAssetDetour = Il2CppAPI.CreateGenericDetour<AssetShardManager, GetLoadedAssetDelegate>(methodName, returnName,
            [
                typeof(string).FullName,
                typeof(bool).MakeByRefType().FullName
            ], [typeof(UnityEngine.Object)], Patch_GetLoadedAsset, out s_GetLoadedAssetOriginal);
            s_PatchApplied = true;
        }

        private static unsafe IntPtr Patch_GetLoadedAsset(IntPtr assetPath, byte* forceAssetDatabase, Il2CppMethodInfo* methodInfo)
        {
            string assetPathStr = new Il2CppSystem.String(assetPath);
            if (AssetAPI.AllCustomAssets.TryGet(assetPathStr, out var assetRef))
            {
                assetRef.GetAsset();
            }

            return s_GetLoadedAssetOriginal(assetPath, forceAssetDatabase, methodInfo);
        }

        [HarmonyPatch]
        private static class GetLoadedAsset
        {
            public static MethodBase TargetMethod()
            {
                return AccessTools.FirstMethod(typeof(AssetShardManager), IsTargetMethod);
            }

            public static bool IsTargetMethod(MethodBase method)
            {
                return method.Name == nameof(AssetShardManager.GetLoadedAsset) && !method.IsGenericMethod;
            }

            public static void Prefix(string assetPath)
            {
                if (AssetAPI.AllCustomAssets.TryGet(assetPath, out var assetRef))
                {
                    assetRef.GetAsset();
                }
            }
        }
    }
}
