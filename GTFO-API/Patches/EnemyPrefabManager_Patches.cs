using System;
using Enemies;
using HarmonyLib;

namespace GTFO.API.Patches;

[HarmonyPatch(typeof(EnemyPrefabManager))]
internal static class EnemyPrefabManager_Patches
{
    public static event Action OnAllPrefabBuilt;

    [HarmonyPatch(nameof(EnemyPrefabManager.GenerateAllEnemyPrefabs))]
    [HarmonyWrapSafe]
    [HarmonyPostfix]
    private static void Postfix()
    {
        OnAllPrefabBuilt?.Invoke();
    }
}
