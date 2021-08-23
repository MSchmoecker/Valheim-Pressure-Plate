using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;

namespace PressurePlate {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "Pressure-Plate";
        public const string ModGuid = "com.maxsch.valheim.pressure_plate";
        public const string ModVersion = "0.4.1";

        internal static Plugin Instance { get; private set; }
        private Harmony harmony;

        private void Awake() {
            Instance = this;
            Log.Init(Logger);

            Config.Clear();

            harmony = new Harmony(ModGuid);
            harmony.PatchAll(typeof(DoorPatches));
            harmony.PatchAll(typeof(MenuPatches));

            LocalizationManager.Instance.AddJson("English", GetTextFileFromResources("Localization.English.json"));
            LocalizationManager.Instance.AddJson("German", GetTextFileFromResources("Localization.German.json"));

            AssetBundle assetBundle = GetAssetBundleFromResources("pressure_plate");
            Items.Init(assetBundle);

            GUIManager.OnPixelFixCreated += () => PressurePlateUI.Init(assetBundle);
        }

        private void OnDestroy() {
            harmony?.UnpatchAll(ModGuid);
        }

        public static AssetBundle GetAssetBundleFromResources(string fileName) {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
            using Stream stream = execAssembly.GetManifestResourceStream(resourceName);
            return AssetBundle.LoadFromStream(stream);
        }

        public static string GetTextFileFromResources(string fileName) {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
            using Stream stream = execAssembly.GetManifestResourceStream(resourceName);
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
