using System;
using System.Collections.Concurrent;

namespace GTFO.API.Assets.UnloadStrategies;

public abstract partial class AssetBundleUnloadStrategy
{
    private static readonly ConcurrentDictionary<string, Type> s_StrategyTypeLookup = [];

    public abstract string StrategyName { get; }
    public AssetBundleReference ManagingBundle { get; internal set; }

    public abstract void Apply();
    public abstract void Unapply();

    public static void RegisterType<T>() where T : AssetBundleUnloadStrategy, new()
    {
        var strategyInstance = new T();
        var strategyName = strategyInstance.StrategyName;

        if (s_StrategyTypeLookup.TryGetValue(strategyName, out var type))
        {
            return;
        }

        s_StrategyTypeLookup[strategyName] = typeof(T);
    }

    public static AssetBundleUnloadStrategy CreateInstance(string strategyName)
    {
        if (!s_StrategyTypeLookup.TryGetValue(strategyName, out var type))
        {
            return new Unspecified();
        }

        return (AssetBundleUnloadStrategy)Activator.CreateInstance(type);
    }
}
