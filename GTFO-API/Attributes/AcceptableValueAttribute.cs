using System;
using BepInEx.Configuration;

namespace GTFO.API.Attributes
{
    /// <summary>
    /// Base Type for <see cref="AcceptableValueRangeAttribute{T}"/> and <see cref="AcceptableValueListAttribute{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class AcceptableValueAttribute : Attribute
    {
        /// <summary>
        /// <see cref="AcceptableValueBase"/> Instance that should be used for ConfigEntry
        /// </summary>
        public AcceptableValueBase Value { get; protected set; }
    }

    /// <summary>
    /// Specify <see cref="AcceptableValueRange{T}"/> for ConfigEntry
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AcceptableValueRangeAttribute<T> : AcceptableValueAttribute where T : IComparable
    {
        /// <summary>
        /// Create Attribute Instance
        /// </summary>
        /// <param name="min">Allowed Minimum Value</param>
        /// <param name="max">Allowed Maximum Value</param>
        public AcceptableValueRangeAttribute(T min, T max)
        {
            Value = new AcceptableValueRange<T>(min, max);
        }
    }

    /// <summary>
    /// Specify <see cref="AcceptableValueList{T}"/> for ConfigEntry
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AcceptableValueListAttribute<T> : AcceptableValueAttribute where T : IEquatable<T>
    {
        /// <summary>
        /// Create Attribute Instance
        /// </summary>
        /// <param name="values">List of values that allowed</param>
        public AcceptableValueListAttribute(params T[] values)
        {
            Value = new AcceptableValueList<T>(values);
        }
    }

    /// <summary>
    /// Specify <see cref="AcceptableValueRange{T}"/> with [0.0~1.0] range for ConfigEntry
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AcceptableValueRange01Attribute : AcceptableValueAttribute
    {
        /// <summary>
        /// Create Attribute Instance
        /// </summary>
        /// <param name="min">Allowed Minimum Value</param>
        /// <param name="max">Allowed Maximum Value</param>
        public AcceptableValueRange01Attribute()
        {
            Value = new AcceptableValueRange<float>(0.0f, 1.0f);
        }
    }
}
