using FMODUnity;
using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;


public class SpamNotePatch
{
    [HarmonyPatch(typeof(SpamNote), "RhythmUpdate_Moving")]
    [HarmonyPrefix]
    static bool RhythmUpdate_Moving(SpamNote __instance)
    {
        // Schedule the hitsound if necessary
        if(HitsoundManager.ShouldNoteSchedule(__instance))
        {
            HitsoundManager.ScheduleNote(__instance, __instance.controller.hitSFX);
        }

        // Perform the original method
        return true;
    }


    [HarmonyPatch(typeof(SpamNote), "RhythmUpdate_Stunned")]
    [HarmonyPrefix]
    static bool RhythmUpdate_Stunned(SpamNote __instance)
    {
        //Also play hitsounds for button presses during the spam period
        if(__instance.IsSwungAt())
        {
            RuntimeManager.PlayOneShot(__instance.controller.hitSFX);
        }
        return true;
    }
}