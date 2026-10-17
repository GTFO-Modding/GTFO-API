namespace GTFO.API.Assets.UnloadStrategies;

public abstract partial class AssetBundleUnloadStrategy
{
    public sealed class OnLevelSelected : AssetBundleUnloadStrategy
    {
        public override string StrategyName => nameof(OnLevelSelected);

        public override void Apply()
        {
            LevelAPI.OnLevelSelected += LevelSelected;
        }

        public override void Unapply()
        {
            LevelAPI.OnLevelSelected -= LevelSelected;
        }

        private void LevelSelected(eRundownTier expTier, int expIndexInTier, GameData.ExpeditionInTierData expData)
        {
            ManagingBundle.UnloadBundle(unloadAllLoadedAssets: true);
        }
    }
}
