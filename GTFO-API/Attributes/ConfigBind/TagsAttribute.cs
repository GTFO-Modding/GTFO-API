using System;
using BepInEx.Configuration;

namespace GTFO.API.Attributes.ConfigBind
{
    /// <summary>
    /// Specify Tags to applied for <see cref="ConfigEntry{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class TagsAttribute : Attribute
    {
        /// <summary>
        /// List of Tags that specified
        /// </summary>
        public string[] Tags { get; private set; }

        /// <summary>
        /// Create Attribute Instance
        /// </summary>
        /// <param name="tags">Tags to Specify</param>
        public TagsAttribute(params string[] tags)
        {
            Tags = tags;
        }
    }
}
