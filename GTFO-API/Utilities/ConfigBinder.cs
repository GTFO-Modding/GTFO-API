using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using GTFO.API.Attributes;

namespace GTFO.API.Utilities
{
    /// <summary>
    /// Utility Class for Automatically Binding <see cref="ConfigEntry{T}"/>
    /// </summary>
    public static class ConfigBinder
    {
        private static readonly MethodInfo s_BindToProperty = typeof(ConfigBinder).GetMethod(nameof(BindToProperty));

        /// <summary>
        /// Search Properties with <see cref="ConfigEntry{T}"/> inside given Type and Automatically Bind it
        /// </summary>
        /// <typeparam name="T">Type to Bind</typeparam>
        /// <param name="config">Config to Bind</param>
        /// <param name="searchNestedType">Search for Nested-Type inside given Type?</param>
        public static void BindToType<T>(ConfigFile config, bool searchNestedType = true)
        {
            BindToType(config, typeof(T), searchNestedType);
        }

        /// <summary>
        /// Search Properties with <see cref="ConfigEntry{T}"/> inside given Type and Automatically Bind it
        /// </summary>
        /// <param name="config">Config to Bind</param>
        /// <param name="type">Type to Bind</param>
        /// <param name="searchNestedType">Search for Nested-Type inside given Type?</param>
        public static void BindToType(ConfigFile config, Type type, bool searchNestedType = true)
        {
            const BindingFlags propFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            const BindingFlags typeFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

            var properties = type.GetProperties(propFlags).Where(IsConfigEntryProperty);
            var sectionAttr = type.GetCustomAttribute<SectionAttribute>();
            var section = sectionAttr?.Section ?? string.Empty;
            foreach (var prop in properties)
            {
                var entryType = prop.PropertyType.GetGenericArguments()[0];
                var args = new object[] { config, prop, section };
                s_BindToProperty.MakeGenericMethod([entryType]).Invoke(null, args);
                section = (string)args[2];
            }

            if (searchNestedType)
            {
                var nestTypes = type.GetNestedTypes(typeFlags).Where(IsConfigSectionType);
                foreach (var nestType in nestTypes)
                {
                    BindToType(config, nestType);
                }
            }
        }

        private static void BindToProperty<T>(ConfigFile config, PropertyInfo prop, ref string section)
        {
            var currentSection = section;
            var sectionAttr = prop.GetCustomAttribute<SectionAttribute>();
            if (sectionAttr != null && !string.IsNullOrWhiteSpace(sectionAttr.Section))
            {
                currentSection = sectionAttr.Section;
                if (!sectionAttr.Exclusive)
                    section = currentSection;
            }

            if (string.IsNullOrWhiteSpace(currentSection))
            {
                APILogger.Error(nameof(ConfigBinder), $"{prop.DeclaringType.FullName}::{prop.Name} doesn't have valid Section Attribute");
                return;
            }

            var configEntryAttribute = prop.GetCustomAttribute<ConfigEntryAttribute>();
            var key = GetKey(prop, configEntryAttribute);
            var description = GetDescription(prop, configEntryAttribute);
            var defaultValue = GetDefaultValue<T>(prop, configEntryAttribute);
            var tags = GetTags(prop);
            var acceptableValue = GetAcceptableValue(prop);

            var configDefinition = new ConfigDefinition(currentSection, key);
            var configDescription = new ConfigDescription(description, acceptableValue, tags);
            var entry = config.Bind(configDefinition, defaultValue, configDescription);

            prop.SetValue(null, entry);
        }

        #region Property Infos
        private static string GetKey(PropertyInfo prop, ConfigEntryAttribute configEntryAttribute)
        {
            var key = configEntryAttribute?.Key ?? null;

            var attr = prop.GetCustomAttribute<DisplayNameAttribute>();
            if (attr != null && !string.IsNullOrWhiteSpace(attr.DisplayName))
                key = attr.DisplayName;

            return string.IsNullOrWhiteSpace(key) ? prop.Name : key;
        }

        private static string GetDescription(PropertyInfo prop, ConfigEntryAttribute configEntryAttribute)
        {
            var desc = configEntryAttribute?.Description ?? null;

            var attr = prop.GetCustomAttribute<DescriptionAttribute>();
            if (attr != null)
                desc = attr.Description;

            return desc;
        }

        private static T GetDefaultValue<T>(PropertyInfo prop, ConfigEntryAttribute configEntryAttribute)
        {
            T value = default;
            if (configEntryAttribute != null && configEntryAttribute.DefaultValue is T v1)
                value = v1;

            var attr = prop.GetCustomAttribute<DefaultValueAttribute>();
            if (attr != null && attr.Value is T v2)
                value = v2;

            var attr2 = prop.GetCustomAttribute<DefaultRGBColorValueAttribute>();
            if (attr2 != null && attr2.HexValue is T v3)
                value = v3;

            return value;
        }

        private static string[] GetTags(PropertyInfo prop)
        {
            return prop.GetCustomAttribute<TagsAttribute>()?.Tags ?? null;
        }

        private static AcceptableValueBase GetAcceptableValue(PropertyInfo prop)
        {
            return prop.GetCustomAttribute<AcceptableValueAttribute>().Value ?? null;
        }
        #endregion Property Infos

        private static bool IsConfigSectionType(Type type)
        {
            if (type == null)
                return false;

            if (type.GetCustomAttribute<BindIgnoreAttribute>() != null)
                return false;

            return true;
        }

        private static bool IsConfigEntryProperty(PropertyInfo prop)
        {
            if (prop == null)
                return false;

            if (prop.GetCustomAttribute<BindIgnoreAttribute>() != null)
                return false;

            if (prop.SetMethod == null)
                return false;

            var propType = prop.PropertyType;
            if (propType == null)
                return false;

            if (!propType.IsGenericType || !propType.IsConstructedGenericType)
                return false;

            if (propType.GetGenericTypeDefinition() != typeof(ConfigEntry<>))
                return false;

            return true;
        }
    }
}
