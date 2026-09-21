using System;
using BepInEx.Configuration;

namespace GTFO.API.Attributes.ConfigModel
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
        /// Create Attribute Instance, Same as using: <code>[AcceptableValueRange&lt;float&gt;(0.0f, 1.0f)]</code>
        /// </summary>
        public AcceptableValueRange01Attribute()
        {
            Value = new AcceptableValueRange<float>(0.0f, 1.0f);
        }
    }

    /// <summary>
    /// Specify <see cref="AcceptableValueRange{T}"/> with primitive types without generic
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AcceptableValueRangeAttribute : AcceptableValueAttribute
    {
        /// <summary>
        /// Create Attribute Instance
        /// </summary>
        /// <param name="min">Minimum Value</param>
        /// <param name="max">Maximum Value</param>
        public AcceptableValueRangeAttribute(byte min, byte max) => SetValue(min, max);

        /// <inheritdoc cref="AcceptableValueRangeAttribute(byte, byte)"/>
        public AcceptableValueRangeAttribute(sbyte min, sbyte max) => SetValue(min, max);

        /// <inheritdoc cref="AcceptableValueRangeAttribute(byte, byte)"/>
        public AcceptableValueRangeAttribute(short min, short max) => SetValue(min, max);

        /// <inheritdoc cref="AcceptableValueRangeAttribute(byte, byte)"/>
        public AcceptableValueRangeAttribute(ushort min, ushort max) => SetValue(min, max);

        /// <inheritdoc cref="AcceptableValueRangeAttribute(byte, byte)"/>
        public AcceptableValueRangeAttribute(int min, int max) => SetValue(min, max);

        /// <inheritdoc cref="AcceptableValueRangeAttribute(byte, byte)"/>
        public AcceptableValueRangeAttribute(uint min, uint max) => SetValue(min, max);

        /// <inheritdoc cref="AcceptableValueRangeAttribute(byte, byte)"/>
        public AcceptableValueRangeAttribute(long min, long max) => SetValue(min, max);

        /// <inheritdoc cref="AcceptableValueRangeAttribute(byte, byte)"/>
        public AcceptableValueRangeAttribute(ulong min, ulong max) => SetValue(min, max);

        /// <inheritdoc cref="AcceptableValueRangeAttribute(byte, byte)"/>
        public AcceptableValueRangeAttribute(float min, float max) => SetValue(min, max);

        /// <inheritdoc cref="AcceptableValueRangeAttribute(byte, byte)"/>
        public AcceptableValueRangeAttribute(double min, double max) => SetValue(min, max);

        /// <inheritdoc cref="AcceptableValueRangeAttribute(byte, byte)"/>
        public AcceptableValueRangeAttribute(decimal min, decimal max) => SetValue(min, max);

        /// <inheritdoc cref="AcceptableValueRangeAttribute(byte, byte)"/>
        public AcceptableValueRangeAttribute(string min, string max) => SetValue(min, max);

        /// <inheritdoc cref="AcceptableValueRangeAttribute(byte, byte)"/>
        public AcceptableValueRangeAttribute(Enum min, Enum max) => SetValue(min, max);

        private void SetValue<T>(T min, T max) where T : IComparable
        {
            Value = new AcceptableValueRange<T>(min, max);
        }
    }

    /// <summary>
    /// Specify <see cref="AcceptableValueList{T}"/> with primitive types without generic
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AcceptableValueListAttribute : AcceptableValueAttribute
    {
        /// <summary>
        /// Create Attribute Instance
        /// </summary>
        /// <param name="values">Acceptable Values</param>
        public AcceptableValueListAttribute(params byte[] values) => SetValue(values);

        /// <inheritdoc cref="AcceptableValueListAttribute(byte[])"/>
        public AcceptableValueListAttribute(params sbyte[] values) => SetValue(values);

        /// <inheritdoc cref="AcceptableValueListAttribute(byte[])"/>
        public AcceptableValueListAttribute(params short[] values) => SetValue(values);

        /// <inheritdoc cref="AcceptableValueListAttribute(byte[])"/>
        public AcceptableValueListAttribute(params ushort[] values) => SetValue(values);

        /// <inheritdoc cref="AcceptableValueListAttribute(byte[])"/>
        public AcceptableValueListAttribute(params int[] values) => SetValue(values);

        /// <inheritdoc cref="AcceptableValueListAttribute(byte[])"/>
        public AcceptableValueListAttribute(params uint[] values) => SetValue(values);

        /// <inheritdoc cref="AcceptableValueListAttribute(byte[])"/>
        public AcceptableValueListAttribute(params long[] values) => SetValue(values);

        /// <inheritdoc cref="AcceptableValueListAttribute(byte[])"/>
        public AcceptableValueListAttribute(params ulong[] values) => SetValue(values);

        /// <inheritdoc cref="AcceptableValueListAttribute(byte[])"/>
        public AcceptableValueListAttribute(params float[] values) => SetValue(values);

        /// <inheritdoc cref="AcceptableValueListAttribute(byte[])"/>
        public AcceptableValueListAttribute(params double[] values) => SetValue(values);

        /// <inheritdoc cref="AcceptableValueListAttribute(byte[])"/>
        public AcceptableValueListAttribute(params decimal[] values) => SetValue(values);

        /// <inheritdoc cref="AcceptableValueListAttribute(byte[])"/>
        public AcceptableValueListAttribute(params string[] values) => SetValue(values);

        private void SetValue<T>(params T[] values) where T : IEquatable<T>
        {
            Value = new AcceptableValueList<T>(values);
        }
    }
}
