using System;
using System.IO;
using BepInEx;
using GTFO.API.NativePatches;
using HarmonyLib;
using Il2CppInterop.Runtime;

namespace GTFO.API.Patches
{
    [HarmonyPatch(typeof(CellJSON_Native))]
    internal static class FavoriteGearFile_Patches
    {
        private static string FavoritesDirectory => Path.Combine(Paths.BepInExRootPath, "GameData", "Favorites");
        private static string FavoritePath => Path.Combine(FavoritesDirectory, "Gear.json");
        private static string BotFavoritePath => Path.Combine(FavoritesDirectory, "BotGear.json");

        [HarmonyPrefix]
        [HarmonyPatch(nameof(CellJSON_Native.ReadFromDisk__T__String_BooleanByRef))]
        private static void ReadFromDisk_Prefix(ref IntPtr n_path)
        {
            string path = new Il2CppSystem.String(n_path);

            if (path.EndsWith("GTFO_Favorites.txt", StringComparison.OrdinalIgnoreCase))
            {
                EnsureFavoriteDirectory();
                n_path = IL2CPP.ManagedStringToIl2Cpp(FavoritePath);
            }
            else if (path.EndsWith("GTFO_BotFavorites.txt", StringComparison.OrdinalIgnoreCase))
            {
                EnsureFavoriteDirectory();
                n_path = IL2CPP.ManagedStringToIl2Cpp(BotFavoritePath);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(CellJSON_Native.SaveToDisk__Void__String_T))]
        private static void SaveToDisk_Prefix(ref IntPtr n_path)
        {
            string path = new Il2CppSystem.String(n_path);

            if (path.EndsWith("GTFO_Favorites.txt", StringComparison.OrdinalIgnoreCase))
            {
                EnsureFavoriteDirectory();
                n_path = IL2CPP.ManagedStringToIl2Cpp(FavoritePath);
            }
            else if (path.EndsWith("GTFO_BotFavorites.txt", StringComparison.OrdinalIgnoreCase))
            {
                EnsureFavoriteDirectory();
                n_path = IL2CPP.ManagedStringToIl2Cpp(BotFavoritePath);
                if (File.Exists(path))
                    File.Delete(path); //Remove Existing BotFavorite File
            }
        }

        private static void EnsureFavoriteDirectory()
        {
            Directory.CreateDirectory(FavoritesDirectory);
        }
    }
}
