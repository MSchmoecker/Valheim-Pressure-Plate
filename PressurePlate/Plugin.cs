using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace PressurePlate {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Minor)]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "Pressure-Plate";
        public const string ModGuid = "com.maxsch.valheim.pressure_plate";
        public const string ModVersion = "0.9.1";

        private readonly AcceptableValueRange<float> percentRange = new AcceptableValueRange<float>(0f, 100f);
        public ConfigEntry<float> plateVolume;

        internal static Plugin Instance { get; private set; }
        private Harmony harmony;

        private void Awake() {
            Instance = this;

            harmony = new Harmony(ModGuid);
            harmony.PatchAll(typeof(DoorPatches));

            CustomLocalization localization = LocalizationManager.Instance.GetLocalization();
            localization.AddJsonFile("English", GetTextFileFromResources("English.json"));
            localization.AddJsonFile("German", GetTextFileFromResources("German.json"));
            localization.AddJsonFile("Spanish", GetTextFileFromResources("Spanish.json"));
            localization.AddJsonFile("Portuguese_Brazilian", GetTextFileFromResources("Portuguese_Brazilian.json"));

            const string plateVolumeDescription = "Volume of the press and release sound of pressure plates. Value in percent, can be changed while ingame";
            plateVolume = Config.Bind("Sound", "Plate Volume", 100f, new ConfigDescription(plateVolumeDescription, percentRange));

            AssetBundle assetBundle = AssetUtils.LoadAssetBundleFromResources("pressure_plate");
            Items.Init(assetBundle);

            GUIManager.OnCustomGUIAvailable += () => PressurePlateUI.Init(assetBundle);

            DoorConfig.AddDoorConfig("h_drawbridge01", new DoorConfig { openClosedInverted = true });
            DoorConfig.AddDoorConfig("h_drawbridge02", new DoorConfig { openClosedInverted = true });
        }

        public static string GetTextFileFromResources(string fileName) {
            return AssetUtils.LoadTextFromResources(fileName, Assembly.GetExecutingAssembly());
        }
    }
}
