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
            HitsoundManager.ScheduleNote(__instance, __instance.controller.hitSFX, __instance.endTime);
        }

        // Perform the original method
        return true;
    }
}