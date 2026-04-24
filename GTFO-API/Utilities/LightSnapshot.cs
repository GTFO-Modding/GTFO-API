using Il2CppInterop.Runtime;
using LevelGeneration;
using UnityEngine;

namespace GTFO.API.Utilities;

/// <summary>
/// Captured Snapshot Data for LG_Light<br/>
/// If needed: Cast It's Instance to Following Types for Specific Light Data
/// <list>
/// - <see cref="PointLightSnapshot"/><br/>
/// - <see cref="SpotLightSnapshot"/><br/>
/// - <see cref="SpotLightAmbientSnapshot"/><br/>
/// </list>
/// </summary>
public abstract record LightSnapshot
{
    /// <summary>
    /// Color Data of the Light
    /// </summary>
    public Color Color { get; init; }

    /// <summary>
    /// Intensity Data of the Light
    /// </summary>
    public float Intensity { get; init; }

    /// <summary>
    /// Range Data of the Light
    /// </summary>
    public float Range { get; init; }

    /// <summary>
    /// Apply Current Snapshot to the LG_Light If Possible
    /// </summary>
    /// <param name="light"></param>
    /// <returns><see langword="true"/> If Snapshot Data has Applied to Light</returns>
    public abstract bool Apply(LG_Light light);

    /// <summary>
    /// Capture a Snapshot from a <see cref="LG_Light"/> Instance
    /// </summary>
    /// <param name="light">Instance to Capture</param>
    /// <returns>Captured Snapshot Instance, <see langword="null"/> If LG_Light is Invalid (null or non-vanilla type)</returns>
    public static LightSnapshot Capture(LG_Light light)
    {
        if (light == null)
            return null;

        var type = light.GetIl2CppType();
        if (type == Il2CppType.Of<LG_PointLight>())
        {
            return PointLightSnapshot.Capture(light.Cast<LG_PointLight>());
        }
        else if (type == Il2CppType.Of<LG_SpotLight>())
        {
            return SpotLightSnapshot.Capture(light.Cast<LG_SpotLight>());
        }
        else if (type == Il2CppType.Of<LG_SpotLightAmbient>())
        {
            return SpotLightAmbientSnapshot.Capture(light.Cast<LG_SpotLightAmbient>());
        }

        return null;
    }
}

/// <inheritdoc/>
public record PointLightSnapshot : LightSnapshot
{
    /// <inheritdoc/>
    public override bool Apply(LG_Light light)
    {
        var pointLight = light.Cast<LG_PointLight>();
        if (pointLight == null)
            return false;

        var ulight = pointLight.m_pointLight;
        if (ulight == null)
            return false;

        light.ChangeColor(Color);
        light.ChangeIntensity(Intensity);
        ulight.range = Range;
        return true;
    }

    /// <summary>
    /// Capture a Snapshot from a <see cref="LG_PointLight"/> Instance
    /// </summary>
    /// <param name="pointLight">Instance to Capture</param>
    /// <returns>Captured Snapshot Instance</returns>
    public static PointLightSnapshot Capture(LG_PointLight pointLight)
    {
        return new PointLightSnapshot()
        {
            Color = pointLight.m_color,
            Intensity = pointLight.m_intensity,
            Range = pointLight.m_pointLight.range
        };
    }
}

/// <inheritdoc/>
public record SpotLightSnapshot : LightSnapshot
{
    /// <summary>
    /// Spot Angle Data of the Light
    /// </summary>
    public float Angle { get; init; }

    /// <inheritdoc/>
    public override bool Apply(LG_Light light)
    {
        var spotLight = light.Cast<LG_SpotLight>();
        if (spotLight == null)
            return false;

        var ulight = spotLight.m_spotLight;
        if (ulight == null)
            return false;

        light.ChangeColor(Color);
        light.ChangeIntensity(Intensity);
        ulight.spotAngle = Angle;
        ulight.range = Range;
        return true;
    }

    /// <summary>
    /// Capture a Snapshot from a <see cref="LG_SpotLight"/> Instance
    /// </summary>
    /// <param name="spotLight">Instance to Capture</param>
    /// <returns>Captured Snapshot Instance</returns>
    public static SpotLightSnapshot Capture(LG_SpotLight spotLight)
    {
        return new SpotLightSnapshot()
        {
            Color = spotLight.m_color,
            Intensity = spotLight.m_intensity,
            Range = spotLight.m_spotLight.range,
            Angle = spotLight.m_spotLight.spotAngle
        };
    }
}

/// <inheritdoc/>
public record SpotLightAmbientSnapshot : SpotLightSnapshot
{
    /// <summary>
    /// Ambient Scale of the <see cref="LG_SpotLightAmbient"/>
    /// </summary>
    public float AmbientScale { get; init; }

    /// <inheritdoc/>
    public override bool Apply(LG_Light light)
    {
        var spotAmbientLight = light.Cast<LG_SpotLightAmbient>();
        if (spotAmbientLight == null)
            return false;

        var ulight = spotAmbientLight.m_spotLight;
        if (ulight == null)
            return false;

        light.ChangeColor(Color);
        light.ChangeIntensity(Intensity);
        spotAmbientLight.m_ambientScale = AmbientScale;
        ulight.range = Range;
        return true;
    }

    /// <summary>
    /// Capture a Snapshot from a <see cref="LG_SpotLightAmbient"/> Instance
    /// </summary>
    /// <param name="spotLightAmbient">Instance to Capture</param>
    /// <returns>Captured Snapshot Instance</returns>
    public static SpotLightAmbientSnapshot Capture(LG_SpotLightAmbient spotLightAmbient)
    {
        return new SpotLightAmbientSnapshot()
        {
            Color = spotLightAmbient.m_color,
            Intensity = spotLightAmbient.m_intensity,
            Range = spotLightAmbient.m_spotLight.range,
            Angle = spotLightAmbient.m_spotLight.spotAngle,
            AmbientScale = spotLightAmbient.m_ambientScale
        };
    }
}
