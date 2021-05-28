using HarmonyLib;

namespace PressurePlate {
    [HarmonyPatch]
    public class DoorPatches {
        [HarmonyPatch(typeof(Door), "Awake"), HarmonyPostfix]
        public static void DoorAwake(Door __instance) {
            if (!Plate.allDoors.Contains(__instance)) {
                Plate.allDoors.Add(__instance);
            }
        }
    }
}
