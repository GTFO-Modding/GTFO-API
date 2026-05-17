using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GTFO.API.Utilities;
using UnityEngine;

namespace GTFO.API;

public abstract class LoadingJob
{
    public abstract string JobName { get; }
    public abstract string DisplayName { get; }

    public string DisplayText { get; private set; }
    public bool IsRunning { get; private set; } = false;
    public bool IsCompleted { get; private set; } = false;

    public event Action OnCompleted;

    public virtual bool ExitApplicationOnException => false;

    internal IEnumerator DoJob()
    {
        IsRunning = true;

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

        IsRunning = false;
        IsCompleted = true;
        SafeInvoke.Invoke(OnCompleted);
    }

    internal void DoUpdateTexts()
    {
        _Builder.Clear();
        _AppendBuilder.Clear();

        string name = DisplayName;
        string status = IsCompleted ? "Done" : "Loading";
        string rawOverride = null;

        UpdateText(ref name, ref status, ref rawOverride, _AppendBuilder);
        string append = _AppendBuilder.ToString();

        if (rawOverride == null)
        {
            _Builder.Append(name.PadRight(48, '.'));
            _Builder.Append(status);
        }
        else
        {
            _Builder.Append(rawOverride);
        }

        if (!string.IsNullOrWhiteSpace(append))
        {
            _Builder.AppendLine();
            _Builder.Append(append);
        }

        DisplayText = _Builder.ToString();
    }

    protected abstract IEnumerator Job();

    /// <summary>
    /// Update a Loading text with given parameters
    /// </summary>
    /// <param name="name">Default: <see cref="DisplayName"/>; PadRight with 48 '.' will be added</param>
    /// <param name="status">Default: "Loading" or "Done"; Depends on if it's completed</param>
    /// <param name="overrideText">Default: <see langword="null"/>; If it's not null, This string will fully override the Loading Text</param>
    /// <param name="append">Default: <see langword="null"/>; If it's not null, This string will be appended as newline to final result</param>
    protected virtual void UpdateText(ref string name, ref string status, ref string overrideText, StringBuilder append)
    {

    }

    public static WaitUntil WaitUntil(Func<bool> predicate)
    {
        return new WaitUntil((Il2CppSystem.Func<bool>)predicate);
    }

    public static WaitUntil WaitForTask(Task task)
    {
        return WaitUntil(() => task.IsCompleted);
    }

    public static WaitUntil WaitForOtherJob(LoadingJob job)
    {
        return WaitUntil(() => job.IsCompleted);
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

    private static readonly StringBuilder _Builder = new();
    private static readonly StringBuilder _AppendBuilder = new();
}
