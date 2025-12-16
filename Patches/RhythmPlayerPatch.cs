using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FMOD.Studio;
using FMODUnity;
using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

[HarmonyPatch(typeof(RhythmPlayer))]
[HarmonyPatch(nameof(RhythmPlayer.StopHold))]
class RhythmPlayer_Mute_Patch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
        for(int i = 0; i < codes.Count; i++)
        {
            // Search for the opcodes that stop and release the hold sound events and replace it with a nop
            // This isn't strictly necessary but it avoids any potential jank since these events should never start in the first place
            if(codes[i].opcode != OpCodes.Call)
            {
                continue;
            }

            MethodInfo targetMethod = typeof(EventInstance).GetMethod("stop");
            if(codes[i].Calls(targetMethod))
            {
                Plugin.Logger.LogInfo($"Removing instruction: RhythmPlayer.StopHold[{i}]");
                codes[i].opcode = OpCodes.Nop;
                continue;
            }

            targetMethod = typeof(EventInstance).GetMethod("release");
            if(codes[i].Calls(targetMethod))
            {
                Plugin.Logger.LogInfo($"Removing instruction: RhythmPlayer.StopHold[{i}]");
                codes[i].opcode = OpCodes.Nop;
                continue;
            }

            targetMethod = typeof(RuntimeManager).GetMethod("CreateInstance", [typeof(EventReference)]);
            if(codes[i].Calls(targetMethod))
            {
                Plugin.Logger.LogInfo($"Removing instruction: RhythmPlayer.StopHold[{i}]");
                codes[i].opcode = OpCodes.Nop;
            }
        }

        return codes.AsEnumerable();
    }
}