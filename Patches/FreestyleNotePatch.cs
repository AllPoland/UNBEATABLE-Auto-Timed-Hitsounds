using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;


public class FreestyleNotePatch
{
    [HarmonyPatch(typeof(FreestyleNote), "RhythmUpdate_Moving")]
    [HarmonyPrefix]
    static bool RhythmUpdate_Moving(FreestyleNote __instance)
    {
        // Schedule the hitsound if necessary
        if(HitsoundManager.ShouldNoteSchedule(__instance))
        {
            HitsoundManager.ScheduleNote(__instance, __instance.controller.hitSFX);
        }

        // Perform the original method
        return true;
    }


    [HarmonyPatch(typeof(FreestyleNote), "RhythmUpdate_Stunned")]
    [HarmonyPrefix]
    static bool RhythmUpdate_Stunned(FreestyleNote __instance)
    {
        if(!__instance.hasChildNotes)
        {
            return true;
        }

        foreach(FreestyleNote child in __instance.childNotes)
        {
            // Schedule the hitsound for each child if necessary
            if(HitsoundManager.ShouldNoteSchedule(child))
            {
                HitsoundManager.ScheduleNote(child, child.controller.hitSFX);
            }
        }

        // Perform the original method
        return true;
    }
}