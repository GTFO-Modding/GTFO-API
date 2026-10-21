using System;
using System.Collections.Concurrent;

namespace GTFO.API.Assets.Profiles;

public abstract partial class AssetBundleProfile
{
    private static readonly ConcurrentDictionary<string, Type> s_ProfileTypeLookup = [];

    public abstract string ProfileName { get; }
    public AssetBundleReference ManagingBundle { get; internal set; }

    public abstract void Apply();
    public abstract void Unapply();

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

    public static AssetBundleProfile CreateInstance(string strategyName)
    {
        if (!s_ProfileTypeLookup.TryGetValue(strategyName, out var type))
        {
            return new Unspecified();
        }

        return (AssetBundleProfile)Activator.CreateInstance(type);
    }
}
