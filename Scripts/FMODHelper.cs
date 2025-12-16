using FMOD;
using FMOD.Studio;
using FMODUnity;
using Rhythm;
using UnityEngine;

namespace AutoTimedHitsounds;

public static class FMODHelper
{
    public static ScheduledNote ScheduleSound(BaseNote note, EventReference sfx, float songTime, float hitsoundTime)
    {
        float delaySeconds = hitsoundTime - songTime;

        EventInstance newEvent = RuntimeManager.CreateInstance(sfx);
        return new ScheduledNote
        {
            note = note,
            sound = newEvent,
            delaySeconds = delaySeconds
        };
    }


    public static bool TryPlaySound(ScheduledNote note)
    {
        RESULT channelGroupStatus = note.sound.getChannelGroup(out ChannelGroup channelGroup);
        if(channelGroupStatus != RESULT.OK)
        {
            // We need to wait a few frames before we can grab the channel group
            return false;
        }

        ulong delaySamples = (ulong)(note.delaySeconds * AudioSettings.GetSampleRate());
        channelGroup.getDSPClock(out ulong dspClock, out ulong parentClock);

        channelGroup.setDelay(parentClock + delaySamples, 0, true);
        note.sound.start();

        // release() marks the event for cleanup *after* it has played to completion
        note.sound.release();

        // Plugin.Logger.LogInfo($"delay seconds: {note.delaySeconds}, delay samples: {delaySamples}, dsp clock: {parentClock}");
        return true;
    }
}