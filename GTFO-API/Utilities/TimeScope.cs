using System;
using System.Runtime.CompilerServices;

#if DEBUG
using System.Diagnostics;
#endif

namespace GTFO.API.Utilities;

internal struct TimeScope : IDisposable
{
#if DEBUG
    private Stopwatch _SW;
    private string _DebugName;
#endif

    public static TimeScope Create(DefaultInterpolatedStringHandler debugName)
    {
#if DEBUG
        return new TimeScope(debugName);
#else
        return default;
#endif
    }

    public static TimeScope Create(string debugName)
    {
#if DEBUG
        return new TimeScope(debugName);
#else
        return default;
#endif
    }

    public TimeScope(DefaultInterpolatedStringHandler debugName)
    {
#if DEBUG
        _DebugName = debugName.ToString();
        _SW = Stopwatch.StartNew();
#endif
    }

    public TimeScope(string debugName)
    {
#if DEBUG
        _DebugName = debugName.ToString();
        _SW = Stopwatch.StartNew();
#endif
    }

    public void End() => Dispose();

    public void Dispose()
    {
#if DEBUG
        _SW.Stop();
        APILogger.Debug("TimeScope", $"'{_DebugName}' Elapsed: {_SW.ElapsedMilliseconds}ms ({_SW.ElapsedTicks} ticks)");
#endif
    }
}
