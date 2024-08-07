﻿using System;
using System.Collections.Generic;
using System.Linq;
using PressurePlate.Compatibility.WardIsLove;
using UnityEngine;

namespace PressurePlate {
    public class Plate : MonoBehaviour, Hoverable, Interactable {
        [SerializeField] private GameObject plate;
        [SerializeField] private EffectList pressEffects = new EffectList();
        [SerializeField] private EffectList releaseEffects = new EffectList();

        private Piece piece;
        private MeshRenderer pieceMesh;
        private bool isPressed;
        private float pressCooldown;
        private float pressTriggerDelay; // time before the plate is triggered
        private float lastTime;
        private ZNetView zNetView;

        public FloatZNetProperty TriggerRadiusHorizontal { get; private set; }
        public FloatZNetProperty TriggerRadiusVertical { get; private set; }
        public FloatZNetProperty OpenRadiusHorizontal { get; private set; }
        public FloatZNetProperty OpenRadiusVertical { get; private set; }
        public FloatZNetProperty OpenTime { get; private set; }
        public FloatZNetProperty TriggerDelay { get; private set; }
        public BoolZNetProperty Invert { get; private set; }
        public BoolZNetProperty ToggleModeOption { get; private set; }
        public BoolZNetProperty IsToggledOn { get; private set; }
        public BoolZNetProperty IgnoreWards { get; private set; }
        public BoolZNetProperty AllowMobs { get; private set; }
        public IntZNetProperty MobTameInteraction { get; private set; }
        public BoolZNetProperty IsInvisible { get; private set; }
        public BoolZNetProperty OnlyOpenNotPermitted { get; private set; }

        private void InitProperties() {
            TriggerRadiusHorizontal = new FloatZNetProperty("pressure_plate_trigger_radius_horizontal", zNetView, 1f);
            TriggerRadiusVertical = new FloatZNetProperty("pressure_plate_trigger_radius_vertical", zNetView, 1f);
            OpenRadiusHorizontal = new FloatZNetProperty("pressure_plate_open_radius_horizontal", zNetView, 3f);
            OpenRadiusVertical = new FloatZNetProperty("pressure_plate_open_radius_vertical", zNetView, 3f);
            OpenTime = new FloatZNetProperty("pressure_plate_open_time", zNetView, 1f);
            TriggerDelay = new FloatZNetProperty("pressure_plate_trigger_delay", zNetView, 0f);
            Invert = new BoolZNetProperty("pressure_plate_invert", zNetView, false);
            ToggleModeOption = new BoolZNetProperty("pressure_plate_toggle_mode", zNetView, false);
            IsToggledOn = new BoolZNetProperty("pressure_plate_is_toggled_on", zNetView, false);
            IgnoreWards = new BoolZNetProperty("pressure_plate_is_public", zNetView, false);
            AllowMobs = new BoolZNetProperty("pressure_plate_allow_mobs", zNetView, false);
            MobTameInteraction = new IntZNetProperty("pressure_plate_tame_interaction", zNetView, (int)TameInteraction.All);
            IsInvisible = new BoolZNetProperty("pressure_plate_invisible", zNetView, false);
            OnlyOpenNotPermitted = new BoolZNetProperty("pressure_plate_only_open_not_permitted", zNetView, false);
        }

        public void PasteData(Plate copy) {
            TriggerRadiusHorizontal.ForceSet(copy.TriggerRadiusHorizontal.Get());
            TriggerRadiusVertical.ForceSet(copy.TriggerRadiusVertical.Get());
            OpenRadiusHorizontal.ForceSet(copy.OpenRadiusHorizontal.Get());
            OpenRadiusVertical.ForceSet(copy.OpenRadiusVertical.Get());
            OpenTime.ForceSet(copy.OpenTime.Get());
            TriggerDelay.ForceSet(copy.TriggerDelay.Get());
            Invert.ForceSet(copy.Invert.Get());
            ToggleModeOption.ForceSet(copy.ToggleModeOption.Get());
            IgnoreWards.ForceSet(copy.IgnoreWards.Get());
            AllowMobs.ForceSet(copy.AllowMobs.Get());
            MobTameInteraction.ForceSet(copy.MobTameInteraction.Get());
            IsInvisible.ForceSet(copy.IsInvisible.Get());
            OnlyOpenNotPermitted.ForceSet(copy.OnlyOpenNotPermitted.Get());
        }

        private void Awake() {
            zNetView = GetComponent<ZNetView>();
            piece = GetComponent<Piece>();
            pieceMesh = plate.GetComponent<MeshRenderer>();

            Plugin.Instance.plateVolume.SettingChanged += SetPlateVolume;
            SetPlateVolume(null, null);

            if (zNetView.IsValid()) {
                InitProperties();
                isPressed = CheckPress(out _);
                pressTriggerDelay = TriggerDelay.Get();
                lastTime = Time.time;
                FixedUpdate();
            }
        }

