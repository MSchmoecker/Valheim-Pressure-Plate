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
        public EffectList pressEffects = new EffectList();
        public EffectList releaseEffects = new EffectList();
        public ZNetView zNetView;
        public string showName = "$pressure_plate";

        private void Awake() {
            CheckPlayerPress(out isPressed);
            zNetView = GetComponent<ZNetView>();
        }

        private void FixedUpdate() {
            if (!zNetView.IsValid()) {
                return; //wait for network spawn
            }

            bool wasPressed = isPressed;
            bool hasAccess = CheckPlayerPress(out bool newPressed);
            List<DoorPowerState> doors = DoorPowerState.FindDoorsInPlateRange(transform.position);

            if (newPressed) {
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

        private bool CheckPlayerPress(out bool pressed) {
            pressed = false;

            if (Player.m_localPlayer == null) {
                lastPlayer = null;
                return false;
            }

            foreach (Player player in Player.GetAllPlayers()) {
                if (InRange(player.transform.position, Plugin.playerPlateRadiusXZ.Value, Plugin.playerPlateRadiusY.Value)) {
                    lastPlayer = player;
                    pressed = true;
                    break;
                }
            }

            return PrivateArea.CheckAccess(transform.position, 0.0f, false) || zNetView.GetZDO().GetBool("pressure_plate_is_public");
        }

        private bool InRange(Vector3 target, float rangeXZ, float rangeY) {
            Vector3 delta = transform.position - target;
            bool inXZ = new Vector3(delta.x, 0, delta.z).sqrMagnitude <= rangeXZ * rangeXZ;
            bool inY = Mathf.Abs(delta.y) <= rangeY;
            return inXZ && inY;
        }

        public string GetHoverText() {
            if (!zNetView.IsValid()) return "";

            bool hasAccess = PrivateArea.CheckAccess(transform.position, 0f, false);
            bool plateIsPublic = zNetView.GetZDO().GetBool("pressure_plate_is_public");

            string text = "";

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
                text += "[<color=yellow><b>$KEY_Use</b></color>] $pressure_plate_set_private";
            } else {
                text += "$pressure_plate_private_text\n";
                text += "[<color=yellow><b>$KEY_Use</b></color>] $pressure_plate_set_public";
            }

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

            bool plateIsPublic = zNetView.GetZDO().GetBool("pressure_plate_is_public");
            zNetView.GetZDO().Set("pressure_plate_is_public", !plateIsPublic);
            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item) {
            return false;
        }
    }
}
