using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using GTFO.API.Attributes;
using GTFO.API.Resources;

namespace GTFO.API
{
    /// <summary>
    /// Helper API for Interop-ing with other plugins
    /// </summary>
    [API("Interop")]
    public static class InteropAPI
    {
        /// <summary>
        /// Status info for the <see cref="InteropAPI"/>
        /// </summary>
        public static ApiStatusInfo Status => APIStatus.Interop;

        /// <summary>
        /// Check if given plugin is loaded
        /// </summary>
        /// <param name="pluginGUID">Plugin GUID to check</param>
        /// <param name="pluginInfo">PluginInfo for that given plugin if exists</param>
        /// <returns>true if Plugin is exists, otherwise false</returns>
        public static bool PluginExists(string pluginGUID, out PluginInfo pluginInfo)
        {
            return IL2CPPChainloader.Instance.Plugins.TryGetValue(pluginGUID, out pluginInfo);
        }

        /// <summary>
        /// Execute callback if plugin with given GUID is loaded
        /// </summary>
        /// <param name="pluginGUID">GUID for Plugin to check</param>
        /// <param name="action">Callback that should be called if Plugin is exists</param>
        public static void ExecuteWhenPluginExists(string pluginGUID, Action<PluginInfo> action)
        {
            if (action == null)
                return;

            if (PluginExists(pluginGUID, out var pluginInfo))
            {
                action.Invoke(pluginInfo);
            }
        }

        /// <summary>
        /// Execute callback if plugin with given GUID is NOT loaded
        /// </summary>
        /// <param name="pluginGUID">GUID for Plugin to check</param>
        /// <param name="action">Callback that should be called if Plugin is NOT exists</param>
        public static void ExecuteWhenPluginNotExists(string pluginGUID, Action action)
        {
            if (action == null)
                return;

            if (!PluginExists(pluginGUID, out _))
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// Register a Call for other developers to use your API without direct reference
        /// </summary>
        /// <param name="callName">Name of the call. should be unique across all other plugins</param>
        /// <param name="callback">Callback when Call has invoked by <see cref="InteropAPI.Call"/></param>
        public static void RegisterCall(string callName, Func<object[], object> callback)
        {
            if (callback == null)
            {
                APILogger.Warn($"Interop", $"'{callName}' was requested to registed without valid {nameof(callback)} parameter given! This will be ignored!");
                return;
            }

            if (s_CallLookup.TryGetValue(callName, out var existingCall))
            {
                APILogger.Error($"Interop", $"{callName} is already occupied by other mods!");
                return;
            }

            s_CallLookup[callName] = callback;
            APILogger.Verbose($"Interop", $"{callName} has successfully registered.");
        }

        /// <summary>
        /// Invoke a registed Call that provided by other Plugins
        /// </summary>
        /// <param name="callName">Name of the Call</param>
        /// <param name="parameters">Parameters to pass to Call</param>
        /// <returns>Returned object from other Plugin</returns>
        public static object Call(string callName, params object[] parameters)
        {
            if (s_CallLookup.TryGetValue(callName, out var action))
            {
                return action?.Invoke(parameters) ?? null;
            }

            return null;
        }

        private static readonly Dictionary<string, Func<object[], object>> s_CallLookup = [];
    }
}
