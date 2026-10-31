using System;
using BepInEx.Unity.IL2CPP.Hook;
using Gear;
using Il2CppInterop.Runtime.Runtime;

namespace GTFO.API.NativePatches;

/// <summary>
/// Managed surface for Harmony Patching NativeDetoured Method
/// </summary>
public static class CellJSON_Native
{
    private static bool s_IsPatched = false;

    private static INativeDetour s_ReadFromDiskDetour;
    private static ReadFromDiskDelegate s_ReadFromDiskOriginal;
    private delegate IntPtr ReadFromDiskDelegate(IntPtr path, byte* createNew, Il2CppMethodInfo* methodInfo);

    private static INativeDetour s_SaveToDiskDetour;
    private static SaveToDiskDelegate s_SaveToDiskOriginal;
    private delegate void SaveToDiskDelegate(IntPtr path, IntPtr obj, Il2CppMethodInfo* methodInfo);

    internal static unsafe void CreateNativePatch()
    {
        if (s_IsPatched)
            return;

        var readMethod = nameof(CellJSON.ReadFromDisk);
        var saveMethod = nameof(CellJSON.SaveToDisk);

        APILogger.Verbose(nameof(CellJSON_Native), "Applying ReadFromDisk Patch");
        s_ReadFromDiskDetour = Il2CppAPI.CreateGenericDetour<CellJSON, ReadFromDiskDelegate>(readMethod, "T", new[]
        {
            typeof(string).FullName,
            typeof(bool).MakeByRefType().FullName
        }, [typeof(GearFavoritesData)], ReadFromDisk__T__String_BooleanByRef, out s_ReadFromDiskOriginal);

        APILogger.Verbose(nameof(CellJSON_Native), "Applying SaveToDisk Patch");
        s_SaveToDiskDetour = Il2CppAPI.CreateGenericDetour<CellJSON, SaveToDiskDelegate>(saveMethod, typeof(void).FullName, new string[]
        {
            typeof(string).FullName,
            "T"
        }, [typeof(GearFavoritesData)], SaveToDisk__Void__String_T, out s_SaveToDiskOriginal);

        s_IsPatched = true;
    }

    /// <summary>
    /// Detoured Method of <see cref="CellJSON.ReadFromDisk"/>
    /// </summary>
    public static unsafe IntPtr ReadFromDisk__T__String_BooleanByRef(IntPtr n_path, byte* n_createdNew, Il2CppMethodInfo* n_methodInfo)
    {
        return s_ReadFromDiskOriginal(n_path, n_createdNew, n_methodInfo);
    }

    /// <summary>
    /// Detoured Method of <see cref="CellJSON.SaveToDisk{T}(string, T)"/>
    /// </summary>
    public static unsafe void SaveToDisk__Void__String_T(IntPtr n_path, IntPtr n_data, Il2CppMethodInfo* n_methodInfo)
    {
        s_SaveToDiskOriginal(n_path, n_data, n_methodInfo);
    }
}
