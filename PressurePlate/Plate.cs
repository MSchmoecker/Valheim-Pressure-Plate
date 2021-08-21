using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PressurePlate {
    public class Plate : MonoBehaviour, Hoverable, Interactable {
        public GameObject plate;
        public bool isPressed;
        public Player lastPlayer;
        private float pressCooldown;
        private float pressTriggerDelay; // time before the plate is triggered
        public EffectList pressEffects = new EffectList();
        public EffectList releaseEffects = new EffectList();
        public ZNetView zNetView;
        public string showName = "$pressure_plate";

        private void Awake() {
            CheckPlayerPress(out isPressed, out _);
            zNetView = GetComponent<ZNetView>();
            pressTriggerDelay = zNetView.IsValid() ? zNetView.GetZDO().GetFloat("pressure_plate_trigger_delay") : 0;
        }

        private void FixedUpdate() {
            if (!zNetView.IsValid()) {
                return; //wait for network spawn
            }

            bool wasPressed = isPressed;
            CheckPlayerPress(out bool newPressed, out bool hasAccess);
            List<DoorPowerState> doors = DoorPowerState.FindDoorsInPlateRange(transform.position);

            if (newPressed) {
                float? maxTime = doors.Max(i => i.GetDoorConfig()?.openTime);
                pressCooldown = maxTime ?? Plugin.plateOpenDelay.Value;

                if (pressTriggerDelay <= 0) {
                    pressTriggerDelay = zNetView.GetZDO().GetFloat("pressure_plate_trigger_delay");
                    isPressed = true;
                } else {
                    pressTriggerDelay -= Time.fixedDeltaTime;
                }
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

                if (hasAccess) {
                    foreach (DoorPowerState door in doors) {
                        if (isPressed) {
                            door.AddPoweringPlate(this);
                        } else {
                            door.RemovePoweringPlate(this);
                        }
                    }
                }
            }

            if (lastPlayer == null) return;
            if (lastPlayer != Player.m_localPlayer) return;
            if (!hasAccess) return;
            if (!stateChange && !isPressed) return;

            foreach (DoorPowerState door in doors) {
                if (isPressed) {
                    // always open the door if the plate is pressed
                    door.Open(lastPlayer, this);
                } else {
                    // only close the door if this is the last plate powering it
                    if (door.GetPoweringPlates().Count(i => i != this) > 0) continue;

                    door.Close(lastPlayer, this);
                }
            }
        }

        private void CheckPlayerPress(out bool pressed, out bool hasAccess) {
            pressed = false;

            if (Player.m_localPlayer == null) {
                lastPlayer = null;
                hasAccess = false;
            }

            foreach (Player player in Player.GetAllPlayers()) {
                if (InRange(player.transform.position, Plugin.playerPlateRadiusXZ.Value, Plugin.playerPlateRadiusY.Value)) {
                    lastPlayer = player;
                    pressed = true;
                    break;
                }
            }

            hasAccess = PrivateArea.CheckAccess(transform.position, 0.0f, false) || zNetView.GetZDO().GetBool("pressure_plate_is_public");
        }

        private bool InRange(Vector3 target, float rangeXZ, float rangeY) {
            Vector3 delta = transform.position - target;
            bool inXZ = new Vector3(delta.x, 0, delta.z).sqrMagnitude <= rangeXZ * rangeXZ;
            bool inY = Mathf.Abs(delta.y) <= rangeY;
            return inXZ && inY;
        }

        private bool InsidePrivateArea() {
            // only show interaction when inside active ward
            return zNetView.IsValid() && PrivateArea.CheckInPrivateArea(transform.position);
        }

        public string GetHoverText() {
            string text = "";

            if (InsidePrivateArea()) {
                bool hasAccess = PrivateArea.CheckAccess(transform.position, 0f, false);

                if (!hasAccess) {
                    text += "$piece_noaccess";
                    return Localization.instance.Localize(text);
                }
            }

            text += "[<color=yellow><b>$KEY_Use</b></color>]";
            return Localization.instance.Localize(text);
        }

        public string GetHoverName() {
            return Localization.instance.Localize(showName);
        }

        public bool Interact(Humanoid user, bool hold) {
            if (hold) {
                return false;
            }

            if (!PrivateArea.CheckAccess(transform.position)) {
                return true;
            }

            if (!PressurePlateUI.instance.IsOpen && !PressurePlateUI.instance.IsFrameBlocked) {
                PressurePlateUI.instance.OpenUI(this);
            }

            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item) {
            return false;
        }
    }
}
