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
            harmony.PatchAll(typeof(MenuPatches));

            LocalizationManager.Instance.AddToken("$pressure_plate", "Pressure Plate", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_wood", "Wooden Pressure Plate", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_stone", "Stone Pressure Plate", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_trigger_radius_horizontal", "Trigger Radius Horizontal", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_trigger_radius_vertical", "Trigger Radius Vertial", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_open_radius_horizontal", "Door Radius Horizontal", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_open_radius_vertical", "Door Radius Vertical", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_open_time", "Activation Time", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_trigger_delay", "Trigger Delay", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_invert", "Invert Doors", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_ignore_wards", "Ignore Wards", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_copy", "Copy", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_paste", "Paste", false);
            LocalizationManager.Instance.AddToken("$pressure_plate_reset", "Reset", false);

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
