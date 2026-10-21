namespace GTFO.API.Assets.Profiles;

public abstract partial class AssetBundleProfile
{
    public sealed class OnLevelSelected : AssetBundleProfile
    {
        public override string ProfileName => nameof(OnLevelSelected);

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
            ManagingBundle.UnloadAllAssets();
            ManagingBundle.UnloadBundle();
        }
    }
}
