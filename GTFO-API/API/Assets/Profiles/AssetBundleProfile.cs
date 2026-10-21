using System;
using System.Collections.Concurrent;

namespace GTFO.API.Assets.Profiles;

/// <summary>
/// Type to define how to manage <see cref="AssetBundleReference"/>
/// </summary>
public abstract partial class AssetBundleProfile
{
    private static readonly ConcurrentDictionary<string, Type> s_ProfileTypeLookup = [];

    /// <summary>
    /// Profile Name, AssetBundle Metadata can use this name to apply specified Profile
    /// </summary>
    public abstract string ProfileName { get; }

    /// <summary>
    /// Managing <see cref="AssetBundleReference"/>
    /// </summary>
    public AssetBundleReference ManagingBundle { get; internal set; }

    /// <summary>
    /// Invoked when this Profile is being used.
    /// </summary>
    public abstract void Apply();

    /// <summary>
    /// Invoked when this Profile is stop being used.
    /// </summary>
    public abstract void Unapply();

    /// <summary>
    /// Register <see cref="AssetBundleProfile"/> to Lookup
    /// </summary>
    /// <typeparam name="T">Profile Type</typeparam>
    public static void RegisterType<T>() where T : AssetBundleProfile, new()
    {
        var strategyInstance = new T();
        var strategyName = strategyInstance.ProfileName;

        if (s_ProfileTypeLookup.TryGetValue(strategyName, out var type))
        {
            return;
        }

        s_ProfileTypeLookup[strategyName] = typeof(T);
    }

    /// <summary>
    /// Create new <see cref="AssetBundleProfile"/> Instance from given Profile Name
    /// </summary>
    /// <param name="profileName">Name of the Profile (<see cref="ProfileName"/>)</param>
    /// <returns></returns>
    public static AssetBundleProfile CreateInstance(string profileName)
    {
        if (!s_ProfileTypeLookup.TryGetValue(profileName, out var type))
        {
            return new Unspecified();
        }

        return (AssetBundleProfile)Activator.CreateInstance(type);
    }
}
