using FMODUnity;
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
            bool useAssistSound = HitsoundUtil.UseAssistSound(__instance, __instance.height, __instance.hitTime);
            EventReference sfx = useAssistSound ? __instance.controller.hitAssistSFX : __instance.controller.hitSFX;
            HitsoundManager.ScheduleNote(__instance, sfx);
        }

        // Perform the original method
        return true;
    }
}