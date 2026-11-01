using System;
using System.Diagnostics;

namespace GTFO.API;

internal readonly struct APIModuledLogger(string moduleName)
{
    [Obsolete("Do not use parameterless for clarity thank you", error: true)]
    public APIModuledLogger() : this("UNKNOWN")
    {

    }

    public readonly string ModuleName = moduleName;

    public void Info(object data) => APILogger.Info(ModuleName, data);
    public void Info(object data, Exception e) => APILogger.Info(ModuleName, data, e);

    public void Debug(object data) => APILogger.Debug(ModuleName, data);
    public void Debug(object data, Exception e) => APILogger.Debug(ModuleName, data, e);

    public void Warn(object data) => APILogger.Warn(ModuleName, data);
    public void Warn(object data, Exception e) => APILogger.Warn(ModuleName, data, e);

    public void Error(object data) => APILogger.Error(ModuleName, data);
    public void Error(object data, Exception e) => APILogger.Error(ModuleName, data, e);

    public void Fatal(object data) => APILogger.Fatal(ModuleName, data);
    public void Fatal(object data, Exception e) => APILogger.Fatal(ModuleName, data, e);


    [Conditional("DEBUG")]
    public void DebugBuildOnly(object data) => APILogger.DebugBuildOnly(ModuleName, data);
}
