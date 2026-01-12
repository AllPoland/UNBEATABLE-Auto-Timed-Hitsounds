using FMOD;
using FMOD.Studio;
using Rhythm;

namespace AutoTimedHitsounds;

public static class TimeHelper
{
    public static EventInstance SongInstance;
    public static RhythmTracker RhythmTracker;

    public static float PositionOffset;
    public static float VisualOffset;
    public static float InputOffset;

    public static float SampleRateMS;


    public static float GetSongPosMS()
    {
        RESULT result = SongInstance.getTimelinePosition(out int timelinePosition);
        if(result != RESULT.OK)
        {
            return RhythmTracker.Position;
        }

        if(timelinePosition <= 0f)
        {
            // We're in the countdown time, so we need to use the countdown position instead
            return RhythmTracker.Position;
        }

        return timelinePosition + PositionOffset;
    }


    public static ulong GetSongPosSamples()
    {
        return (ulong)(GetSongPosMS() * SampleRateMS);
    }


    public static float GetInputPosMS()
    {
        return GetSongPosMS() + InputOffset;
    }


    public static float GetScheduleMS(float noteTime)
    {
        return noteTime + VisualOffset;
    }


    public static ulong GetScheduleSamples(float noteTime)
    {
        return (ulong)(GetScheduleMS(noteTime) * SampleRateMS);
    }
}