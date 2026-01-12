using FMOD;
using FMOD.Studio;
using FMODUnity;

namespace AutoTimedHitsounds;

public static class FMODHelper
{
    public static ScheduledSound ScheduleSound(EventReference sfx, ulong startSamples)
    {
        EventInstance newEvent = RuntimeManager.CreateInstance(sfx);
        return new ScheduledSound
        {
            sound = newEvent,
            startSamples = startSamples
        };
    }


    public static ScheduledHold ScheduleHold(EventReference sfx, ulong startSamples, ulong endSamples)
    {
        EventInstance newEvent = RuntimeManager.CreateInstance(sfx);
        return new ScheduledHold
        {
            sound = newEvent,
            startSamples = startSamples,
            endSamples = endSamples
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

        ulong songSamples = TimeHelper.GetSongPosSamples();
        ulong delay = sound.startSamples - songSamples;
        if(delay <= 0)
        {
            // It's too late to try playing this sound
            return true;
        }

        channelGroup.getDSPClock(out ulong _, out ulong parentClock);

        channelGroup.setDelay(parentClock + delay, 0, true);
        sound.sound.start();

        // release() marks the event for cleanup *after* it has played to completion
        sound.sound.release();
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

        ulong songSamples = TimeHelper.GetSongPosSamples();
        ulong delay = hold.startSamples - songSamples;
        ulong endDelay = hold.endSamples - songSamples;
        if(delay <= 0 || endDelay <= 0)
        {
            // It's too late to try playing this sound
            return true;
        }

        channelGroup.getDSPClock(out ulong _, out ulong parentClock);

        channelGroup.setDelay(parentClock + delay, parentClock + endDelay, true);
        hold.sound.start();

        // release() marks the event for cleanup *after* it has played to completion
        hold.sound.release();
        return true;
    }
}