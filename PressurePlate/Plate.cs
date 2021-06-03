using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PressurePlate {
    public class Plate : MonoBehaviour {
        public GameObject plate;
        public static List<Plate> allPlates = new List<Plate>();
        public bool isPressed;
        public Player lastPlayer;
        private float pressCooldown;
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
                List<DoorPowerState> doors = DoorPowerState.FindDoorsInPlateRange(transform.position);

                float? maxTime = doors.Max(i => i.GetDoorConfig()?.openTime);
                pressCooldown = maxTime ?? Plugin.plateOpenDelay.Value;

                isPressed = true;
            } else {
                if (pressCooldown > 0) {
                    pressCooldown -= Time.fixedDeltaTime;
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

            if (stateChange) {
                List<DoorPowerState> doors = DoorPowerState.FindDoorsInPlateRange(transform.position);

                foreach (DoorPowerState door in doors) {
                    if (isPressed) {
                        door.AddPoweringPlate(this);
                    } else {
                        door.RemovePoweringPlate(this);
                    }

                    if (door.GetPoweringPlates().Count(i => i != this) > 0) continue;
                    if (lastPlayer == null) continue;

                    if (isPressed) {
                        door.Open(lastPlayer);
                    } else {
                        door.Close(lastPlayer);
                    }
                }
            }
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
}
