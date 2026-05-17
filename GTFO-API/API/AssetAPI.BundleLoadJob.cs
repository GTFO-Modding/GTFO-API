using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GTFO.API.Utilities;
using UnityEngine;

namespace GTFO.API;

internal sealed class AssetBundlesLoadJob : LoadingJob
{
    public override string JobName => "AssetBundles";
    public override string DisplayName => "Loading AssetBundles";

    public ICollection<string> BundlePathsToLoad = [];

    private List<LoadBundleJob> _Jobs = [];
    private int _BundlesToLoadCount = 0;

    private sealed class LoadBundleJob
    {


        public string BundleName = "";
        public bool IsDone = false;
        public AssetBundleRequest ActiveRequest = null;

        private List<(string assetName, AssetBundleRequest request)> _Requests = [];
        private static readonly Regex _AssetNameRegex = new("(?<=\\/)[^\\/]+(?=\\.[^\\.]+$)");

        public IEnumerator Load(string path)
        {
            var loadJob = AssetBundle.LoadFromFileAsync(path);
            yield return loadJob;

            var bundle = loadJob.assetBundle;
            if (bundle == null)
            {
                APILogger.Warn(nameof(AssetAPI), $"Failed to load asset bundle '{path}'");
            }

            string[] assetNames = bundle.AllAssetNames();
            /*
            APILogger.Verbose($"Asset", $"Bundle names: [{string.Join(", ", assetNames)}]");
            foreach (string assetName in assetNames)
            {
                _Requests.Add((assetName, bundle.LoadAssetAsync(assetName)));
            }
            */

            ActiveRequest = bundle.LoadAllAssetsAsync();
            yield return ActiveRequest;

            for (int i = 0; i < assetNames.Length; i++)
            {
                var fullname = assetNames[i];
                UnityEngine.Object asset = ActiveRequest.allAssets[i];

                if (!Path.GetFileNameWithoutExtension(fullname).Equals(asset.name, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    APILogger.Error($"Asset", $"Asset Fullname Mismatches?? {asset.name} <-> {fullname}");
                }

                if (asset != null) AssetAPI.RegisterAsset(fullname, asset);
                else APILogger.Warn("Asset", $"Skipping asset {fullname}");
                yield return null;
            }

            IsDone = true;
            APILogger.Warn($"Asset", $"BundleLoaded: {path}");
        }
    }

    protected override IEnumerator Job()
    {
        foreach (var path in BundlePathsToLoad)
        {
            var job = new LoadBundleJob();

            var bundleName = Path.GetFileNameWithoutExtension(path);
            if (bundleName.EndsWith("-compressed"))
                bundleName = bundleName[..^"-compressed".Length];

            job.BundleName = bundleName;

            CoroutineDispatcher.StartCoroutine(job.Load(path));
            _Jobs.Add(job);
            _BundlesToLoadCount++;
        }

        yield return WaitUntil(() => _Jobs.All(job => job.IsDone));
    }

    protected override void UpdateText(ref string name, ref string status, ref string overrideText, StringBuilder append)
    {
        if (_BundlesToLoadCount <= 0)
            return;

        var doneCount = 0;
        foreach (var job in _Jobs)
        {
            if (job.IsDone)
                doneCount++;
            else
            {
                append.Append(" - ");
                append.Append(job.BundleName);
                append.Append("...");
                if (job.ActiveRequest != null) append.Append(job.ActiveRequest.progress.ToString("P2"));
                append.AppendLine();
            }
        }

        if (_BundlesToLoadCount == doneCount)
            status = "Done";
        else
            status = ((float)doneCount / (float)_BundlesToLoadCount).ToString("P");
    }
}
