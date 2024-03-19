using System;
using System.Collections.Generic;
using UnityEngine;

namespace PressurePlate {
    public abstract class PowerState : MonoBehaviour {
        public static readonly List<PowerState> AllStates = new List<PowerState>();

        private List<Plate> poweringPlates = new List<Plate>();
        private int prefabId = -1;

        protected virtual void Awake() {
            AllStates.Add(this);
        }

        protected virtual void OnDestroy() {
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

        protected abstract void Interact(Plate plate);

        public abstract bool CanInteract(Player activator, ZDO targetArea);

        public virtual void Open(Plate plate) {
            if (!IsReallySpawned(out _)) return;

            if (!IsOpen(plate)) {
                Interact(plate);
            }
        }

        public virtual void Close(Plate plate) {
            if (!IsReallySpawned(out _)) return;

            if (IsOpen(plate)) {
                Interact(plate);
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

        public static List<PowerState> FindDoorsInPlateRange(Plate plate, Vector3 platePos) {
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
