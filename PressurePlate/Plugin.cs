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
        public const string ModVersion = "0.3.1";

        internal static Plugin Instance { get; private set; }
        private Harmony harmony;

        private void Awake() {
            Instance = this;
            Log.Init(Logger);

            Config.Clear();

            harmony = new Harmony(ModGuid);
            harmony.PatchAll(typeof(DoorPatches));

            LocalizationManager.Instance.AddToken("$pressure_plate", "Pressure Plate", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_wood", "Wooden Pressure Plate", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_stone", "Stone Pressure Plate", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_public_text", "Ignore access rights to doors", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_private_text", "Respect access rights to doors", false);
            LocalizationManager.Instance.AddToken("$public", "Public", false);
            LocalizationManager.Instance.AddToken("$private", "Private", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_set_public", "Set public", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_set_private", "Set private", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_trigger_delay", "Trigger delay", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_seconds_short", "s", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_trigger_delay_off", "off", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_trigger_delay_description", "Change time", false);

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
    }
}
