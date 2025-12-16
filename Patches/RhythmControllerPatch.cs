using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

class RhythmControllerPatch
{
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