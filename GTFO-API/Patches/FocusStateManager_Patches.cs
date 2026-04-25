using HarmonyLib;

namespace GTFO.API.Patches;

[HarmonyPatch(typeof(FocusStateManager))]
internal static class FocusStateManager_Patches
{
    [HarmonyPatch(nameof(FocusStateManager.ChangeState))]
    [HarmonyWrapSafe]
    [HarmonyPostfix]
    static void Post_StateChanged()
    {
        var old = FocusStateManager.Current.m_previousFocusState;
        var current = FocusStateManager.Current.m_currentState;
        if (old != current)
        {
            EventAPI.FocusChanged(old, current);
        }
    }
}
