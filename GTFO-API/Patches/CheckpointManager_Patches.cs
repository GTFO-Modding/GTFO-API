using HarmonyLib;

namespace GTFO.API.Patches;

[HarmonyPatch(typeof(CheckpointManager))]
internal static class CheckpointManager_Patches
{
    [HarmonyPatch(nameof(CheckpointManager.OnStateChange))]
    [HarmonyWrapSafe]
    [HarmonyPostfix]
    static void Pre_StateChanged(pCheckpointState oldState, pCheckpointState newState, bool isRecall)
    {
        if (!oldState.isReloadingCheckpoint && !newState.isReloadingCheckpoint)
        {
            // Ignore cases:
            // Client syncs on drop with isRecall: true.
            // Client runs a redundant StoreCheckpoint call w/ no changes prior to any change.
            if (isRecall || oldState.doorLockPosition == newState.doorLockPosition)
                return;

            EventAPI.CheckpointReached();
        }
        else if (oldState.isReloadingCheckpoint && isRecall)
        {
            EventAPI.CheckpointReloaded();
        }
    }
}
