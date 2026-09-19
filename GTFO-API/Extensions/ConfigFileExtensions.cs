using System;
using BepInEx.Configuration;
using GTFO.API.Utilities;

namespace GTFO.API.Extensions
{
    public static class ConfigFileExtensions
    {
        /// <inheritdoc cref="ConfigBinder.BindToType(ConfigFile, Type, bool)"/>
        public static void BindToType(this ConfigFile config, Type type, bool searchNestedType = true)
        {
            ConfigBinder.BindToType(config, type, searchNestedType);
        }

        /// <inheritdoc cref="ConfigBinder.BindToType{T}(ConfigFile, bool)"/>
        public static void BindToType<T>(this ConfigFile config, bool searchNestedType = true)
        {
            ConfigBinder.BindToType(config, typeof(T), searchNestedType);
        }
    }
}
