﻿using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace GTFO.API.Utilities.Impl
{
    internal class CoroutineDispatcher_Impl : MonoBehaviour
    {
        public static CoroutineDispatcher_Impl Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    CoroutineDispatcher_Impl existing = FindObjectOfType<CoroutineDispatcher_Impl>();
                    if (existing != null) s_Instance = existing;
                }
                return s_Instance;
            }
        }

        static CoroutineDispatcher_Impl()
        {
            AssetAPI.OnStartupAssetsLoaded += OnAssetsLoaded;
        }

        private static void OnAssetsLoaded()
        {
            if (s_Instance != null) return;

            GameObject dispatcher = new();
            CoroutineDispatcher_Impl dispatcherComp = dispatcher.AddComponent<CoroutineDispatcher_Impl>();
            dispatcher.name = "GTFO-API Coroutine Dispatcher";
            dispatcher.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(dispatcher);

            s_Instance = dispatcherComp;
        }

        private void Update()
        {
            if (m_HasInLevelCoroutines && !GameStateManager.IsInExpedition)
            {
                m_InLevelCoroutines.ForEach((coroutine) => { StopCoroutine(coroutine); });
                m_InLevelCoroutines.Clear();
                m_HasInLevelCoroutines = false;
            }
        }

        [HideFromIl2Cpp]
        internal Coroutine RunCoroutine(IEnumerator routine)
        {
            return this.StartCoroutine(routine);
        }

        [HideFromIl2Cpp]
        internal Coroutine RunInLevelCoroutine(IEnumerator routine)
        {
            if (!GameStateManager.IsInExpedition)
            {
                APILogger.Error(nameof(CoroutineDispatcher), "Cannot run InLevelCoroutine while you're not in level!");
                return null;
            }

            var coroutine = this.StartCoroutine(routine);
            m_InLevelCoroutines.Add(coroutine);
            m_HasInLevelCoroutines = true;
            return coroutine;
        }

        private bool m_HasInLevelCoroutines = false;
        private readonly List<Coroutine> m_InLevelCoroutines = [];
        private static CoroutineDispatcher_Impl s_Instance = null;
    }
}
