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
    /// Profile Name, AssetBundle Metadata can use this name to apply specified Profile<br/>
    /// - Default: GetType().Name
    /// </summary>
    public virtual string ProfileName => GetType().Name;

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
        var profileInstance = new T();
        var profileName = profileInstance.ProfileName;

        if (s_ProfileTypeLookup.TryGetValue(profileName, out var type))
        {
            APILogger.Error("AssetBundle", $"'{profileName}' is already registered by '{type.AssemblyQualifiedName}'");
            return;
        }

        s_ProfileTypeLookup[profileName] = typeof(T);
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
            APILogger.Error("AssetBundle", $"'{profileName}' is invalid Profile name! Fall-backs to '{nameof(Unspecified)}'");
            return new Unspecified();
        }

        return (AssetBundleProfile)Activator.CreateInstance(type);
    }
}
