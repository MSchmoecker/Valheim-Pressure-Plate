using System;
using BepInEx;
using HarmonyLib;
using ValheimLib;

namespace PressurePlate {
    [BepInPlugin("com.maxsch.valheim.pressure_plate", "Pressure-Plate", "0.0.0")]
    [BepInProcess("valheim.exe")]
    public class Plugin : BaseUnityPlugin {
        internal static Plugin Instance { get; private set; }

        private void Awake() {
            Instance = this;
            Log.Init(Logger);
            
            Harmony harmony = new Harmony("com.maxsch.valheim.pressure_plate");
            harmony.PatchAll();

            Items.Init();
            Language.AddToken("$pressure_plate", "Pressure Plate");
        }
    }
}
