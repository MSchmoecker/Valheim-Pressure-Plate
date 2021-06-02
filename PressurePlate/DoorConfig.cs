using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PressurePlate {
    public class DoorConfig {
        public bool openClosedInverted;
        public float openTime;

        private static readonly Dictionary<string, DoorConfig> specificDoors = new Dictionary<string, DoorConfig>();

        public DoorConfig(bool openClosedInverted = false, float openTime = 1) {
            this.openClosedInverted = openClosedInverted;
            this.openTime = openTime;
        }

        public static bool AddDoorConfig(string pieceName, DoorConfig doorConfig) {
            if (specificDoors.ContainsKey(pieceName)) {
                return false;
            }

            specificDoors.Add(pieceName, doorConfig);
            return true;
        }

        public static DoorConfig GetDoorConfig(string pieceName) {
            if (specificDoors.ContainsKey(pieceName)) {
                return specificDoors[pieceName];
            }

            return null;
        }
    }
}
