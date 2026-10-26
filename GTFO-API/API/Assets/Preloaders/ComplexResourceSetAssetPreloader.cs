using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameData;
using GTFO.API.EventArg;
using HarmonyLib;

namespace GTFO.API.Assets.Preloaders;

public static class ComplexResourceSetAssetPreloader
{
    public static bool Enabled { get; private set; } = false;
    public static IDictionary<string, bool> ResourceNameLoadFlags { get; private set; } = new Dictionary<string, bool>();

    private static PropertyInfo[] _ResourceProperties;

    internal static void Setup()
    {
        var closedGeneric = typeof(Il2CppSystem.Collections.Generic.List<ResourceData>);
        _ResourceProperties = [.. AccessTools.GetDeclaredProperties(typeof(ComplexResourceSetDataBlock))
            .Where(prop => prop.PropertyType == closedGeneric && !prop.Name.EndsWith("_k__BackingField", StringComparison.Ordinal))
        ];

        SetEnabled(true);
    }

    public static void SetEnabled(bool enabled)
    {
        if (Enabled == enabled)
            return;

        Enabled = enabled;
        if (enabled)
        {
            AssetAPI.OnLoadLevelAssets += PrecacheLevelAssets;
        }
        else
        {
            AssetAPI.OnLoadLevelAssets -= PrecacheLevelAssets;
        }
    }

    private static void PrecacheLevelAssets(OnLoadLevelAssetsEventArgs arg)
    {
        var expData = LevelAPI.CurrentExpeditionData;

        if (expData == null)
        {
            APILogger.Error("CRSAssetPreloader", $"{nameof(LevelAPI)}.{nameof(LevelAPI.CurrentExpeditionData)} is null!");
            return;
        }

        var crsID = expData.Expedition.ComplexResourceData;

        if (TryGetCRSDataBlock(crsID, out var crsDB))
        {
            RegisterPrecacheAssets(arg, crsDB);
        }

        if (expData.DimensionDatas == null)
            return;

        foreach (var dim in expData.DimensionDatas)
        {
            if (!TryGetDimensionDataBlock(dim.DimensionData, out var dimDB))
                continue;

            if (dimDB.DimensionData == null)
                continue;

            if (!TryGetCRSDataBlock(dimDB.DimensionData.DimensionResourceSetID, out var dimcrsDB))
                continue;

            RegisterPrecacheAssets(arg, dimcrsDB);
        }
    }

    private static void RegisterPrecacheAssets(OnLoadLevelAssetsEventArgs arg, ComplexResourceSetDataBlock block)
    {
        foreach (var prop in _ResourceProperties)
        {
            try
            {
                if (ResourceNameLoadFlags.TryGetValue(prop.Name, out var enabled) && !enabled)
                    continue;

                var datas = (Il2CppSystem.Collections.Generic.List<ResourceData>)prop.GetValue(block);

                if (datas == null)
                    continue;

                foreach (var data in datas)
                {
                    arg.EnqueueAssetToLoad(data.Prefab);
                }
            }
            catch (Exception e)
            {
                APILogger.Error("CRSAssetPreloader", $"Exception thrown while Registering Precache Assets! : {e}");
            }
        }
    }

    private static bool TryGetCRSDataBlock(uint crsID, out ComplexResourceSetDataBlock db)
    {
        try
        {
            db = ComplexResourceSetDataBlock.s_blockByID[crsID];
            return db != null;
        }
        catch
        {
            db = null;
            return false;
        }
    }

    private static bool TryGetDimensionDataBlock(uint dimID, out DimensionDataBlock db)
    {
        try
        {
            db = DimensionDataBlock.s_blockByID[dimID];
            return db != null;
        }
        catch
        {
            db = null;
            return false;
        }
    }
}
