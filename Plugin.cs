using AutoTimedHitsounds.Patches;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace AutoTimedHitsounds;

[BepInPlugin(PluginReleaseInfo.PLUGIN_GUID, PluginReleaseInfo.PLUGIN_NAME, PluginReleaseInfo.PLUGIN_VERSION)]
[BepInProcess("UNBEATABLE.exe")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    public static float MaxScheduleOffset = 200f;
    public static float MinScheduleOffset = 10f;

        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        // Patch all method overrides
        Harmony.CreateAndPatchAll(typeof(RhythmControllerPatch));
        Harmony.CreateAndPatchAll(typeof(RhythmBaseCharacter_Mute_Patch));

        Harmony.CreateAndPatchAll(typeof(DefaultNotePatch));
        Harmony.CreateAndPatchAll(typeof(DodgeNotePatch));
        Harmony.CreateAndPatchAll(typeof(DoubleNotePatch));
        Harmony.CreateAndPatchAll(typeof(FreestyleNotePatch));
        Harmony.CreateAndPatchAll(typeof(SpamNotePatch));

        Logger.LogInfo($"Plugin {PluginReleaseInfo.PLUGIN_GUID} is loaded!");
    }
}