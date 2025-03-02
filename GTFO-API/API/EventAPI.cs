using System;
using AssetShards;
using Globals;
using GTFO.API.Attributes;
using GTFO.API.Resources;
using GTFO.API.Utilities;

namespace GTFO.API
{
    [API("Event")]
    public static class EventAPI
    {
        /// <summary>
        /// Status info for the <see cref="EventAPI"/>
        /// </summary>
        public static ApiStatusInfo Status => APIStatus.Event;

        /// <summary>
        /// Invoked when all native managers are set up
        /// </summary>
        public static event Action OnManagersSetup;

        /// <summary>
        /// Invoked when the player exits the elevator and is able to move
        /// </summary>
        public static event Action OnExpeditionStarted;

        /// <summary>
        /// Invoked when all native assets are loaded
        /// </summary>
        public static event Action OnAssetsLoaded;

        internal static void Setup()
        {
            Global.add_OnAllManagersSetup((Action)ManagersSetup);
            AssetShardManager.add_OnStartupAssetsLoaded((Action)AssetsLoaded);
            RundownManager.add_OnExpeditionGameplayStarted((Action)ExpeditionStarted);
        }

        private static void ManagersSetup() => SafeInvoke.Invoke(OnManagersSetup);
        private static void ExpeditionStarted() => SafeInvoke.Invoke(OnExpeditionStarted);
        private static void AssetsLoaded() => SafeInvoke.Invoke(OnAssetsLoaded);
    }
}
