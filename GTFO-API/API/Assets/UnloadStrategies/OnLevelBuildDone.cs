namespace GTFO.API.Assets.UnloadStrategies;

public abstract partial class AssetBundleUnloadStrategy
{
    public sealed class OnLevelBuildDone : AssetBundleUnloadStrategy
    {
        public override string StrategyName => nameof(OnLevelBuildDone);

        public override void Apply()
        {
            LevelAPI.OnBuildDone += OnBuildDone;
        }

        public override void Unapply()
        {
            LevelAPI.OnBuildDone -= OnBuildDone;
        }

        private void OnBuildDone()
        {
            ManagingBundle.UnloadBundle(unloadAllLoadedAssets: true);
        }
    }
}
