using System;
using System.Diagnostics;

namespace GTFO.API.Utilities;

/// <summary>
/// Simple <see cref="Stopwatch"/> without allocation
/// </summary>
public ref struct StopwatchNoalloc
{
    /// <summary>
    /// Multiplier for Converting Stopwatch Tick into TimeSpan Tick
    /// </summary>
    public static readonly double TickConversion = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;

    /// <inheritdoc cref="Stopwatch.Elapsed"/>
    public readonly TimeSpan Elapsed => GetElapsed(_StartTick, EndTickOrCurrentTick);

    /// <inheritdoc cref="Stopwatch.ElapsedTicks"/>
    public readonly long ElapsedTicks => EndTickOrCurrentTick - _StartTick;

    /// <inheritdoc cref="Stopwatch.ElapsedMilliseconds"/>
    public readonly long ElapsedMilliseconds => Elapsed.Milliseconds;

    private readonly long EndTickOrCurrentTick => _EndTick > -1 ? _EndTick : Stopwatch.GetTimestamp();

    private long _StartTick;
    private long _EndTick;

    /// <summary>
    /// Get Elapsed TimeSpan from Start to End
    /// </summary>
    /// <param name="startTick">Starting Timestamp</param>
    /// <param name="endTick">End Timestamp</param>
    /// <returns>Elapsed Time in <see cref="TimeSpan"/></returns>
    public static TimeSpan GetElapsed(long startTick, long endTick)
    {
        return new((long)((endTick - startTick) * TickConversion));
    }

    /// <summary>
    /// Get Elapsed TimeSpan from Start to Current
    /// </summary>
    /// <param name="startTick">Starting Timestamp</param>
    /// <returns>Elapsed Time in <see cref="TimeSpan"/></returns>
    public static TimeSpan GetElapsedFrom(long startTick)
    {
        return new((long)((Stopwatch.GetTimestamp() - startTick) * TickConversion));
    }

    /// <inheritdoc cref="Stopwatch.GetTimestamp"/>
    public static long GetTimestamp()
    {
        return Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// Create New Instance
    /// </summary>
    public StopwatchNoalloc()
    {
        _StartTick = Stopwatch.GetTimestamp();
        _EndTick = -1;
    }

    /// <inheritdoc cref="Stopwatch.Restart"/>
    public void Restart()
    {
        _StartTick = Stopwatch.GetTimestamp();
        _EndTick = -1;
    }

    /// <inheritdoc cref="Stopwatch.Stop"/>
    public void Stop()
    {
        _EndTick = Stopwatch.GetTimestamp();
    }
}
