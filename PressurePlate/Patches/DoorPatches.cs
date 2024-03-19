using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace PressurePlate {
    [HarmonyPatch]
    public static class DoorPatches {
        [HarmonyPatch(typeof(Door), "Awake"), HarmonyPostfix]
        public static void DoorAwake(Door __instance) {
            if (!__instance.GetComponent<DoorPowerState>()) {
                __instance.gameObject.AddComponent<DoorPowerState>();
            }
        }
    }

    public class DoorPowerState : PowerState {
        private Door door;

        protected override void Awake() {
            base.Awake();
            door = GetComponent<Door>();
        }

        protected override void Interact(Plate plate) {
            if (door.m_keyItem) {
                return;
            }

            Vector3 deltaDir = (plate.transform.position - transform.position).normalized;
            bool forward = Vector3.Dot(transform.forward, deltaDir) < 0.0f;
            door.m_nview.InvokeRPC("UseDoor", forward);
        }

        public override bool CanInteract(Player activator, ZDO targetArea) {
            return Plate.CanInteractWithDoor(activator, this, targetArea);
        }
    }
}
