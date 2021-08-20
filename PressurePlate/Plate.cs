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

            float currentTriggerDelay = zNetView.GetZDO().GetFloat("pressure_plate_trigger_delay");

            // not the cleanest way to handle input here but this way it only listens when the player hovers over the plate
            if (Plugin.addTimeKey.Value.IsDown()) {
                currentTriggerDelay += 0.25f;
                zNetView.GetZDO().Set("pressure_plate_trigger_delay", currentTriggerDelay);
                pressTriggerDelay = currentTriggerDelay;
            }

            if (Plugin.subtractTimeKey.Value.IsDown()) {
                currentTriggerDelay = Mathf.Max(0, currentTriggerDelay - .25f);
                zNetView.GetZDO().Set("pressure_plate_trigger_delay", currentTriggerDelay);
                pressTriggerDelay = currentTriggerDelay;
            }

            if (InsidePrivateArea()) {
                bool hasAccess = PrivateArea.CheckAccess(transform.position, 0f, false);
                bool plateIsPublic = zNetView.GetZDO().GetBool("pressure_plate_is_public");

                if (plateIsPublic) {
                    text += showName + " ($public)\n";
                } else {
                    text += showName + " ($private)\n";
                }

                if (!hasAccess) {
                    text += "$piece_noaccess";
                    return Localization.instance.Localize(text);
                }

                if (plateIsPublic) {
                    text += "$pressure_plate_public_text\n";
                    text += "[<color=yellow><b>$KEY_Use</b></color>] $pressure_plate_set_private\n";
                } else {
                    text += "$pressure_plate_private_text\n";
                    text += "[<color=yellow><b>$KEY_Use</b></color>] $pressure_plate_set_public\n";
                }
            }

            if (currentTriggerDelay == 0) {
                text += "$pressure_plate_trigger_delay: $pressure_plate_trigger_delay_off\n";
            } else {
                text += $"$pressure_plate_trigger_delay: {currentTriggerDelay}$pressure_plate_seconds_short\n";
            }

            text += $"[<color=yellow><b>{Plugin.addTimeKey.Value.ToString()}</b></color>]/";
            text += $"[<color=yellow><b>{Plugin.subtractTimeKey.Value.ToString()}</b></color>] ";
            text += "$pressure_plate_trigger_delay_description\n";

            return Localization.instance.Localize(text);
        }

        public string GetHoverName() {
            return Localization.instance.Localize(showName);
        }

        public bool Interact(Humanoid user, bool hold) {
            if (!InsidePrivateArea()) {
                return false;
            }

            if (hold) {
                return false;
            }

            if (!PrivateArea.CheckAccess(transform.position)) {
                return true;
            }

            bool plateIsPublic = zNetView.GetZDO().GetBool("pressure_plate_is_public");
            zNetView.GetZDO().Set("pressure_plate_is_public", !plateIsPublic);
            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item) {
            return false;
        }
    }
}
