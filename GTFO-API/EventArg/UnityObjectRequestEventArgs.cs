using System;

namespace GTFO.API.EventArg;

public delegate void UnityObjectRequestEvent(UnityObjectRequestEventArgs args);

public sealed class UnityObjectRequestEventArgs : EventArgs
{
    public UnityEngine.Object LoadedObject { get; init; }
    public bool Success { get; init; }
}
