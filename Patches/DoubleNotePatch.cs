using FMODUnity;
using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

public class DoubleNotePatch
{
    [HarmonyPatch(typeof(DoubleNote), "RhythmUpdate_Moving")]
    [HarmonyPrefix]
    static bool RhythmUpdate_MovingPrefix(DoubleNote __instance)
    {
        // Schedule the hitsound if necessary
        SoundQueue<BaseNote> queue = HitsoundManager.BaseQueue;
        if(queue.ShouldNoteSchedule(__instance))
        {
            bool useAssistSound = HitsoundUtil.UseAssistSound(__instance, __instance.height, __instance.hitTime);
            EventReference sfx = useAssistSound ? __instance.controller.hitAssistSFX : __instance.controller.hitSFX;

            // Plug in the end time to avoid unscheduling this sound before the entire note is done
            // (if that happens, it could get rescheduled and stack on top of the second sound)
            queue.ScheduleNote(__instance, sfx, __instance.hitTime, __instance.endTime);
        }

        // Schedule the hitsound for the second hit if necessary
        if(queue.ShouldNoteSchedule(__instance, 1))
        {
            bool useAssistSound = HitsoundUtil.UseAssistSound(__instance, HeightUtil.OppositeHeight(__instance.height), __instance.endTime);
            EventReference sfx = useAssistSound ? __instance.controller.hitAssistSFX : __instance.controller.hitSFX;
            queue.ScheduleNote(__instance, sfx, __instance.endTime, __instance.endTime, 1);
        }

        // Perform the original method
        return true;
    }


    [HarmonyPatch(typeof(DoubleNote), "RhythmUpdate_Stunned")]
    [HarmonyPrefix]
    static bool RhythmUpdate_StunnedPrefix(DoubleNote __instance)
    {
        // Schedule the hitsound for the second hit if necessary
        SoundQueue<BaseNote> queue = HitsoundManager.BaseQueue;
        if(queue.ShouldNoteSchedule(__instance, 1))
        {
            bool useAssistSound = HitsoundUtil.UseAssistSound(__instance, __instance.height, __instance.endTime);
            EventReference sfx = useAssistSound ? __instance.controller.hitAssistSFX : __instance.controller.hitSFX;
            queue.ScheduleNote(__instance, sfx, __instance.endTime, __instance.endTime, 1);
        }

        // Perform the original method
        return true;
    }
}