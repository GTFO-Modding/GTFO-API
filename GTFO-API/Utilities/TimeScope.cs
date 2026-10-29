using System;
using System.Diagnostics;

namespace GTFO.API.Utilities;

internal struct TimeScope : IDisposable
{
#if DEBUG
    private Stopwatch _SW;
    private string _DebugName;
#endif

    public static TimeScope Create(string debugName)
    {
        return new TimeScope(debugName);
    }

    public TimeScope(string debugName)
    {
#if DEBUG
        _DebugName = debugName;
        _SW = Stopwatch.StartNew();
#endif
    }

    public void Dispose()
    {
#if DEBUG
        _SW.Stop();
        APILogger.Debug("TimeScope", $"'{_DebugName}' Elapsed: {_SW.ElapsedMilliseconds}ms ({_SW.ElapsedTicks} ticks)");
#endif
    }
}
