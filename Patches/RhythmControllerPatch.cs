using System.Collections.Generic;
using HarmonyLib;
using FMOD.Studio;
using Rhythm;
using System.Linq;

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

        if(__instance.enableCountdown)
        {
            // Save the countdown length so that we can calculate negative time values
            // This is necessary in order to schedule notes placed within the first few ms of the map (or at 0ms)
            TimingPointInfo timingPointInfo = __instance.beatmap.timingPoints.FirstOrDefault(x => x.beatLength > 0f);
            HitsoundManager.CountdownLength = 750f + 8f * timingPointInfo.beatLength;
        }
    }


    [HarmonyPatch(typeof(RhythmController), "OnDestroy")]
    [HarmonyPrefix]
    static bool OnDestroyPrefix()
    {
        // Clear our hitsound state so we don't memory leak all over the place
        for(byte id = 0; id < HitsoundManager.ScheduledSounds.Length; id++)
        {
            HitsoundManager.ScheduledSounds[id].Clear();
            foreach(KeyValuePair<BaseNote, ScheduledSound> pair in HitsoundManager.PlayedSounds[id])
            {
                pair.Value.sound.stop(STOP_MODE.IMMEDIATE);
            }
            HitsoundManager.PlayedSounds[id].Clear();
        }

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