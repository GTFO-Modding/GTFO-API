using System;

namespace GTFO.API.EventArg;

/// <summary>
/// Delegate for Request/Load Callback of <see cref="UnityEngine.Object"/> kind objects
/// </summary>
/// <param name="args">EventArgs</param>
public delegate void UnityObjectRequestCallback(UnityObjectRequestEventArgs args);

/// <summary>
/// EventArgs for Request/Load Callback of <see cref="UnityEngine.Object"/> kind objects
/// </summary>
public sealed class UnityObjectRequestEventArgs : EventArgs
{
    /// <summary>
    /// Loaded Object
    /// </summary>
    public UnityEngine.Object LoadedObject { get; init; }

    /// <summary>
    /// Flag to check if request was successfully done.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Elapsed Time to load.
    /// </summary>
    public TimeSpan Elapsed { get; init; }
}
