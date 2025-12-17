using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FMODUnity;
using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

class RhythmController_Mute_Patch
{
    // static IEnumerable<MethodBase> TargetMethods()
    // {
    //     MethodInfo[] allMethods = typeof(RhythmController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

    //     foreach(MethodInfo info in allMethods)
    //     {
    //         Plugin.Logger.LogInfo($"Patching: {info.Attributes} {info.ReturnType} {info.DeclaringType}.{info.Name}({info.GetParameters().Join()}) {info.CallingConvention}");
    //         if(info.Name == nameof(RhythmController.Hit) || info.Name == nameof(RhythmController.Dodge))
    //         {
    //             yield return info;
    //         }
    //     }
    // }


    // [HarmonyPatch(typeof(RhythmController), nameof(RhythmController.Dodge))]
    // [HarmonyTranspiler]
    // static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
    // {
    //     List<CodeInstruction> codes = instructions.ToList();
    //     for(int i = 0; i < codes.Count; i++)
    //     {
    //         // Search for any opcode that calls for RuntimeManager.PlayOneShot()
    //         // and replace it with a nop
    //         if(codes[i].opcode != OpCodes.Call)
    //         {
    //             continue;
    //         }

    //         MethodInfo targetMethod = typeof(RuntimeManager).GetMethod("PlayOneShot");
    //         if(codes[i].Calls(targetMethod))
    //         {
    //             // Bye bye hitsound :D
    //             Plugin.Logger.LogInfo($"Removing instruction: {original.DeclaringType}.{original.Name}[{i}]");
    //             codes[i].opcode = OpCodes.Nop;
    //         }
    //     }

    //     Plugin.Logger.LogInfo($"Done patching {original.DeclaringType}.{original.Name}()");
    //     return codes.AsEnumerable();
    // }


    [HarmonyPatch(typeof(RhythmController), nameof(RhythmController.Hit))]
    [HarmonyPrefix]
    static bool HitPrefix(Height height, ref bool silent)
    {
        // Forces hits to be handled silently
        silent = true;
        return true;
    }


    [HarmonyPatch(typeof(RhythmController), nameof(RhythmController.Dodge))]
    [HarmonyPrefix]
    static bool DodgePrefix(RhythmController __instance, Height height)
    {
        // Same behavior as the original method, just without hitsound
        // This is far from ideal, but reflection is being poopy and won't let me transpile
        Height opposite;
        switch(height)
        {
            case Height.Low:
                opposite = Height.Top;
                break;
            case Height.Top:
                opposite = Height.Low;
                break;
            default:
                opposite = Height.None;
                break;
        }
        __instance.player.alreadyHitHeights.Add(opposite);
        __instance.TutorialResume();

        // Override the original method
        return false;
    }
}