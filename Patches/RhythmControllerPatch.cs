using HarmonyLib;
using FMOD.Studio;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

class RhythmControllerPatch
{
    [HarmonyPatch(typeof(RhythmController), "OnDestroy")]
    [HarmonyPrefix]
    static bool OnDestroy()
    {
        // Clear our scheduled hitsound list so we don't memory leak all over the place
        HitsoundManager.ScheduledNotes.Clear();
        foreach(ScheduledNote note in HitsoundManager.PlayedNotes)
        {
            note.sound.stop(STOP_MODE.IMMEDIATE);
        }
        HitsoundManager.PlayedNotes.Clear();
        return true;
    }


    [HarmonyPatch(typeof(RhythmController), "FixedUpdate")]
    [HarmonyPrefix]
    static bool FixedUpdate()
    {
        // Update scheduled hitsounds to see if they're initialized now
        HitsoundManager.UpdateScheduledNotes();
        return true;
    }


    [HarmonyPatch(typeof(RhythmController), nameof(RhythmController.Hit))]
    [HarmonyPrefix]
    static bool HitPrefix(RhythmController __instance, Height height, bool silent = false)
    {
        // Original method body minus hitsounds
        __instance.player.alreadyHitHeights.Add(height);
        RhythmCamera.Shake(0.02f, 0.05f);
        __instance.TutorialResume();

        // Override the original method
        return false;
    }


    [HarmonyPatch(typeof(RhythmController), nameof(RhythmController.Dodge))]
    [HarmonyPrefix]
    static bool DodgePrefix(RhythmController __instance, Height height)
    {
        // Original method body minus hitsounds
        __instance.player.alreadyHitHeights.Add(HeightHelper.GetOpposite(height));
        __instance.TutorialResume();

        // Override the original method
        return false;
    }

    // We still want miss sounds to play as normal (they will now play on top of hitsounds)
}