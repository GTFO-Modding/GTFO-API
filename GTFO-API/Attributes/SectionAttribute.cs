using System;

namespace GTFO.API.Attributes
{
    /// <summary>
    /// Config Section Definition Attribute: <see cref="Section"/> can be skipped when it's for nested-type and same as type name
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SectionAttribute : Attribute
    {
        public string Section { get; private set; }

        /// <summary>
        /// Only for Properties; Is this attribute is exclusive for this specific property?
        /// </summary>
        public bool Exclusive { get; set; } = false;

        /// <summary>
        /// Creates Section Definition
        /// </summary>
        /// <param name="section">Name of the Section, Can be skipped if it's for nested-type and Section name is same as type name</param>
        public SectionAttribute(string section = null)
        {
            Section = section;
        }
    }
}
