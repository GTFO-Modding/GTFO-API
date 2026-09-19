using System;
using GTFO.API.Utilities;

namespace GTFO.API.Attributes
{
    /// <summary>
    /// Property or Class specified with this Attribute will not be processed by <see cref="ConfigBinder"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class BindIgnoreAttribute : Attribute;
}
