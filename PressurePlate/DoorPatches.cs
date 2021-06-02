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
        private ZNetView zNetView;
        private Door door;

        private void Awake() {
            AllStates.Add(this);
            zNetView = GetComponent<ZNetView>();
            door = GetComponent<Door>();
        }

        private void OnDestroy() {
            AllStates.Remove(this);
        }

        public DoorConfig GetDoorConfig() {
            return DoorConfig.GetDoorConfig(gameObject.name);
        }

        public int GetState() {
            int state = zNetView.GetZDO().GetInt("state");

            if (GetDoorConfig() == null) {
                return state;
            }

            DoorConfig doorConfig = GetDoorConfig();

            if (doorConfig.openClosedInverted) {
                state = 1 - Math.Abs(state);
            }

            return state;
        }

        public void Open(Humanoid humanoid) {
            if (!IsOpen()) {
                door.Interact(humanoid, false);
            }
        }

        public void Close(Humanoid humanoid) {
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
    }
}
