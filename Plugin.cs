using BepInEx;
using BepInEx.Logging;

namespace AutoTimedHitsounds;

[BepInPlugin(PluginReleaseInfo.PLUGIN_GUID, PluginReleaseInfo.PLUGIN_NAME, PluginReleaseInfo.PLUGIN_VERSION)]
[BepInProcess("UNBEATABLE.exe")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PluginReleaseInfo.PLUGIN_GUID} is loaded!");
    }
}
