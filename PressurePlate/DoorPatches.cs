using System;
using HarmonyLib;

namespace PressurePlate {
    [HarmonyPatch]
    public static class DoorPatches {
        [HarmonyPatch(typeof(Door), "Awake"), HarmonyPostfix]
        public static void DoorAwake(Door __instance) {
            if (!Plate.allDoors.Contains(__instance)) {
                Plate.allDoors.Add(__instance);
            }
        }

        public static int GetState(this Door door) {
            int state = door.GetComponent<ZNetView>().GetZDO().GetInt("state");
            DoorConfig doorConfig = DoorConfig.GetDoorConfig(door.name);

            if (doorConfig == null) {
                return state;
            }

            if (doorConfig.openClosedInverted) {
                state = 1 - Math.Abs(state);
            }

            return state;
        }
    }
}
