namespace GTFO.API.Assets.Profiles;

public abstract partial class AssetBundleProfile
{
    public sealed class OnLevelBuildDone : AssetBundleProfile
    {
        public override string ProfileName => nameof(OnLevelBuildDone);

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
            ManagingBundle.UnloadAllAssets();
            ManagingBundle.UnloadBundle();
        }
    }
}
