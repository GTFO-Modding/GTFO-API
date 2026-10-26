namespace GTFO.API.Assets.Profiles;

public abstract partial class AssetBundleProfile
{
    /// <summary>
    /// <see cref="AssetBundleProfile"/>: LevelAsset
    /// <para>
    /// Unload Bundle on LevelAPI.OnBuildDone
    /// Unload Assets on LevelAPI.OnLevelCleanup
    /// </para>
    /// </summary>
    public sealed class LevelAsset : AssetBundleProfile
    {
        /// <inheritdoc/>
        public override string ProfileName => nameof(LevelAsset);

        /// <inheritdoc/>
        public override void Apply()
        {
            LevelAPI.OnBuildDone += OnBuildDone;
            LevelAPI.OnLevelSelected += OnLevelSelected;
        }

        /// <inheritdoc/>
        public override void Unapply()
        {
            LevelAPI.OnBuildDone -= OnBuildDone;
            LevelAPI.OnLevelSelected -= OnLevelSelected;
        }

        private void OnBuildDone()
        {
            ManagingBundle.UnloadBundle();
        }

        private void OnLevelSelected(eRundownTier expTier, int expIndexInTier, GameData.ExpeditionInTierData expData)
        {
            ManagingBundle.UnloadAllAssets();
        }
    }
}
