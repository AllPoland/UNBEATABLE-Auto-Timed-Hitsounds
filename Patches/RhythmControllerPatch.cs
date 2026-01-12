using HarmonyLib;
using FMOD.Studio;
using Rhythm;
using UnityEngine;

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

        TimeHelper.PositionOffset = tracker.Field("positionOffset").GetValue<float>();
        TimeHelper.VisualOffset = FileStorage.options.GetVideoOffset();

        // "Audio Offset" is a misnomer for this - it's only used for timing judgement offsets
        TimeHelper.InputOffset = FileStorage.options.GetAudioOffset();
        TimeHelper.SampleRateMS = AudioSettings.GetSampleRate() / 1000f;

        TimeHelper.RhythmTracker = __instance.songTracker;
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