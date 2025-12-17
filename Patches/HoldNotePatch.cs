using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

public class HoldNotePatch
{
    [HarmonyPatch(typeof(HoldNote), "OnDestroy")]
    [HarmonyPrefix]
    static bool OnDestroyPrefix(HoldNote __instance)
    {
        // The note gets destroyed when the player releases early, so stop any sounds here
        HitsoundManager.UnregisterNote(__instance);
        HitsoundManager.UnregisterHold(__instance);

        // Perform the original method
        return true;
    }


    [HarmonyPatch(typeof(HoldNote), "RhythmUpdate_Moving")]
    [HarmonyPrefix]
    static bool RhythmUpdate_MovingPrefix(HoldNote __instance)
    {
        // Schedule the hold sound if necessary
        if(HitsoundManager.ShouldHoldSchedule(__instance))
        {
            HitsoundManager.ScheduleHold(__instance, __instance.controller.holdSFX);
        }

        // Schedule the release hitsound if necessary
        if(HitsoundManager.ShouldNoteSchedule(__instance))
        {
            HitsoundManager.ScheduleNote(__instance, __instance.controller.hitSFX, __instance.endTime, __instance.endTime);
        }

        // Perform the original method
        return true;
    }


    [HarmonyPatch(typeof(HoldNote), "RhythmUpdate_Stunned")]
    [HarmonyPrefix]
    static bool RhythmUpdate_StunnedPrefix(HoldNote __instance)
    {
        // Schedule the release hitsound if necessary
        if(HitsoundManager.ShouldNoteSchedule(__instance))
        {
            HitsoundManager.ScheduleNote(__instance, __instance.controller.hitSFX, __instance.endTime, __instance.endTime);
        }

        // Perform the original method
        return true;
    }
}