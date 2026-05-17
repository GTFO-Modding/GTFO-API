using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace GTFO.API;

public abstract class LoadingJob
{
    public abstract string JobName { get; }
    public abstract string DisplayName { get; }

    public string DisplayText { get; private set; }
    public bool IsCompleted { get; private set; } = false;

    public virtual bool ExitApplicationOnException => false;

    internal IEnumerator DoJob()
    {
        var enumerator = Job();
        while (true)
        {
            object ret = null;
            try
            {
                if (!enumerator.MoveNext())
                {
                    break;
                }
                ret = enumerator.Current;
            }
            catch (Exception e)
            {
                var moduleName = $"{nameof(LoadingAPI)}.Job.{GetType().FullName}";
                if (ExitApplicationOnException)
                {
                    APILogger.Error(moduleName, $"Exception Thrown; This is critical and will kill the application!:{Environment.NewLine}{e}");
                    Application.Quit();
                }
                else
                {
                    APILogger.Warn(moduleName, $"Exception Thrown; {Environment.NewLine}{e}");
                }
            }

            yield return ret;
        }

        IsCompleted = true;

        var readTask = File.ReadAllTextAsync("");
        yield return WaitForTask(readTask);
        var text = readTask.Result;
    }

    internal void DoUpdateTexts()
    {
        string name = DisplayName;
        string status = "";
        string rawOverride = null;
        UpdateText(ref name, ref status, ref rawOverride);

        if (rawOverride == null)
        {
            DisplayText = $"{name.PadRight(16, '.')}{status}";
        }
        else
        {
            DisplayText = rawOverride;
        }
    }

    protected abstract IEnumerator Job();

    protected virtual void UpdateText(ref string name, ref string status, ref string overrideText)
    {
        name = DisplayName;
        status = IsCompleted ? "Loading..." : "Done";
    }

    public static WaitUntil WaitForTask(Task task)
    {
        return new WaitUntil((Il2CppSystem.Func<bool>)(() => task.IsCompleted));
    }

    public static WaitUntil WaitForOtherJob(LoadingJob job)
    {
        return new WaitUntil((Il2CppSystem.Func<bool>)(() => job.IsCompleted));
    }

    public static WaitUntil WaitForOtherJob(string jobName, bool exceptionOnMissing = false)
    {
        if (LoadingAPI.Jobs.TryGetValue(jobName, out var job))
        {
            return WaitForOtherJob(job);
        }
        else if (!exceptionOnMissing)
        {
            return new WaitUntil((Il2CppSystem.Func<bool>)(() => true));
        }
        else
        {
            throw new KeyNotFoundException($"Can't find a dependency job with a name: '{jobName}'");
        }
    }
}
