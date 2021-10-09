using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PressurePlate {
    public class Plate : MonoBehaviour, Hoverable, Interactable {
        public GameObject plate;
        public Piece piece;
        public bool isPressed;
        public Character lastCharacter;
        private float pressCooldown;
        private float pressTriggerDelay; // time before the plate is triggered
        public EffectList pressEffects = new EffectList();
        public EffectList releaseEffects = new EffectList();
        public ZNetView zNetView;
        public string showName = "$pressure_plate";

        public const string KeyTriggerRadiusHorizontal = "pressure_plate_trigger_radius_horizontal";
        public const string KeyTriggerRadiusVertical = "pressure_plate_trigger_radius_vertical";
        public const string KeyOpenRadiusHorizontal = "pressure_plate_open_radius_horizontal";
        public const string KeyOpenRadiusVertical = "pressure_plate_open_radius_vertical";
        public const string KeyOpenTime = "pressure_plate_open_time";
        public const string KeyTriggerDelay = "pressure_plate_trigger_delay";
        public const string KeyInvert = "pressure_plate_invert";
        public const string KeyIgnoreWards = "pressure_plate_is_public";
        public const string KeyAllowMobs = "pressure_plate_allow_mobs";

        private void Awake() {
            zNetView = GetComponent<ZNetView>();
            piece = GetComponent<Piece>();

            if (zNetView.IsValid()) {
                CheckPlayerPress(out isPressed, out _);
                pressTriggerDelay = TriggerDelay;
            }
        }

        private void FixedUpdate() {
            if (!zNetView.IsValid()) {
                return; //wait for network spawn
            }

            bool wasPressed = isPressed;
            CheckPlayerPress(out bool newPressed, out bool hasAccess);
            List<DoorPowerState> doors = DoorPowerState.FindDoorsInPlateRange(this, transform.position);

            if (newPressed) {
                if (pressTriggerDelay <= 0) {
                    pressTriggerDelay = TriggerDelay;
                    isPressed = true;

                    float? maxTime = doors.Max(i => i.GetDoorConfig()?.openTime);
                    pressCooldown = maxTime ?? OpenTime;
                } else {
                    pressTriggerDelay -= Time.fixedDeltaTime;
                }
            } else {
                if (pressCooldown > 0) {
                    pressCooldown -= Time.fixedDeltaTime;
                    isPressed = true;
                } else {
                    isPressed = false;
                    pressTriggerDelay = TriggerDelay;
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

            // TODO: ownership
            if (lastCharacter == null) return;
            if (!hasAccess) return;
            if (!stateChange && !isPressed) return;

            foreach (DoorPowerState door in doors) {
                if (isPressed) {
                    // always open the door if the plate is pressed
                    door.Open(lastCharacter, this);
                } else {
                    // only close the door if this is the last plate powering it
                    if (door.GetPoweringPlates().Count(i => i != this) > 0) continue;

                    door.Close(lastCharacter, this);
                }
            }
        }

        private void CheckPlayerPress(out bool pressed, out bool hasAccess) {
            pressed = false;

            if (Player.m_localPlayer == null) {
                lastCharacter = null;
                hasAccess = false;
                return;
            }

            if (AllowMobs) {
                foreach (Character character in Character.GetAllCharacters()) {
                    if (InRange(character.transform.position)) {
                        lastCharacter = character;
                        pressed = true;
                        break;
                    }
                }
            } else {
                foreach (Player player in Player.GetAllPlayers()) {
                    if (InRange(player.transform.position)) {
                        lastCharacter = player;
                        pressed = true;
                        break;
                    }
                }
            }

            hasAccess = PrivateArea.CheckAccess(transform.position, 0.0f, false) || IgnoreWards;
        }

        private bool InRange(Vector3 target) {
            float rangeXZ = TriggerRadiusHorizontal;
            float rangeY = TriggerRadiusVertical;
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

            text += piece.m_name + "\n";

            if (InsidePrivateArea()) {
                bool hasAccess = PrivateArea.CheckAccess(transform.position, 0f, false);

                if (!hasAccess) {
                    text += "$piece_noaccess";
                    return Localization.instance.Localize(text);
                }
            }

            text += "[<color=yellow><b>$KEY_Use</b></color>] $piece_use";
            return Localization.instance.Localize(text);
        }

        public string GetHoverName() {
            return Localization.instance.Localize(showName);
        }

        public bool Interact(Humanoid user, bool hold, bool alt) {
            if (hold) {
                return false;
            }

            if (!PrivateArea.CheckAccess(transform.position)) {
                return true;
            }

            if (!PressurePlateUI.IsOpen() && !PressurePlateUI.instance.IsFrameBlocked) {
                PressurePlateUI.instance.OpenUI(this);
            }

            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item) {
            return false;
        }

        public float TriggerRadiusHorizontal {
            get => zNetView.GetZDO().GetFloat(KeyTriggerRadiusHorizontal, 1f);
            set {
                if (!zNetView.IsOwner()) zNetView.ClaimOwnership();
                zNetView.GetZDO().Set(KeyTriggerRadiusHorizontal, value);
            }
        }

        public float TriggerRadiusVertical {
            get => zNetView.GetZDO().GetFloat(KeyTriggerRadiusVertical, 1f);
            set {
                if (!zNetView.IsOwner()) zNetView.ClaimOwnership();
                zNetView.GetZDO().Set(KeyTriggerRadiusVertical, value);
            }
        }

        public float OpenRadiusHorizontal {
            get => zNetView.GetZDO().GetFloat(KeyOpenRadiusHorizontal, 3f);
            set {
                if (!zNetView.IsOwner()) zNetView.ClaimOwnership();
                zNetView.GetZDO().Set(KeyOpenRadiusHorizontal, value);
            }
        }

        public float OpenRadiusVertical {
            get => zNetView.GetZDO().GetFloat(KeyOpenRadiusVertical, 3f);
            set {
                if (!zNetView.IsOwner()) zNetView.ClaimOwnership();
                zNetView.GetZDO().Set(KeyOpenRadiusVertical, value);
            }
        }

        public float OpenTime {
            get => zNetView.GetZDO().GetFloat(KeyOpenTime, 1f);
            set {
                if (!zNetView.IsOwner()) zNetView.ClaimOwnership();
                zNetView.GetZDO().Set(KeyOpenTime, value);
            }
        }

        public float TriggerDelay {
            get => zNetView.GetZDO().GetFloat(KeyTriggerDelay, 0f);
            set {
                if (!zNetView.IsOwner()) zNetView.ClaimOwnership();
                zNetView.GetZDO().Set(KeyTriggerDelay, value);
            }
        }

        public bool Invert {
            get => zNetView.GetZDO().GetBool(KeyInvert, false);
            set {
                if (!zNetView.IsOwner()) zNetView.ClaimOwnership();
                zNetView.GetZDO().Set(KeyInvert, value);
            }
        }

        public bool IgnoreWards {
            get => zNetView.GetZDO().GetBool(KeyIgnoreWards, false);
            set {
                if (!zNetView.IsOwner()) zNetView.ClaimOwnership();
                zNetView.GetZDO().Set(KeyIgnoreWards, value);
            }
        }

        public bool AllowMobs {
            get => zNetView.GetZDO().GetBool(KeyAllowMobs, false);
            set {
                if (!zNetView.IsOwner()) zNetView.ClaimOwnership();
                zNetView.GetZDO().Set(KeyAllowMobs, value);
            }
        }

        public void ResetValues() {
            if (!zNetView.IsOwner()) {
                zNetView.ClaimOwnership();
            }

            zNetView.GetZDO().ReleaseFloats();
            zNetView.GetZDO().ReleaseVec3();
            zNetView.GetZDO().ReleaseQuats();
            zNetView.GetZDO().ReleaseInts();
            zNetView.GetZDO().ReleaseLongs();
            zNetView.GetZDO().ReleaseStrings();

            zNetView.GetZDO().IncreseDataRevision();
        }
    }
}
