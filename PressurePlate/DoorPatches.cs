using System;
using System.Collections.Generic;
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

    public class DoorPowerState : MonoBehaviour {
        public static readonly List<DoorPowerState> AllStates = new List<DoorPowerState>();

        private List<Plate> poweringPlates = new List<Plate>();
        private Door door;
        private int prefabId = -1;

        private void Awake() {
            AllStates.Add(this);
            door = GetComponent<Door>();
        }

        private void OnDestroy() {
            AllStates.Remove(this);
        }

        public DoorConfig GetDoorConfig() {
            return DoorConfig.GetDoorConfig(this);
        }

        public int GetState() {
            if(!IsReallySpawned(out ZNetView zNetView)) return 0;

            int state = zNetView.GetZDO().GetInt("state");
            DoorConfig doorConfig = GetDoorConfig();

            if (doorConfig == null) return state;

            if (doorConfig.openClosedInverted) {
                state = 1 - Math.Abs(state);
            }

            return state;
        }

        public bool IsReallySpawned(out ZNetView zNetView) {
            // If the player places a new door prefab, the door exists but no ZNetView
            zNetView = GetComponent<ZNetView>();
            return zNetView != null;
        }

        public void Open(Humanoid humanoid) {
            if(!IsReallySpawned(out _)) return;

            if (!IsOpen()) {
                door.Interact(humanoid, false);
            }
        }

        public void Close(Humanoid humanoid) {
            if(!IsReallySpawned(out _)) return;

            if (IsOpen()) {
                door.Interact(humanoid, false);
            }
        }

        public bool IsOpen() {
            return GetState() != 0;
        }

        public List<Plate> GetPoweringPlates() {
            poweringPlates.RemoveAll(i => i == null);
            return poweringPlates;
        }

        public void AddPoweringPlate(Plate plate) {
            if (!GetPoweringPlates().Contains(plate)) {
                poweringPlates.Add(plate);
            }
        }

        public void RemovePoweringPlate(Plate plate) {
            if (GetPoweringPlates().Contains(plate)) {
                poweringPlates.Remove(plate);
            }
        }

        public static List<DoorPowerState> FindDoorsInPlateRange(Vector3 platePos) {
            return AllStates.FindAll(i => i.InRange(platePos, Plugin.plateRadiusXZ.Value, Plugin.plateRadiusY.Value));
        }

        private bool InRange(Vector3 target, float rangeXZ, float rangeY) {
            Vector3 delta = transform.position - target;
            bool inXZ = new Vector3(delta.x, 0, delta.z).sqrMagnitude <= rangeXZ * rangeXZ;
            bool inY = Mathf.Abs(delta.y) <= rangeY;
            return inXZ && inY;
        }

        public int GetPrefabId() {
            if (prefabId == -1) {
                prefabId = Utils.GetPrefabName(gameObject).GetStableHashCode();
            }

            return prefabId;
        }
    }
}
