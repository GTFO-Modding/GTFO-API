using System;
using System.Collections.Generic;
using System.Linq;
using GTFO.API.Attributes;
using GTFO.API.Resources;

namespace GTFO.API;

[API("Loading")]
public static class LoadingAPI
{
    public static ApiStatusInfo Status => APIStatus.Loading;

    public static IReadOnlyDictionary<string, LoadingJob> Jobs => _Jobs;

    public static bool AllJobsCompleted => _Jobs.Values.All(job => job.IsCompleted);

    internal static void Setup()
    {
        APIStatus.Loading.Ready = true;
        APIStatus.Loading.Created = true;
        AssetAPI.OnStartupAssetsLoaded += StartupAssetLoaded;
    }

    private static void StartupAssetLoaded()
    {
        _CanAddJobs = false;
    }

    public static void RegisterJob(LoadingJob job)
    {
        if (!_CanAddJobs)
        {
            APILogger.Error(nameof(LoadingAPI), $"LoadingJobs cannot be added after done loading!{Environment.NewLine}{Environment.StackTrace}");
            return;
        }

        if (_Jobs.ContainsKey(job.JobName))
        {
            APILogger.Error(nameof(LoadingAPI), $"Duplicated JobName! '{job.JobName}'{Environment.NewLine}{Environment.StackTrace}");
            return;
        }

        _Jobs[job.JobName] = job;
    }

    private static bool _CanAddJobs = true;
    private static readonly Dictionary<string, LoadingJob> _Jobs = [];
}
