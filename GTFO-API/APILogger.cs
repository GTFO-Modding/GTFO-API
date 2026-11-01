using System;
using System.Diagnostics;
using BepInEx.Logging;

namespace GTFO.API
{
    internal static class APILogger
    {
        private static readonly ManualLogSource _logger;

        static APILogger()
        {
            _logger = new ManualLogSource("GTFO-API");
            Logger.Sources.Add(_logger);
        }
        private static string Format(string module, object msg) => $"[{module}]: {msg}";
        private static string FormatException(string module, object msg, Exception e)
        {
            return $"[{module}]: {msg}{Environment.NewLine} - {e}";
        }

        public static void Info(string module, object data) => _logger.LogMessage(Format(module, data));
        public static void Info(string module, object data, Exception e) => _logger.LogMessage(FormatException(module, data, e));

        public static void Debug(string module, object data) => _logger.LogDebug(Format(module, data));
        public static void Debug(string module, object data, Exception e) => _logger.LogDebug(FormatException(module, data, e));

        public static void Warn(string module, object data) => _logger.LogWarning(Format(module, data));
        public static void Warn(string module, object data, Exception e) => _logger.LogWarning(FormatException(module, data, e));

        public static void Error(string module, object data) => _logger.LogError(Format(module, data));
        public static void Error(string module, object data, Exception e) => _logger.LogError(FormatException(module, data, e));

        public static void Fatal(string module, object data) => _logger.LogFatal(Format(module, data));
        public static void Fatal(string module, object data, Exception e) => _logger.LogFatal(FormatException(module, data, e));

        [Conditional("DEBUG")]
        public static void DebugBuildOnly(string module, object data)
        {
#if DEBUG
            _logger.LogDebug(Format(module, data));
#endif
        }
    }
}
