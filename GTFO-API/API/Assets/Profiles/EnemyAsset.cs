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
        public override string ProfileName => nameof(EnemyAsset);

        public override void Apply()
        {
            EnemyPrefabManager_Patches.OnAllPrefabBuilt += OnPrefabsBuilt;
        }

        public override void Unapply()
        {
            EnemyPrefabManager_Patches.OnAllPrefabBuilt -= OnPrefabsBuilt;
        }

        private void OnPrefabsBuilt()
        {
            ManagingBundle.UnloadBundle();
            ManagingBundle.UnloadAllAssets();
        }
    }
}
