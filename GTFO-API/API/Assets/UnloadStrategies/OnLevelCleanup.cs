namespace GTFO.API.Assets.UnloadStrategies;

public abstract partial class AssetBundleUnloadStrategy
{
    public sealed class OnLevelCleanup : AssetBundleUnloadStrategy
    {
        public override string StrategyName => nameof(OnLevelCleanup);

        public override void Apply()
        {
            LevelAPI.OnLevelCleanup += OnLevelClear;
        }

        public override void Unapply()
        {
            LevelAPI.OnLevelCleanup -= OnLevelClear;
        }

        private void OnLevelClear()
        {
            ManagingBundle.UnloadBundle(unloadAllLoadedAssets: true);
        }
    }
}
