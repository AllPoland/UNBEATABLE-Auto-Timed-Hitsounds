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
        if(HitsoundManager.BaseQueue.ShouldNoteSchedule(__instance))
        {
            bool useAssistSound = HitsoundUtil.UseAssistSound(__instance.height, __instance.hitTime);
            EventReference sfx = useAssistSound ? __instance.controller.hitAssistSFX : __instance.controller.hitSFX;
            HitsoundManager.BaseQueue.ScheduleNote(__instance, __instance.hitTime, sfx);
        }

        // Perform the original method
        return true;
    }
}