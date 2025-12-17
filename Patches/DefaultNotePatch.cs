using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

public class DefaultNotePatch
{
    [HarmonyPatch(typeof(DefaultNote), "RhythmUpdate_Moving")]
    [HarmonyPrefix]
    static bool RhythmUpdate_MovingPrefix(DefaultNote __instance)
    {
        // Schedule the hitsound if necessary
        if(HitsoundManager.ShouldNoteSchedule(__instance))
        {
            HitsoundManager.ScheduleNote(__instance, __instance.controller.hitSFX);
        }

        // Perform the original method
        return true;
    }
}