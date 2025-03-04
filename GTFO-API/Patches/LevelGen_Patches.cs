using HarmonyLib;
using LevelGeneration;

namespace GTFO.API.Patches
{
    [HarmonyPatch(typeof(LG_Factory))]
    internal static class LevelGen_Patches
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(LG_Factory.OnStart))]
        private static void Post_Start()
        {
            LevelAPI.FactoryStart();
        }

        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(LG_Factory.NextBatch))]
        private static void Pre_Batch(LG_Factory __instance)
        {
            if (__instance.m_batchStep > -1)
            {
                LevelAPI.AfterBuildBatch(__instance.m_currentBatchName);
            }
        }

        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(LG_Factory.NextBatch))]
        private static void Post_Batch(LG_Factory __instance)
        {
            LevelAPI.BeforeBuildBatch(__instance.m_currentBatchName);
        }

        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(LG_Factory.FactoryDone))]
        private static void Post_Finished()
        {
            LevelAPI.FactoryFinished();
        }
    }
}
