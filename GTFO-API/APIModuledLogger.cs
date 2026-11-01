using System.Diagnostics;

namespace GTFO.API;

internal readonly struct APIModuledLogger(string moduleName)
{
    public readonly string ModuleName = moduleName;

    public void Info(object data) => APILogger.Info(ModuleName, data);
    public void Debug(object data) => APILogger.Debug(ModuleName, data);
    public void Warn(object data) => APILogger.Warn(ModuleName, data);
    public void Error(object data) => APILogger.Error(ModuleName, data);

    [Conditional("DEBUG")]
    public void DebugBuildOnly(object data) => APILogger.DebugBuildOnly(ModuleName, data);
}