        private void UpdateVisibility() {
            pieceMesh.enabled = !IsInvisible.Get();
        }

        private void FixedUpdate() {
            if (!zNetView.IsValid()) {
                return; //wait for network spawn
            }

            UpdateVisibility();

            float timeDelta = Time.time - lastTime;
            lastTime = Time.time;

            bool wasPressed = isPressed;
            bool newPressed = CheckPress(out Player activator);
            ZDO targetArea = GetArea(transform.position);
            List<PowerState> doors = PowerState.FindDoorsInPlateRange(this, transform.position);

            if (newPressed) {
                if (pressTriggerDelay <= 0) {
                    pressTriggerDelay = TriggerDelay.Get();
                    isPressed = true;

                    pressCooldown = Mathf.Max(OpenTime.Get());
                } else {
                    pressTriggerDelay -= timeDelta;
                }
            } else {
                if (pressCooldown > 0) {
                    pressCooldown -= timeDelta;
                    isPressed = true;
                } else {
                    isPressed = false;
                    pressTriggerDelay = TriggerDelay.Get();
                }
            }

            bool stateChange = isPressed != wasPressed;

            Vector3 pos = isPressed ? new Vector3(0f, -0.025f, 0f) : new Vector3(0f, 0.05f, 0f);
            plate.transform.localPosition = pos;

            if (zNetView.IsOwner()) {
                if (ToggleModeOption.Get()) {
                    if (stateChange && isPressed) {
                        IsToggledOn.Set(!IsToggledOn.Get());
                    }
                } else {
                    IsToggledOn.Set(false);
                }
            }

            bool isPowering = IsPowering();

            if (stateChange) {
                if (isPowering) {
                    pressEffects.Create(transform.position, Quaternion.identity);
                } else {
                    releaseEffects.Create(transform.position, Quaternion.identity);
                }

                foreach (PowerState door in doors) {
                    if (!door.CanInteract(activator, targetArea)) {
                        continue;
                    }

                    if (isPowering) {
                        door.AddPoweringPlate(this);
                    } else {
                        door.RemovePoweringPlate(this);
                    }
                }
            }

            if (!zNetView.IsOwner()) return;
            if (!stateChange && !isPowering) return;

            foreach (PowerState door in doors) {
                if (!door.CanInteract(activator, targetArea)) {
                    continue;
                }

                if (isPowering) {
                    // always open the door if the plate is pressed
                    door.Open(this);
                } else {
                    // only close the door if this is the last plate powering it
                    if (door.GetPoweringPlates().Count(i => i != this) > 0) continue;

                    door.Close(this);
                }
            }
        }

        private bool IsPowering() {
            if (ToggleModeOption.Get()) {
                return IsToggledOn.Get();
            }

            return isPressed;
        }

        public static bool CanInteractWithDoor(Player activator, DoorPowerState door, ZDO targetArea) {
            if (activator != null && PlayerHasAccess(activator, door.transform.position)) {
                return true;
            }

            return targetArea == GetArea(door.transform.position);
        }

        private bool CheckPress(out Player activator) {
            if (IgnoreWards.Get() && AllowMobs.Get()) {
                activator = null;
                return CheckMobPress();
            }

            activator = CheckPlayerPress();
            return activator != null;
        }

        private Player CheckPlayerPress() {
            foreach (Player player in Player.GetAllPlayers()) {
                if (!InRange(player.transform.position)) {
                    continue;
                }

                if (NotOpenBecauseIsPermitted(player)) {
                    continue;
                }

                if (IgnoreWards.Get() || PlayerHasAccess(player, player.transform.position)) {
                    return player;
                }
            }

            return null;
        }

        private bool NotOpenBecauseIsPermitted(Player player) {
            return IgnoreWards.Get() && OnlyOpenNotPermitted.Get() && PlayerHasAccess(player, player.transform.position);
        }

        private bool CheckMobPress() {
            foreach (Character character in Character.GetAllCharacters()) {
                if (!InRange(character.transform.position)) {
                    continue;
                }

                if (character is Player player && NotOpenBecauseIsPermitted(player)) {
                    continue;
                }

                switch (MobTameInteraction.Get()) {
                    case (int)TameInteraction.All: {
                        return true;
                    }

                    case (int)TameInteraction.OnlyTames: {
                        if (character.IsTamed()) {
                            return true;
                        }

                        continue;
                    }

                    case (int)TameInteraction.OnlyNonTames: {
                        if (!character.IsTamed()) {
                            return true;
                        }

                        continue;
                    }
                }

                return true;
            }

            return false;
        }

