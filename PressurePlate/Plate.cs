using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using ValheimLib;
using ValheimLib.ODB;

namespace PressurePlate {
    public class Plate : MonoBehaviour {
        public GameObject plate;
        public static List<Door> allDoors = new List<Door>();
        public bool isPressed;
        public Player lastPlayer;
        private float notPressedCooldown;

        private void Awake() {
            isPressed = FindPlayerInRange();
        }

        private void FixedUpdate() {
            if (!GetComponent<ZNetView>()) {
                return;
            }
            
            bool wasPressed = isPressed;
            bool newPressed = FindPlayerInRange();

            if (newPressed) {
                notPressedCooldown = 1;
                isPressed = true;
            }

            if (!newPressed) {
                if (notPressedCooldown > 0) {
                    notPressedCooldown -= Time.fixedDeltaTime;
                    isPressed = true;
                } else {
                    isPressed = false;
                }
            }

            Vector3 pos = isPressed ? new Vector3(0f, -0.025f, 0f) : new Vector3(0f, 0.05f, 0f);
            plate.transform.localPosition = pos;

            if (lastPlayer == null) {
                return;
            }

            if (isPressed != wasPressed || isPressed) {
                allDoors.RemoveAll(i => i == null);

                List<Door> doors = allDoors.FindAll(i => InRange(i.transform, 3f));

                foreach (Door door in doors) {
                    if (!door.GetComponent<ZNetView>()) {
                        continue;
                    }

                    int state = door.GetComponent<ZNetView>().GetZDO().GetInt("state");
                    if (isPressed && state == 0 || !isPressed && state != 0) {
                        door.Interact(lastPlayer, false);
                    }
                }
            }
        }

        private bool FindPlayerInRange() {
            Player player = Player.GetAllPlayers().Find(i => InRange(i.transform, 1f));

            if (player != null) {
                lastPlayer = player;
                return true;
            }

            return false;
        }

        private bool InRange(Transform target, float range) {
            return Vector3.Distance(transform.position, target.position) <= range;
        }

        // private void OnTriggerEnter(Collider other) {
        //     Player player = other.GetComponent<Player>();
        //     if (player == null) {
        //         return;
        //     }
        //
        //     lastPlayer = player;
        // }
        //
        // private void OnTriggerExit(Collider other) {
        // }
    }

    [HarmonyPatch]
    public static class DoorPatch {
        [HarmonyPatch(typeof(Door), "Awake"), HarmonyPostfix]
        public static void DoorAwake(Door __instance) {
            if (!Plate.allDoors.Contains(__instance)) {
                Plate.allDoors.Add(__instance);
            }
        }
    }
}
