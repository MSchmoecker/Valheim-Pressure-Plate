using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace PressurePlate {
    [HarmonyPatch]
    public class MenuPatches {
        [HarmonyPatch(typeof(Menu), "Update"), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> MenuUpdate(IEnumerable<CodeInstruction> instructions) {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);

            MethodInfo minimapIsOpen = AccessTools.GetDeclaredMethods(typeof(Minimap)).First(m => m.Name == nameof(Minimap.IsOpen));
            MethodInfo pressurePlateUIIsOpen = AccessTools.GetDeclaredMethods(typeof(PressurePlateUI))
                                                          .First(m => m.Name == nameof(PressurePlateUI.IsOpen));

            for (int i = 0; i < list.Count; i++) {
                CodeInstruction instruction = list[i];

                if (instruction.Calls(minimapIsOpen)) {
                    yield return LogInst(instruction); // Minimap Is Open Call
                    instruction = list[++i];
                    object label = instruction.operand;
                    yield return LogInst(instruction); // Brtrue_S Call

                    yield return LogInst(new CodeInstruction(OpCodes.Call, pressurePlateUIIsOpen));
                    yield return LogInst(new CodeInstruction(OpCodes.Brtrue, label));
                } else {
                    yield return LogInst(instruction);
                }
            }
        }

        static CodeInstruction LogInst(CodeInstruction instruction) {
            Log.LogInfo(instruction);
            return instruction;
        }
    }
}
