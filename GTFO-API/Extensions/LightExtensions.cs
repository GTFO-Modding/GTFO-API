using GTFO.API.Components;
using LevelGeneration;

namespace GTFO.API.Extensions;

/// <summary>
/// Container for all extension for <see cref="LG_Light"/>
/// </summary>
public static class LightExtensions
{
    /// <summary>
    /// Get <see cref="LG_LightInfo"/> Instance for given <see cref="LG_Light"/>
    /// </summary>
    /// <param name="light">Instance to check</param>
    /// <returns>Instance of <see cref="LG_LightInfo"/> attached to Light</returns>
    public static LG_LightInfo GetLightInfo(this LG_Light light)
    {
        return light.GetComponent<LG_LightInfo>();
    }
}
