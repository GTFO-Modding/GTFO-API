﻿using System.IO;
using BepInEx;
using System.Text.RegularExpressions;
using System.Text.Json;
using JsonSerializer = GTFO.API.JSON.JsonSerializer;

namespace GTFO.API.Utilities
{
    /// <summary>
    /// Utility class used to easily store configuration and data on disk in JSON format
    /// </summary>
    /// <typeparam name="T">The data type to store on disk</typeparam>
    public class PersistentData<T> where T : PersistentData<T>, new()
    {
        private const string VERSION_REGEX = @"""PersistentDataVersion"": ""(.+?)""";

        private static T s_CurrentData;

        /// <summary>
        /// The current data instance, loaded automatically when first accessed
        /// </summary>
        public static T CurrentData
        {
            get
            {
                if (s_CurrentData != null)
                {
                    return s_CurrentData;
                }
                else
                {
                    s_CurrentData = Load();
                    return s_CurrentData;
                }
            }
            set
            {
                s_CurrentData = value;
            }
        }

        /// <summary>
        /// The default data path on disk
        /// </summary>
        protected static string persistentPath
        {
            get
            {
                return Path.Combine(Paths.PluginPath, "PersistentData", typeof(T).Assembly.GetName().Name, $"{typeof(T).Name}.json");
            }
        }

        /// <summary>
        /// The version of the stored data
        /// </summary>
        public virtual string PersistentDataVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Loads the stored data from the default path and creates default if it didn't exist
        /// </summary>
        /// <returns>The stored data or default if it didn't exist</returns>
        public static T Load()
        {
            return Load(persistentPath);
        }

        /// <summary>
        /// Loads the stored data from the specified path and creates default if it didn't exist
        /// </summary>
        /// <param name="path">The path to load from</param>
        /// <returns>The stored data or default if it didn't exist</returns>
        public static T Load(string path)
        {
            T res = new();

            if (File.Exists(path))
            {
                string contents = File.ReadAllText(path);
                T deserialized;

                try
                {
                    deserialized = JsonSerializer.Deserialize<T>(contents);
                }
                catch (JsonException)
                {                    
                    APILogger.Warn("JSON", $"Failed to deserialize {typeof(T).Name}, replacing with default");

                    string version = "FAILED";

                    Match match = Regex.Match(contents, VERSION_REGEX);
                    if (match.Success)
                    {
                        version = $"{match.Groups[1].Value}-FAILED";
                    }

                    File.WriteAllText($"{Path.ChangeExtension(path, null)}-{version}.json", contents);
                    deserialized = new();
                    deserialized.Save(path);
                }

                if (deserialized.PersistentDataVersion != res.PersistentDataVersion)
                {
                    deserialized.Save($"{Path.ChangeExtension(path, null)}-{deserialized.PersistentDataVersion}.json");
                    res.Save(path);
                }
                else
                    res = deserialized;
            }
            else
            {
                res.Save(path);
            }

            return res;
        }

        /// <summary>
        /// Saves this data to the default path
        /// </summary>
        public void Save()
        {
            Save(persistentPath);
        }

        /// <summary>
        /// Saves this data to the specified path
        /// </summary>
        /// <param name="path">The path to save to</param>
        public void Save(string path)
        {
            string contents = JsonSerializer.Serialize((T)this);
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(path, contents);
        }
    }
}
