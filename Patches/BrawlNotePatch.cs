using FMODUnity;
using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

public class BrawlNotePatch
{
    [HarmonyPatch(typeof(BrawlNote), "RhythmUpdate_Active")]
    [HarmonyPrefix]
    static bool RhythmUpdate_ActivePrefix(BrawlNote __instance)
    {
        

        // Perform the original method
        return true;
    }


    [HarmonyPatch(typeof(BrawlNote), "RhythmUpdate_Stunned")]
    [HarmonyPrefix]
    static bool RhythmUpdate_StunnedPrefix(BrawlNote __instance)
    {
        

        // Perform the original method
        return true;
    }
}