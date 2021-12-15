using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace PressurePlate {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "Pressure-Plate";
        public const string ModGuid = "com.maxsch.valheim.pressure_plate";
        public const string ModVersion = "0.6.0";

        public ConfigEntry<float> plateVolume;
        internal static Plugin Instance { get; private set; }
        private Harmony harmony;

        private void Awake() {
            Instance = this;

            Config.Clear();

            harmony = new Harmony(ModGuid);
            harmony.PatchAll(typeof(DoorPatches));
            harmony.PatchAll(typeof(MenuPatches));

            CustomLocalization localization = new CustomLocalization();
            localization.AddJsonFile("English", GetTextFileFromResources("Localization.English.json"));
            localization.AddJsonFile("German", GetTextFileFromResources("Localization.German.json"));
            LocalizationManager.Instance.AddLocalization(localization);

            AcceptableValueRange<float> percentRange = new AcceptableValueRange<float>(0f, 100f);

            const string plateVolumeDescription = "Volume of the press and release sound of pressure plates. " +
                                                  "Value in percent, can be changed while ingame";
            plateVolume = Config.Bind("Sound", "Plate Volume", 100f, new ConfigDescription(plateVolumeDescription, percentRange));

            AssetBundle assetBundle = GetAssetBundleFromResources("pressure_plate");
            Items.Init(assetBundle);

            GUIManager.OnCustomGUIAvailable += () => PressurePlateUI.Init(assetBundle);

            DoorConfig.AddDoorConfig("h_drawbridge01", new DoorConfig { openClosedInverted = true });
            DoorConfig.AddDoorConfig("h_drawbridge02", new DoorConfig { openClosedInverted = true });
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
