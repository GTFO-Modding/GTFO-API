using System;
using System.Runtime.CompilerServices;

#if DEBUG
using System.Diagnostics;
#endif

namespace GTFO.API.Utilities;

internal struct TimeScope : IDisposable
{
#if DEBUG
    private long _StartTick;
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
        _StartTick = Stopwatch.GetTimestamp();
#endif
    }

    public TimeScope(string debugName)
    {
#if DEBUG
        _DebugName = debugName.ToString();
        _StartTick = Stopwatch.GetTimestamp();
#endif
    }

    public void End() => Dispose();

    public void Dispose()
    {
#if DEBUG
        var endTick = Stopwatch.GetTimestamp();
        var elapsed = endTick - _StartTick;
        var timeSpan = new TimeSpan((long)((elapsed) * StopwatchNoalloc.TickConversion));
        APILogger.Debug("TimeScope", $"'{_DebugName}' Elapsed: {timeSpan.Milliseconds}ms ({timeSpan.Ticks} ticks)");
#endif
    }
}
