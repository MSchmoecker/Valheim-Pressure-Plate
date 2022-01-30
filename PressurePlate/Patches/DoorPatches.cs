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
            if (!IsReallySpawned(out ZNetView zNetView)) return 0;

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
            return zNetView != null && (bool) zNetView && zNetView.IsValid();
        }

        private void CustomDoorInteract(Plate plate) {
            if (door.m_keyItem) {
                return;
            }

            Vector3 deltaDir = (plate.transform.position - transform.position).normalized;
            bool forward = Vector3.Dot(transform.forward, deltaDir) < 0.0f;
            door.m_nview.InvokeRPC("UseDoor", forward);
        }

        public void Open(Plate plate) {
            if (!IsReallySpawned(out _)) return;

            if (!IsOpen(plate)) {
                CustomDoorInteract(plate);
            }
        }

        public void Close(Plate plate) {
            if (!IsReallySpawned(out _)) return;

            if (IsOpen(plate)) {
                CustomDoorInteract(plate);
            }
        }

        public bool IsOpen(Plate plate) {
            return (plate == null || !plate.Invert.Get()) ? GetState() != 0 : GetState() == 0;
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

        public static List<DoorPowerState> FindDoorsInPlateRange(Plate plate, Vector3 platePos) {
            return AllStates.FindAll(i => i.InRange(platePos, plate.OpenRadiusHorizontal.Get(), plate.OpenRadiusVertical.Get()));
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
