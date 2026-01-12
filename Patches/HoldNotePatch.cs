using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

public class HoldNotePatch
{
    [HarmonyPatch(typeof(HoldNote), "OnDestroy")]
    [HarmonyPrefix]
    static bool OnDestroyPrefix(HoldNote __instance)
    {
        if(!__instance.WithinHitRange())
        {
            // The note gets destroyed when the player releases early, so stop any sounds here
            SoundQueue<BaseNote> queue = HitsoundManager.BaseQueue;
            queue.UnregisterNote(__instance);
            queue.UnregisterHold(__instance);
        }

        // Perform the original method
        return true;
    }


    [HarmonyPatch(typeof(HoldNote), "RhythmUpdate_Moving")]
    [HarmonyPrefix]
    static bool RhythmUpdate_MovingPrefix(HoldNote __instance)
    {
        // Schedule the hold sound if necessary
        SoundQueue<BaseNote> queue = HitsoundManager.BaseQueue;
        if(queue.ShouldHoldSchedule(__instance))
        {
            queue.ScheduleHold(__instance, __instance.hitTime, __instance.endTime, __instance.controller.holdSFX);
        }

        // Schedule the release hitsound if necessary
        if(queue.ShouldNoteSchedule(__instance))
        {
            queue.ScheduleNote(__instance, __instance.controller.hitSFX, __instance.endTime, __instance.endTime);
        }

        // Perform the original method
        return true;
    }


    [HarmonyPatch(typeof(HoldNote), "RhythmUpdate_Stunned")]
    [HarmonyPrefix]
    static bool RhythmUpdate_StunnedPrefix(HoldNote __instance)
    {
        // Schedule the release hitsound if necessary
        SoundQueue<BaseNote> queue = HitsoundManager.BaseQueue;
        if(queue.ShouldNoteSchedule(__instance))
        {
            queue.ScheduleNote(__instance, __instance.controller.hitSFX, __instance.endTime, __instance.endTime);
        }

        // Perform the original method
        return true;
    }
}