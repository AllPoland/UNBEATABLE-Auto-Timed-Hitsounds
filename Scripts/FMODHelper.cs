using FMOD;
using FMOD.Studio;
using FMODUnity;

namespace AutoTimedHitsounds;

public static class FMODHelper
{
    public static ScheduledSound ScheduleSound(EventReference sfx, ulong songSamples, ulong hitsoundSamples)
    {
        ulong delaySamples = hitsoundSamples - songSamples;
        // Plugin.Logger.LogInfo($"song samples: {songSamples}, hitsound samples: {hitsoundSamples}, delay: {delaySamples}");

        EventInstance newEvent = RuntimeManager.CreateInstance(sfx);
        return new ScheduledSound
        {
            sound = newEvent,
            delaySamples = delaySamples
        };
    }


    public static ScheduledHold ScheduleHold(EventReference sfx, ulong songSamples, ulong startSamples, ulong endSamples)
    {
        ulong delaySamples = startSamples - songSamples;
        ulong endDelaySamples = endSamples - songSamples;

        EventInstance newEvent = RuntimeManager.CreateInstance(sfx);
        return new ScheduledHold
        {
            sound = newEvent,
            delaySamples = delaySamples,
            endDelaySamples = endDelaySamples
        };
    }


    public static bool TryPlaySound(ScheduledSound sound)
    {
        RESULT channelGroupStatus = sound.sound.getChannelGroup(out ChannelGroup channelGroup);
        if(channelGroupStatus != RESULT.OK)
        {
            // We need to wait a few frames before we can grab the channel group
            return false;
        }

        channelGroup.getDSPClock(out ulong dspClock, out ulong parentClock);

        channelGroup.setDelay(parentClock + sound.delaySamples, 0, true);
        sound.sound.start();

        // release() marks the event for cleanup *after* it has played to completion
        sound.sound.release();

        // Plugin.Logger.LogInfo($"delay seconds: {note.delaySeconds}, delay samples: {delaySamples}, dsp clock: {parentClock}");
        return true;
    }


    public static bool TryPlayHoldSound(ScheduledHold hold)
    {
        RESULT channelGroupStatus = hold.sound.getChannelGroup(out ChannelGroup channelGroup);
        if(channelGroupStatus != RESULT.OK)
        {
            // We need to wait a few frames before we can grab the channel group
            return false;
        }

        channelGroup.getDSPClock(out ulong dspClock, out ulong parentClock);

        channelGroup.setDelay(parentClock + hold.delaySamples, parentClock + hold.endDelaySamples, true);
        hold.sound.start();

        // release() marks the event for cleanup *after* it has played to completion
        hold.sound.release();

        return true;
    }
}