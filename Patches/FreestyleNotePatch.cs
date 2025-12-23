using FMODUnity;
using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

public class FreestyleNotePatch
{
    private static void RhythmUpdate(FreestyleNote __instance)
    {
        // Schedule the hitsound if necessary
        if(HitsoundManager.ShouldNoteSchedule(__instance))
        {
            bool useAssistSound = HitsoundUtil.UseAssistSound(__instance, __instance.height, __instance.hitTime);
            EventReference sfx = useAssistSound ? __instance.controller.hitAssistSFX : __instance.controller.hitSFX;
            HitsoundManager.ScheduleNote(__instance, sfx);
        }

        if(!__instance.hasChildNotes)
        {
            return;
        }

        foreach(FreestyleNote child in __instance.childNotes)
        {
            // Schedule the hitsound for each child if necessary
            if(HitsoundManager.ShouldNoteSchedule(child))
            {
                bool useAssistSound = HitsoundUtil.UseAssistSound(child, child.height, child.hitTime);
                EventReference sfx = useAssistSound ? child.controller.hitAssistSFX : child.controller.hitSFX;
                HitsoundManager.ScheduleNote(child, sfx);
            }
        }
    }


    [HarmonyPatch(typeof(FreestyleNote), "RhythmUpdate_Moving")]
    [HarmonyPrefix]
    static bool RhythmUpdate_MovingPrefix(FreestyleNote __instance)
    {
        RhythmUpdate(__instance);

        // Perform the original method
        return true;
    }


    [HarmonyPatch(typeof(FreestyleNote), "RhythmUpdate_Stunned")]
    [HarmonyPrefix]
    static bool RhythmUpdate_StunnedPrefix(FreestyleNote __instance)
    {
        RhythmUpdate(__instance);

        // Perform the original method
        return true;
    }
}