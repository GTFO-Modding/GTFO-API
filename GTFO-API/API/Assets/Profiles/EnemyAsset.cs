using AssetsTools.NET.Extra;
using Enemies;
using GTFO.API.Patches;

namespace GTFO.API.Assets.Profiles;

public abstract partial class AssetBundleProfile
{
    /// <summary>
    /// <see cref="AssetBundleProfile"/>: EnemyAsset
    /// <para>
    /// UnloadBundle and Assets when <see cref="EnemyPrefabManager.GenerateAllEnemyPrefabs"/> is done.
    /// </para>
    /// </summary>
    public sealed class EnemyAsset : AssetBundleProfile
    {
        /// <inheritdoc/>
        public override string ProfileName => nameof(EnemyAsset);

        /// <inheritdoc/>
        public override void Apply()
        {
            EnemyPrefabManager_Patches.OnAllPrefabBuilt += OnPrefabsBuilt;
        }

        /// <inheritdoc/>
        public override void Unapply()
        {
            EnemyPrefabManager_Patches.OnAllPrefabBuilt -= OnPrefabsBuilt;
        }

        private void OnPrefabsBuilt()
        {
            ManagingBundle.UnloadBundle();
            foreach (var prefabs in ManagingBundle.Assets.FindByType(AssetClassID.GameObject))
            {
                prefabs.Unload();
            }
        }
    }
}
