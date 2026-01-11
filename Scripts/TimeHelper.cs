using FMOD;
using FMOD.Studio;
using Rhythm;
using UnityEngine;

namespace AutoTimedHitsounds;

public static class TimeHelper
{
    public static EventInstance SongInstance;
    public static RhythmTracker RhythmTracker;

    public static float positionOffset;
    public static float audioOffset;
    public static bool enableCountdown;
    public static float CountdownLength;


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

        return timelinePosition + positionOffset - audioOffset;
    }


    public static ulong GetSongPosSamples()
    {
        float songPosition = GetSongPosMS();

        int sampleRate = AudioSettings.GetSampleRate();
        return (ulong)(songPosition * (sampleRate / 1000));
    }
}