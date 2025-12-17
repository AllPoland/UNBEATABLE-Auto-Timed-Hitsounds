using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

public class DodgeNotePatch
{
    [HarmonyPatch(typeof(DodgeNote), "RhythmUpdate")]
    [HarmonyPrefix]
    static bool RhythmUpdatePrefix(DodgeNote __instance)
    {
        if(__instance.isPast)
        {
            DodgeNoteStateManager.UnregisterNote(__instance);
            return true;
        }

        DodgeNoteState state = DodgeNoteStateManager.GetState(__instance);
        if(!state.markedForSound && __instance.height == __instance.player.height && __instance.WithinHitRange())
        {
            state.markedForSound = true;
            DodgeNoteStateManager.States[__instance] = state;
        }

        if(state.markedForSound)
        {
            // Schedule the hitsound if necessary
            if(HitsoundManager.ShouldNoteSchedule(__instance))
            {
                HitsoundManager.ScheduleNote(__instance, __instance.controller.dodgeSFX);
            }
        }

        // Perform the original method
        return true;
    }
}