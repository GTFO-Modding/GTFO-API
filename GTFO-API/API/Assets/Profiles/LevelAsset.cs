namespace GTFO.API.Assets.Profiles;

public abstract partial class AssetBundleProfile
{
    /// <summary>
    /// AssetBundleProfile: LevelAsset
    /// <para>
    /// Unload Bundle on LevelAPI.OnBuildDone
    /// Unload Asset on 
    /// </para>
    /// </summary>
    public sealed class LevelAsset : AssetBundleProfile
    {
        public override string ProfileName => nameof(LevelAsset);

        public override void Apply()
        {
            LevelAPI.OnBuildDone += OnBuildDone;
            LevelAPI.OnLevelCleanup += OnLevelCleanup;
        }

        private void OnBuildDone()
        {
            ManagingBundle.UnloadBundle();
        }

        private void OnLevelCleanup()
        {
            ManagingBundle.UnloadAllAssets();
        }

        public override void Unapply()
        {
            LevelAPI.OnBuildDone -= OnBuildDone;
            LevelAPI.OnLevelCleanup -= OnLevelCleanup;
        }
    }
}
