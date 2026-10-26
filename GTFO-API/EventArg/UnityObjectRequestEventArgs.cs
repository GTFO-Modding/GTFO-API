using System;

namespace GTFO.API.EventArg;

/// <summary>
/// Delegate for Load Callback of <see cref="UnityEngine.Object"/> kind objects
/// </summary>
/// <param name="args">EventArgs</param>
public delegate void UnityObjectRequestEvent(UnityObjectRequestEventArgs args);

/// <summary>
/// EventArgs for Load Callback of <see cref="UnityEngine.Object"/> kind objects
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
}
