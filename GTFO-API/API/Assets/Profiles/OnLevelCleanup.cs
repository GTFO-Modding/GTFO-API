namespace GTFO.API.Assets.Profiles;

public abstract partial class AssetBundleProfile
{
    public sealed class OnLevelCleanup : AssetBundleProfile
    {
        public override string ProfileName => nameof(OnLevelCleanup);

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
            ManagingBundle.UnloadAllAssets();
            ManagingBundle.UnloadBundle();
        }
    }
}
