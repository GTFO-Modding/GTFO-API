using GTFO.API.Utilities;
using HarmonyLib;
using Localization;

namespace GTFO.API.Patches;

[HarmonyPatch(typeof(GS_Offline))]
internal static class GS_Offline_Patches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GS_Offline.Update))]
    static bool Prefix()
    {
        var intro = MainMenuGuiLayer.Current.PageIntro;
        var text = MainMenuGuiLayer.Current.PageIntro.m_textCenter;
        var loadingPosted = intro.m_loadingPosted;
        var runOriginal = true;
        var allCompleted = LoadingAPI.AllJobsCompleted;
        if (loadingPosted)
        {
            text.ClearBuffer();
            text.m_text.fontSize = 16f;
            text.m_buffer.Add("");
            if (allCompleted)
                text.m_buffer.Add($"<size=20>{Text.Get(54U)}</size>");
            else
                text.m_buffer.Add($"<size=20>Waiting for custom assets to loaded..</size>");
            text.m_buffer.Add("");
        }

        foreach (var jobs in LoadingAPI.Jobs.Values)
        {
            if (!jobs.IsCompleted)
            {
                if (!jobs.IsRunning)
                {
                    CoroutineDispatcher.StartCoroutine(jobs.DoJob());
                }
            }
            if (loadingPosted)
            {
                jobs.DoUpdateTexts();
                text.m_buffer.Add(jobs.DisplayText);
            }
        }

        if (loadingPosted)
        {
            if (allCompleted) text.m_buffer.Add("Custom Assets All Loaded");
            text.UpdateText();
        }

        runOriginal = allCompleted;
        return runOriginal;
    }
}
