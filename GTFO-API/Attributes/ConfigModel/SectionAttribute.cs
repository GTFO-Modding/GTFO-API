using System;

namespace GTFO.API.Attributes.ConfigModel
{
    /// <summary>
    /// Config Section Definition Attribute: <see cref="Section"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SectionAttribute : Attribute
    {
        /// <summary>
        /// Section Name
        /// </summary>
        public string Section { get; private set; }

        /// <summary>
        /// Only for Properties; Is this attribute exclusive for this specific property?
        /// </summary>
        public bool Exclusive { get; set; } = false;

        /// <summary>
        /// Creates Section Definition
        /// </summary>
        /// <param name="section">Name of the Section</param>
        public SectionAttribute(string section = null)
        {
            Section = section;
        }
    }
}
