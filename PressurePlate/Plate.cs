﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PressurePlate {
    public class Plate : MonoBehaviour {
        public GameObject plate;
        public static List<Door> allDoors = new List<Door>();
        public static List<Plate> allPlates = new List<Plate>();
        public bool isPressed;
        public Player lastPlayer;
        private float notPressedCooldown;
        public EffectList pressEffects = new EffectList();
        public EffectList releaseEffects = new EffectList();

        private void Awake() {
            allPlates.Add(this);
            isPressed = FindPlayerInRange();
        }

        private void FixedUpdate() {
            if (!GetComponent<ZNetView>()) {
                return; //wait for network spawn
            }

            bool wasPressed = isPressed;
            bool newPressed = FindPlayerInRange();

            if (newPressed) {
                notPressedCooldown = Plugin.plateOpenDelay.Value;
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

            bool stateChange = isPressed != wasPressed;

            Vector3 pos = isPressed ? new Vector3(0f, -0.025f, 0f) : new Vector3(0f, 0.05f, 0f);
            plate.transform.localPosition = pos;

            if (stateChange) {
                if (isPressed) {
                    pressEffects.Create(transform.position, Quaternion.identity);
                } else {
                    releaseEffects.Create(transform.position, Quaternion.identity);
                }
            }

            if (lastPlayer == null) {
                return;
            }

            if (stateChange || isPressed) {
                allDoors.RemoveAll(i => i == null);

                List<Door> doors = FindDoorsInPlateRange();

                foreach (Door door in doors) {
                    if (!door.GetComponent<ZNetView>()) {
                        continue; //wait for network spawn
                    }

                    int state = door.GetComponent<ZNetView>().GetZDO().GetInt("state");

                    if (!PrivateHelper.InvokePrivateMethod<Door, bool>(door, "CanInteract", new object[0])) {
                        isPressed = true; //wait till door can be closed 
                    }

                    if (isPressed && state == 0) { //open
                        door.Interact(lastPlayer, false);
                    }

                    if (!isPressed && state != 0) { //close
                        if (allPlates.Any(i => i != this && i.isPressed && i.FindDoorsInPlateRange().Contains(door))) {
                            continue; //don't close the door if another plate is pressed
                        }

                        door.Interact(lastPlayer, false);
                    }
                }
            }
        }

        public List<Door> FindDoorsInPlateRange() {
            return allDoors.FindAll(i => InRange(i.transform.position, Plugin.plateRadiusXZ.Value, Plugin.plateRadiusY.Value));
        }

        private bool FindPlayerInRange() {
            Player player = Player.GetAllPlayers().Find(i => InRange(i.transform.position, Plugin.playerPlateRadiusXZ.Value, Plugin.playerPlateRadiusY.Value));

            if (player != null) {
                lastPlayer = player;
                return true;
            }

            return false;
        }

        private bool InRange(Vector3 target, float rangeXZ, float rangeY) {
            Vector3 delta = transform.position - target;
            bool inXZ = new Vector3(delta.x, 0, delta.z).sqrMagnitude <= rangeXZ * rangeXZ;
            bool inY = Mathf.Abs(delta.y) <= rangeY;
            return inXZ && inY;
        }

        private void OnDestroy() {
            allPlates.Remove(this);
        }
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
