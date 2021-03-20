﻿using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using ValheimLib;

namespace PressurePlate {
    [BepInPlugin(ModGuid, "Pressure-Plate", "0.0.3")]
    [BepInDependency(ValheimLib.ValheimLib.ModGuid)]
    [BepInProcess("valheim.exe")]
    public class Plugin : BaseUnityPlugin {
        public const string ModGuid = "com.maxsch.valheim.pressure_plate";
        internal static Plugin Instance { get; private set; }

        private void Awake() {
            Instance = this;
            Log.Init(Logger);

            Harmony harmony = new Harmony(ModGuid);
            harmony.PatchAll();

            Language.AddToken("$pressure_plate", "Pressure Plate");
        }
    }
}
