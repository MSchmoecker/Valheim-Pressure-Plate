using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PressurePlate {
    public class DoorConfig {
        public bool openClosedInverted;
        [Obsolete] public float openTime;

        private static readonly Dictionary<int, DoorConfig> specificDoors = new Dictionary<int, DoorConfig>();

        public DoorConfig(bool openClosedInverted = false, float openTime = 1) {
            this.openClosedInverted = openClosedInverted;
        }

        public static bool AddDoorConfig(Door door, DoorConfig doorConfig) {
            if (!door.TryGetComponent(out DoorPowerState doorPowerState)) {
                doorPowerState = door.gameObject.AddComponent<DoorPowerState>();
            }

            if (specificDoors.ContainsKey(doorPowerState.GetPrefabId())) return false;

            specificDoors.Add(doorPowerState.GetPrefabId(), doorConfig);
            return true;
        }

        public static bool AddDoorConfig(string prefabName, DoorConfig doorConfig) {
            int hashCode = prefabName.GetStableHashCode();

            if (specificDoors.ContainsKey(hashCode)) return false;

            specificDoors.Add(hashCode, doorConfig);
            return true;
        }

        public static DoorConfig GetDoorConfig(PowerState doorPowerState) {
            if (specificDoors.ContainsKey(doorPowerState.GetPrefabId())) {
                return specificDoors[doorPowerState.GetPrefabId()];
            }

            return null;
        }
    }
}
