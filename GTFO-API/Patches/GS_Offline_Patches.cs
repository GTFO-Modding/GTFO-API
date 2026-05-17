using HarmonyLib;

namespace GTFO.API.Patches;

[HarmonyPatch(typeof(GS_Offline))]
internal static class GS_Offline_Patches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GS_Offline.Update))]
    static bool Prefix()
    {
        if (LoadingAPI.AllJobsCompleted)
        {
            return true; //Run Original
        }

        return false; //Skip Original
    }
}
