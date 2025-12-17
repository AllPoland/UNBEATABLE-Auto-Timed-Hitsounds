using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

public class DodgeNotePatch
{
    [HarmonyPatch(typeof(DodgeNote), "RhythmUpdate")]
    [HarmonyPrefix]
    static bool RhythmUpdatePrefix(DodgeNote __instance)
    {
        // Schedule the hitsound if necessary
        if(HitsoundManager.ShouldNoteSchedule(__instance))
        {
            HitsoundManager.ScheduleNote(__instance, __instance.controller.dodgeSFX);
        }

        // Perform the original method
        return true;
    }
}