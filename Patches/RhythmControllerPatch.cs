using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds;

class RhythmControllerPatch
{
    [HarmonyPatch(typeof(RhythmController), "Hit")]
    [HarmonyPrefix]
    static bool HitPrefix(RhythmController __instance, Height height, bool silent = false)
    {
        Plugin.Logger.LogInfo("Hit a noet !!!!");
        // Run the original method
        return true;
    }
}