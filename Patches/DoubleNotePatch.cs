using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;


public class DoubleNotePatch
{
    [HarmonyPatch(typeof(DoubleNote), "RhythmUpdate_Moving")]
    [HarmonyPrefix]
    static bool RhythmUpdate_Moving(DoubleNote __instance)
    {
        // Schedule the hitsound if necessary
        if(HitsoundManager.ShouldNoteSchedule(__instance))
        {
            HitsoundManager.ScheduleNote(__instance, __instance.controller.hitSFX);
        }

        // Perform the original method
        return true;
    }


    [HarmonyPatch(typeof(DoubleNote), "RhythmUpdate_Stunned")]
    [HarmonyPrefix]
    static bool RhythmUpdate_Stunned(DoubleNote __instance)
    {
        // Schedule the hitsound for the second hit if necessary
        if(HitsoundManager.ShouldNoteSchedule(__instance, 1))
        {
            HitsoundManager.ScheduleNote(__instance, __instance.controller.hitSFX, __instance.endTime, 1);
        }

        // Perform the original method
        return true;
    }
}