using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ValheimLib;

namespace PressurePlate {
    [BepInPlugin(ModGuid, "Pressure-Plate", "0.0.5")]
    [BepInDependency(ValheimLib.ValheimLib.ModGuid)]
    public class Plugin : BaseUnityPlugin {
        public const string ModGuid = "com.maxsch.valheim.pressure_plate";
        internal static Plugin Instance { get; private set; }

        public static ConfigEntry<float> plateRadiusXZ;
        public static ConfigEntry<float> plateRadiusY;
        public static ConfigEntry<float> playerPlateRadiusXZ;
        public static ConfigEntry<float> playerPlateRadiusY;

        private void Awake() {
            Instance = this;
            Log.Init(Logger);

            plateRadiusXZ = Config.Bind<float>("General", "PressurePlateRadiusHorizontal", 3, new ConfigDescription("Max horizontal distance from a pressure plate to open/close a door. Value in Unity units, e.g. 2 is 1m in Valheim"));
            plateRadiusY = Config.Bind<float>("General", "PressurePlateRadiusVertical", 3, new ConfigDescription("Max vertical distance from a pressure plate to open/close a door. Value in Unity units, e.g. 2 is 1m in Valheim"));
            playerPlateRadiusXZ = Config.Bind<float>("General", "PressurePlatePlayerRadiusHorizontal", 1, new ConfigDescription("Max horizontal distance from a player to trigger a pressure plate. Value in Unity units, e.g. 2 is 1m in Valheim"));
            playerPlateRadiusY = Config.Bind<float>("General", "PressurePlatePlayerRadiusVertical", 1, new ConfigDescription("Max vertical distance from a player to trigger a pressure plate. Value in Unity units, e.g. 2 is 1m in Valheim"));

            Harmony harmony = new Harmony(ModGuid);
            harmony.PatchAll();

            Language.AddToken("$pressure_plate_wood", "Wooden Pressure Plate");
            Language.AddToken("$pressure_plate_stone", "Stone Pressure Plate");
            Items.Init();
        }
    }
}
