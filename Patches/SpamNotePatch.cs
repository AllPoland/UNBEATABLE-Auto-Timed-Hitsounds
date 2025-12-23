using FMODUnity;
using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

public class SpamNotePatch
{
    [HarmonyPatch(typeof(SpamNote), "RhythmUpdate_Moving")]
    [HarmonyPrefix]
    static bool RhythmUpdate_MovingPrefix(SpamNote __instance)
    {
        // Schedule the hitsound if necessary
        if(HitsoundManager.BaseQueue.ShouldNoteSchedule(__instance))
        {
            HitsoundManager.BaseQueue.ScheduleNote(__instance, __instance.hitTime, __instance.controller.hitSFX);
        }

        // Perform the original method
        return true;
    }


    [HarmonyPatch(typeof(SpamNote), "RhythmUpdate_Stunned")]
    [HarmonyPrefix]
    static bool RhythmUpdate_StunnedPrefix(SpamNote __instance)
    {
        //Also play hitsounds for button presses during the spam period
        if(__instance.IsSwungAt())
        {
            RuntimeManager.PlayOneShot(__instance.controller.hitSFX);
        }
        return true;
    }
}