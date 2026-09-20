using System;
using System.ComponentModel;

namespace GTFO.API.Attributes.ConfigBind
{
    /// <summary>
    /// Compounded Attribute for setting all Key, Description and DefaultValue<br/>
    /// - Description is same as using <see cref="DescriptionAttribute"/><br/>
    /// - DefaultValue is same as using <see cref="DefaultValueAttribute"/><br/>
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
        /// Has DefaultValue set?
        /// </summary>
        public bool HasDefaultValue { get; private set; } = false;

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
            HasDefaultValue = true;
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
            HasDefaultValue = true;
        }

        /// <summary>
        /// Create Attribute Instance
        /// </summary>
        /// <param name="key">Key Value</param>
        public ConfigEntryAttribute(string key)
        {
            Key = key;
            Description = null;
            DefaultValue = null;
            HasDefaultValue = false;
        }
    }
}
