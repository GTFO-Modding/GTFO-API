using HarmonyLib;

namespace GTFO.API.Patches;

[HarmonyPatch(typeof(GameStateManager))]
internal static class GameStateManager_Patches
{
    [HarmonyPatch(nameof(GameStateManager.DoChangeState))]
    [HarmonyWrapSafe]
    [HarmonyPrefix]
    static void Pre_StateChanged(GameStateManager __instance, ref eGameStateName __state)
    {
        __state = __instance.m_currentStateName;
    }

    [HarmonyPatch(nameof(GameStateManager.DoChangeState))]
    [HarmonyWrapSafe]
    [HarmonyPostfix]
    static void Post_StateChanged(GameStateManager __instance, eGameStateName __state)
    {
        var old = __state;
        var current = __instance.m_currentStateName;
        if (old != current)
        {
            EventAPI.GameStateChanged(old, current);
        }
    }
}
