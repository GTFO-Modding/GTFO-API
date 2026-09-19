using System;
using BepInEx.Configuration;

namespace GTFO.API.Attributes
{
    /// <summary>
    /// Specify Tags to applied for <see cref="ConfigEntry{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class TagsAttribute : Attribute
    {
        public string[] Tags { get; private set; }

        public TagsAttribute(params string[] tags)
        {
            Tags = tags;
        }
    }
}
