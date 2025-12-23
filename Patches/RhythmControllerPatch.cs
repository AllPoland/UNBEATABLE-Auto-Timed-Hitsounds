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
        TimeHelper.SongInstance = tracker.Field("instance").GetValue<EventInstance>();
        TimeHelper.positionOffset = tracker.Field("positionOffset").GetValue<float>();

        TimeHelper.enableCountdown = __instance.enableCountdown;
        TimeHelper.RhythmTracker = __instance.songTracker;

        if(__instance.enableCountdown)
        {
            // Save the countdown length so that we can calculate negative time values
            // This is necessary in order to schedule notes placed within the first few ms of the map (or at 0ms)
            TimingPointInfo timingPointInfo = __instance.beatmap.timingPoints.FirstOrDefault(x => x.beatLength > 0f);
            TimeHelper.CountdownLength = 750f + 8f * timingPointInfo.beatLength;
        }
    }


    [HarmonyPatch(typeof(RhythmController), "OnDestroy")]
    [HarmonyPrefix]
    static bool OnDestroyPrefix()
    {
        // Clear our hitsound state so we don't memory leak all over the place
        HitsoundManager.UnregisterAllSounds();
        return true;
    }


    [HarmonyPatch(typeof(RhythmController), "FixedUpdate")]
    [HarmonyPrefix]
    static bool FixedUpdatePrefix()
    {
        // HitsoundManager piggybacks off of FixedUpdate as long as RhythmController is active
        HitsoundManager.UpdateScheduledSounds();
        HitsoundManager.DisposeOldSounds();
        return true;
    }
}