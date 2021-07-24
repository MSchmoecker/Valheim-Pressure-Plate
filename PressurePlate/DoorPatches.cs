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

        private static readonly MethodInfo CheckAccessCall =
            AccessTools.GetDeclaredMethods(typeof(PrivateArea)).First(m => m.Name == "CheckAccess");

        private static readonly MethodInfo GetComponentDoorPowerState =
            AccessTools.Method(typeof(Component), "GetComponent", new Type[0], new[] {typeof(DoorPowerState)});

        private static readonly FieldInfo GetPlateBypassWard =
            AccessTools.Field(typeof(DoorPowerState), nameof(DoorPowerState.bypassWard));

        [HarmonyPatch(typeof(Door), "Interact"), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DoorInteract(IEnumerable<CodeInstruction> instructions) {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            int checkAccessCallIndex = -1;
            Label afterReturnLabel = new Label();

            for (int i = 0; i < code.Count; i++) {
                CodeInstruction instruction = code[i];

                if (instruction.Is(OpCodes.Call, CheckAccessCall)) {
                    checkAccessCallIndex = i;
                }

                if (checkAccessCallIndex > -1 && i == checkAccessCallIndex + 1) {
                    afterReturnLabel = (Label) instruction.operand;
                }
            }

            if (checkAccessCallIndex > -1) {
                Label privateAreaReturnLabel = new Label();
                code[checkAccessCallIndex + 2].WithLabels(privateAreaReturnLabel);

                CodeInstruction[] bypassWard = {
                    // check if ward should be bypassed
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, GetComponentDoorPowerState),
                    new CodeInstruction(OpCodes.Ldfld, GetPlateBypassWard),
                    new CodeInstruction(OpCodes.Brtrue, afterReturnLabel), // skip the return and continue method
                };

                code[checkAccessCallIndex - 6].MoveLabelsTo(bypassWard[0]);

                // bevore CheckAccess is called
                code.InsertRange(checkAccessCallIndex - 6, bypassWard);
            }

            return code.AsEnumerable();
        }
    }

    public class DoorPowerState : MonoBehaviour {
        public static readonly List<DoorPowerState> AllStates = new List<DoorPowerState>();
        public bool bypassWard;

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
            return zNetView != null && zNetView.IsValid();
        }

        public void Open(Humanoid humanoid, Plate plate) {
            if (!IsReallySpawned(out _)) return;

            if (!IsOpen()) {
                bypassWard = plate.zNetView.GetZDO().GetBool("pressure_plate_is_public");
                door.Interact(humanoid, false);
                bypassWard = false;
            }
        }

        public void Close(Humanoid humanoid, Plate plate) {
            if (!IsReallySpawned(out _)) return;

            if (IsOpen()) {
                bypassWard = plate.zNetView.GetZDO().GetBool("pressure_plate_is_public");
                door.Interact(humanoid, false);
                bypassWard = false;
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
