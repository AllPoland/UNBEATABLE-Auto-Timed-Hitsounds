using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FMOD.Studio;
using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

class RhythmBaseCharacter_Mute_Patch
{
    [HarmonyPatch(typeof(RhythmBaseCharacter), nameof(RhythmBaseCharacter.Hold))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> HoldTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        List<CodeInstruction> codes = instructions.ToList();
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
                // Plugin.Logger.LogInfo($"Removing instruction: {original.DeclaringType}.{original.Name}[{i}]");
                codes[i].opcode = OpCodes.Nop;
            }
        }

        // Plugin.Logger.LogInfo($"Done patching {original.DeclaringType}.{original.Name}()");
        return codes.AsEnumerable();
    }
}