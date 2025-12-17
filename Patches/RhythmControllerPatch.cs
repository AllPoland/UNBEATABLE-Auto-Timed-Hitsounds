using HarmonyLib;
using FMOD.Studio;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

class RhythmControllerPatch
{
    [HarmonyPatch(typeof(RhythmController), nameof(RhythmController.InitializeAndPlay))]
    [HarmonyPostfix]
    static void InitializeAndPlayPostfix(RhythmController __instance)
    {
        // Cache the song EventInstance to get accurate timing straight from the source
        // Good to cache this because we need to use Traverse to access the private property on RhythmTracker
        Traverse tracker = Traverse.Create(__instance.songTracker);
        HitsoundManager.SongInstance = tracker.Field("instance").GetValue<EventInstance>();
    }


    [HarmonyPatch(typeof(RhythmController), "OnDestroy")]
    [HarmonyPrefix]
    static bool OnDestroyPrefix()
    {
        // Clear our scheduled hitsound list so we don't memory leak all over the place
        HitsoundManager.ScheduledNotes.Clear();
        foreach(ScheduledNote note in HitsoundManager.PlayedNotes)
        {
            note.sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        HitsoundManager.PlayedNotes.Clear();
        return true;
    }


    [HarmonyPatch(typeof(RhythmController), "FixedUpdate")]
    [HarmonyPrefix]
    static bool FixedUpdatePrefix()
    {
        // Update scheduled hitsounds to see if they're initialized now
        HitsoundManager.UpdateScheduledNotes();
        HitsoundManager.UpdateScheduledHolds();
        return true;
    }
}