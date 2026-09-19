using System;
using System.ComponentModel;

namespace GTFO.API.Attributes
{
    /// <summary>
    /// Compounded Attribute for <see cref="DisplayNameAttribute"/>, <see cref="DescriptionAttribute"/> and <see cref="DefaultValueAttribute"/><br/>
    /// - Key is <see cref="DisplayNameAttribute"/><br/>
    /// - Description is <see cref="DescriptionAttribute"/><br/>
    /// - DefaultValue is <see cref="DefaultValueAttribute"/><br/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ConfigEntryAttribute : Attribute
    {
        /// <summary>
        /// ConfigEntry Key Value
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// ConfigEntry Description Value
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// ConfigEntry Default Value
        /// </summary>
        public object DefaultValue { get; private set; }

        /// <summary>
        /// Create Attribute Instance
        /// </summary>
        /// <param name="key">Key Value</param>
        /// <param name="defaultValue">Default Value</param>
        /// <param name="description">Description Value</param>
        public ConfigEntryAttribute(string key, object defaultValue, string description)
        {
            Key = key;
            Description = description;
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// Create Attribute Instance
        /// </summary>
        /// <param name="key">Key Value</param>
        /// <param name="defaultValue">Default Value</param>
        public ConfigEntryAttribute(string key, object defaultValue)
        {
            Key = key;
            Description = null;
            DefaultValue = defaultValue;
        }
    }
}
