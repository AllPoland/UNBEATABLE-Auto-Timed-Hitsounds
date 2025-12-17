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
        try
        {
            Logger.LogInfo("Patching out base game hitsounds.");
            Harmony.CreateAndPatchAll(typeof(RhythmController_Mute_Patch));
        }
        catch(System.Exception e)
        {
            Logger.LogFatal($"{e.Message}, {e.StackTrace}");
        }

        try
        {
            Harmony.CreateAndPatchAll(typeof(RhythmBaseCharacter_Mute_Patch));
        }
        catch(System.Exception e)
        {
            Logger.LogFatal($"{e.Message}, {e.StackTrace}");
        }

        try
        {
            Logger.LogInfo("Patching in auto hitsounds.");
            Harmony.CreateAndPatchAll(typeof(RhythmControllerPatch));
            Harmony.CreateAndPatchAll(typeof(DefaultNotePatch));
            Harmony.CreateAndPatchAll(typeof(DodgeNotePatch));
            Harmony.CreateAndPatchAll(typeof(DoubleNotePatch));
            Harmony.CreateAndPatchAll(typeof(FreestyleNotePatch));
            Harmony.CreateAndPatchAll(typeof(HoldNotePatch));
            Harmony.CreateAndPatchAll(typeof(SpamNotePatch));
        }
        catch(System.Exception e)
        {
            Logger.LogFatal($"{e.Message}, {e.StackTrace}");
        }

        Logger.LogInfo($"Plugin {PluginReleaseInfo.PLUGIN_GUID} is loaded!");
    }
}