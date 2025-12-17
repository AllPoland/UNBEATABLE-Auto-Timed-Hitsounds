using System.Collections.Generic;
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
        // Clear our hitsound state so we don't memory leak all over the place
        HitsoundManager.ScheduledSounds.Clear();
        foreach(KeyValuePair<ScheduledNote, ScheduledSound> pair in HitsoundManager.PlayedSounds)
        {
            pair.Value.sound.stop(STOP_MODE.IMMEDIATE);
        }
        HitsoundManager.PlayedSounds.Clear();

        HitsoundManager.ScheduledHolds.Clear();
        foreach(KeyValuePair<BaseNote, ScheduledHold> pair in HitsoundManager.PlayedHolds)
        {
            pair.Value.sound.stop(STOP_MODE.IMMEDIATE);
        }
        HitsoundManager.PlayedHolds.Clear();
        return true;
    }


    [HarmonyPatch(typeof(RhythmController), "FixedUpdate")]
    [HarmonyPrefix]
    static bool FixedUpdatePrefix()
    {
        // HitsoundManager piggybacks off of FixedUpdate as long as RhythmController is active
        HitsoundManager.UpdateScheduledSounds();
        HitsoundManager.UpdateScheduledHolds();
        HitsoundManager.DisposeOldSounds();
        return true;
    }
}