        public static bool PlayerHasAccess(Player player, Vector3 pos) {
            if (WardIsLovePlugin.IsLoaded() && DoorInteract.InsideWard(pos)) {
                return DoorInteract.CanInteract(player, pos);
            }

            foreach (PrivateArea allArea in PrivateArea.m_allAreas) {
                bool areaEnabled = allArea.IsEnabled();
                bool inside = allArea.IsInside(pos, 0.0f);
                bool playerPermitted = allArea.m_piece.GetCreator() == player.GetPlayerID() || allArea.IsPermitted(player.GetPlayerID());

                if (areaEnabled && inside && !playerPermitted) {
                    return false;
                }
            }

            return true;
        }

        public static ZDO GetArea(Vector3 pos) {
            if (WardIsLovePlugin.IsLoaded() && DoorInteract.InsideWard(pos)) {
                return WardMonoscriptExt.GetWardMonoscript(pos).GetZDO();
            }

            foreach (PrivateArea allArea in PrivateArea.m_allAreas) {
                if (allArea.IsEnabled() && allArea.IsInside(pos, 0.0f)) {
                    return allArea.m_nview.GetZDO();
                }
            }

            return null;
        }

        private bool InRange(Vector3 target) {
            float rangeXZ = TriggerRadiusHorizontal.Get();
            float rangeY = TriggerRadiusVertical.Get();
            Vector3 delta = transform.position - target;
            bool inXZ = new Vector3(delta.x, 0, delta.z).sqrMagnitude <= rangeXZ * rangeXZ;
            bool inY = Mathf.Abs(delta.y) <= rangeY;
            return inXZ && inY;
        }

        private void SetPlateVolume(object sender, EventArgs args) {
            foreach (EffectList.EffectData effectData in pressEffects.m_effectPrefabs) {
                if (effectData.m_prefab.TryGetComponent(out ZSFX zsfx)) {
                    float volume = 0.8f * (Plugin.Instance.plateVolume.Value / 100f);
                    zsfx.m_minVol = volume;
                    zsfx.m_maxVol = volume;
                }
            }

            foreach (EffectList.EffectData effectData in releaseEffects.m_effectPrefabs) {
                if (effectData.m_prefab.TryGetComponent(out ZSFX zsfx)) {
                    float volume = 0.9f * (Plugin.Instance.plateVolume.Value / 100f);
                    zsfx.m_minVol = volume;
                    zsfx.m_maxVol = volume;
                }
            }
        }

        public string GetHoverText() {
            if (!zNetView.IsValid()) {
                return string.Empty;
            }

            if (!PlayerHasAccess(Player.m_localPlayer, Player.m_localPlayer.transform.position)) {
                return Localization.instance.Localize($"{GetHoverName()}\n$piece_noaccess");
            }

            string text = string.Empty;

            if (IsPowering()) {
                text += $"{GetHoverName()}: $pressure_plate_powering_on\n";
            } else {
                text += $"{GetHoverName()}: $pressure_plate_powering_off\n";
            }

            text += "[<color=yellow><b>$KEY_Use</b></color>] $piece_use";
            return Localization.instance.Localize(text);
        }

        public string GetHoverName() {
            return Localization.instance.Localize(piece.m_name);
        }

        public bool Interact(Humanoid user, bool hold, bool alt) {
            if (hold) {
                return false;
            }

            if (!PlayerHasAccess(Player.m_localPlayer, Player.m_localPlayer.transform.position)) {
                return true;
            }

            if (!PressurePlateUI.IsOpen && !PressurePlateUI.instance.IsFrameBlocked) {
                PressurePlateUI.instance.OpenUI(this);
            }

            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item) {
            return false;
        }

        public void ResetValues() {
            if (!zNetView.IsOwner()) {
                zNetView.ClaimOwnership();
            }

            ZDOExtraData.ReleaseFloats(zNetView.GetZDO().m_uid);
            ZDOExtraData.ReleaseVec3(zNetView.GetZDO().m_uid);
            ZDOExtraData.ReleaseQuats(zNetView.GetZDO().m_uid);
            ZDOExtraData.ReleaseInts(zNetView.GetZDO().m_uid);
            ZDOExtraData.ReleaseLongs(zNetView.GetZDO().m_uid);
            ZDOExtraData.ReleaseStrings(zNetView.GetZDO().m_uid);

            zNetView.GetZDO().IncreaseDataRevision();
        }

        private void OnDestroy() {
            Plugin.Instance.plateVolume.SettingChanged -= SetPlateVolume;
        }
    }
}
