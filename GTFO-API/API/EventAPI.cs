using System;
using AssetShards;
using Globals;
using GTFO.API.Attributes;
using GTFO.API.Resources;
using GTFO.API.Utilities;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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
        /// Invoked when very first scene got loaded into game (First Game Load)
        /// <list>
        /// - Useful for Initializing a Singleton Object that Independent from Vanilla codebase.
        /// </list>
        /// </summary>
        public static event Action OnInitialSceneLoaded;

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


        private static bool _InitialSceneLoaded = false;

        internal static void Setup()
        {
            Global.add_OnAllManagersSetup((Action)ManagersSetup);
            AssetShardManager.add_OnStartupAssetsLoaded((Action)AssetsLoaded);
            RundownManager.add_OnExpeditionGameplayStarted((Action)ExpeditionStarted);
            SceneManager.add_sceneLoaded((UnityAction<Scene, LoadSceneMode>)((scene, mode) =>
            {
                if (!_InitialSceneLoaded)
                {
                    SafeInvoke.Invoke(OnInitialSceneLoaded);
                    _InitialSceneLoaded = true;
                }
            }));
        }

        private static void ManagersSetup() => SafeInvoke.Invoke(OnManagersSetup);
        private static void ExpeditionStarted() => SafeInvoke.Invoke(OnExpeditionStarted);
        private static void AssetsLoaded() => SafeInvoke.Invoke(OnAssetsLoaded);
    }
}
