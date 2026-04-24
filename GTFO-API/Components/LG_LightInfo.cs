using System.Collections.Generic;
using CullingSystem;
using GameData;
using GTFO.API.Utilities;
using LevelGeneration;
using UnityEngine;

namespace GTFO.API.Components;

/// <summary>
/// Lookup for Level-Generated Light's Information
/// </summary>
public sealed class LG_LightInfo : MonoBehaviour
{
    /// <summary>
    /// Reference to Owning LG_Light Instance
    /// </summary>
    public LG_Light OwnerLight { get; internal set; }

    /// <summary>
    /// Light Snapshot for the Prefab State (Before the <see cref="LightSettingsDataBlock"/> applied)
    /// </summary>
    public LightSnapshot PrefabSnapshot { get; internal set; }

    /// <summary>
    /// Light Snapshot for the Start State (After the <see cref="LightSettingsDataBlock"/> applied)
    /// </summary>
    public LightSnapshot StartSnapshot { get; internal set; }

    /// <summary>
    /// Light Updator Instance that LG_Light for the Start State (After the <see cref="LightSettingsDataBlock"/> applied)
    /// </summary>
    public iC_LightUpdator StartLightUpdator { get; internal set; }

    /// <summary>
    /// Apply <see cref="PrefabSnapshot"/> to the Light
    /// </summary>
    public void ApplyPrefabSettings()
    {
        PrefabSnapshot.Apply(OwnerLight);
    }

    /// <summary>
    /// Apply <see cref="StartSnapshot"/> to the Light
    /// </summary>
    public void ApplyStartSettings()
    {
        StartSnapshot.Apply(OwnerLight);
    }

    /// <summary>
    /// Apply <see cref="StartLightUpdator"/> to the Light
    /// </summary>
    public void ApplyStartLightUpdator()
    {
        OwnerLight.GetC_Light().LightUpdator = StartLightUpdator;
    }

    static LG_LightInfo()
    {
        LevelAPI.OnBeforeBuildBatch += (LG_Factory.BatchName batch) =>
        {
            if (batch != LG_Factory.BatchName.ZoneLights)
                return;

            foreach (var light in GetAllLights(byGetComponent: true))
            {
                var lightInfo = light.GetComponent<LG_LightInfo>();
                if (lightInfo == null)
                {
                    lightInfo = light.gameObject.AddComponent<LG_LightInfo>();
                    lightInfo.PrefabSnapshot = LightSnapshot.Capture(light);
                }
            }
        };

        LevelAPI.OnAfterBuildBatch += (LG_Factory.BatchName batch) =>
        {
            if (batch != LG_Factory.BatchName.ZoneLights)
                return;

            foreach (var light in GetAllLights(byGetComponent: false))
            {
                var lightInfo = light.GetComponent<LG_LightInfo>();
                if (lightInfo == null)
                    continue;

                lightInfo.StartSnapshot = LightSnapshot.Capture(light);
            }
        };
    }

    private static IEnumerable<LG_Light> GetAllLights(bool byGetComponent = false)
    {
        foreach (var zone in Builder.CurrentFloor.allZones)
        {
            if (byGetComponent)
            {
                foreach (var node in zone.m_courseNodes)
                {
                    var lights = node.m_area.GetComponentsInChildren<LG_Light>();
                    if (lights == null)
                        yield break;

                    foreach (var light in lights)
                        yield return light;
                }
            }
            else
            {
                foreach (var light in zone.m_lightsInZone)
                {
                    yield return light;
                }
            }
        }
    }
}
