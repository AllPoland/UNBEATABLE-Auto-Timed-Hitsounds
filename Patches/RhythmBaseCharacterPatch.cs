using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FMOD.Studio;
using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

[HarmonyPatch(typeof(RhythmBaseCharacter))]
[HarmonyPatch(nameof(RhythmBaseCharacter.Hold))]
class RhythmBaseCharacter_Mute_Patch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
        for(int i = 0; i < codes.Count; i++)
        {
            // Search for the opcode that calls for EventInstance to play the hold hitsound
            // and replace it with a nop
            if(codes[i].opcode != OpCodes.Call)
            {
                continue;
            }

            MethodInfo targetMethod = typeof(EventInstance).GetMethod("start");
            if(codes[i].Calls(targetMethod))
            {
                // Bye bye hitsound :D
                Plugin.Logger.LogInfo($"Removing instruction: RhythmBaseCharacter.Hold[{i}]");
                codes[i].opcode = OpCodes.Nop;
            }
        }

        return codes.AsEnumerable();
    }
}