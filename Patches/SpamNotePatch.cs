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
            bool useAssistSound = HitsoundUtil.UseAssistSound(__instance.height, __instance.hitTime);
            EventReference sfx = useAssistSound ? __instance.controller.hitAssistSFX : __instance.controller.hitSFX;
            HitsoundManager.BaseQueue.ScheduleNote(__instance, __instance.hitTime, sfx);
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