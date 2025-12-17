using System.Collections.Generic;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Rhythm;
using UnityEngine;

namespace AutoTimedHitsounds;

public static class HitsoundManager
{
    public static Dictionary<BaseNote, ScheduledSound>[] PlayedSounds = [[],[]];
    public static Dictionary<BaseNote, ScheduledSound>[] ScheduledSounds = [[],[]];

    public static Dictionary<BaseNote, ScheduledHold> PlayedHolds = new Dictionary<BaseNote, ScheduledHold>();
    public static Dictionary<BaseNote, ScheduledHold> ScheduledHolds = new Dictionary<BaseNote, ScheduledHold>();

    public static EventInstance SongInstance;

    private const float minSongPositionToSchedule = 100f;


    public static void ScheduleNote(BaseNote note, EventReference sfx, float noteTime, float endTime, byte id = 0)
    {
        RESULT result = SongInstance.getTimelinePosition(out int songPosition);
        if(result != RESULT.OK)
        {
            Plugin.Logger.LogWarning($"Failed to get song position: {result}");
            return;
        }

        if(songPosition < minSongPositionToSchedule)
        {
            // Give song position a moment to settle down before trying to schedule
            return;
        }

        float noteOffset = noteTime - songPosition;
        if(noteOffset > Plugin.MaxScheduleOffset || noteOffset < Plugin.MinScheduleOffset)
        {
            // It's either too late or too early to schedule this
            return;
        }

        int sampleRate = AudioSettings.GetSampleRate();
        ulong songSamples = (ulong)(songPosition / 1000f * sampleRate);
        ulong noteSamples = (ulong)(noteTime / 1000f * sampleRate);

        ScheduledSound newSound = FMODHelper.ScheduleSound(sfx, songSamples, noteSamples);
        newSound.endTime = endTime;

        ScheduledSounds[id].Add(note, newSound);
    }


    public static void ScheduleNote(BaseNote note, EventReference sfx, byte id = 0)
    {
        // Note and song time are stored in milliseconds
        float noteTime = note.hitTime;
        ScheduleNote(note, sfx, noteTime, noteTime, id);
    }


    public static void ScheduleHold(HoldNote note, EventReference sfx)
    {
        RESULT result = SongInstance.getTimelinePosition(out int songPosition);
        if(result != RESULT.OK)
        {
            Plugin.Logger.LogWarning($"Failed to get song position: {result}");
            return;
        }

        if(songPosition < minSongPositionToSchedule)
        {
            // Give song position a moment to settle down before trying to schedule
            return;
        }

        float startTime = note.hitTime;
        float noteOffset = startTime - songPosition;
        if(noteOffset > Plugin.MaxScheduleOffset)
        {
            // It's too early to schedule this
            return;
        }

        float endTime = note.endTime;

        Plugin.Logger.LogInfo($"Scheduled hold, song time {songPosition}, start time {startTime}, end time {endTime}");

        int sampleRate = AudioSettings.GetSampleRate();
        ulong songSamples = (ulong)(songPosition / 1000f * sampleRate);
        ulong startSamples = (ulong)(startTime / 1000f * sampleRate);
        ulong endSamples = (ulong)(endTime / 1000f * sampleRate);

        ScheduledHold newHold = FMODHelper.ScheduleHold(sfx, songSamples, startSamples, endSamples);
        newHold.endTime = endTime;

        ScheduledHolds.Add(note, newHold);
    }


    public static bool ShouldNoteSchedule(BaseNote note, byte id = 0)
    {

        return !ScheduledSounds[id].ContainsKey(note) && !PlayedSounds[id].ContainsKey(note);
    }


    public static bool ShouldHoldSchedule(HoldNote note)
    {
        return !ScheduledHolds.ContainsKey(note) && !PlayedHolds.ContainsKey(note);
    }


    public static void UpdateScheduledSounds()
    {
        for(byte id = 0; id < ScheduledSounds.Length; id++)
        {
            List<BaseNote> finishedNotes = new List<BaseNote>();
            foreach(KeyValuePair<BaseNote, ScheduledSound> pair in ScheduledSounds[id])
            {
                if(FMODHelper.TryPlaySound(pair.Value))
                {
                    // This sound is now good to go
                    PlayedSounds[id].Add(pair.Key, pair.Value);
                    finishedNotes.Add(pair.Key);
                }
            }

            foreach(BaseNote note in finishedNotes)
            {
                ScheduledSounds[id].Remove(note);
            }
        }
    }


    public static void UpdateScheduledHolds()
    {
        List<BaseNote> finishedHolds = new List<BaseNote>();
        foreach(KeyValuePair<BaseNote, ScheduledHold> pair in ScheduledHolds)
        {
            if(FMODHelper.TryPlayHoldSound(pair.Value))
            {
                // This sound is now good to go
                PlayedHolds.Add(pair.Key, pair.Value);
                finishedHolds.Add(pair.Key);
            }
        }

        foreach(BaseNote note in finishedHolds)
        {
            ScheduledHolds.Remove(note);
        }
    }


    public static void DisposeOldSounds()
    {
        const float MinTimeOffsetToDispose = 300f;
        RESULT result = SongInstance.getTimelinePosition(out int songPosition);
        if(result != RESULT.OK)
        {
            Plugin.Logger.LogWarning($"Failed to get song position: {result}");
            return;
        }

        for(byte id = 0; id < PlayedSounds.Length; id++)
        {
            List<BaseNote> notesToDispose = new List<BaseNote>();
            foreach(KeyValuePair<BaseNote, ScheduledSound> pair in PlayedSounds[id])
            {
                float noteTime = pair.Value.endTime;
                float timeOffset = songPosition - noteTime;
                if(timeOffset >= MinTimeOffsetToDispose)
                {
                    notesToDispose.Add(pair.Key);
                }
            }

            foreach(BaseNote note in notesToDispose)
            {
                UnregisterNote(note, id);
            }
        }

        List<BaseNote> holdsToDispose = new List<BaseNote>();
        foreach(KeyValuePair<BaseNote, ScheduledHold> pair in PlayedHolds)
        {
            float endTime = pair.Value.endTime;
            float timeOffset = songPosition - endTime;
            if(timeOffset >= MinTimeOffsetToDispose)
            {
                holdsToDispose.Add(pair.Key);
            }
        }

        foreach(BaseNote note in holdsToDispose)
        {
            UnregisterHold(note);
        }
    }


    public static void UnregisterNote(BaseNote note, byte id)
    {
        ScheduledSounds[id].Remove(note);
        if(PlayedSounds[id].ContainsKey(note))
        {
            PlayedSounds[id][note].sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            PlayedSounds[id].Remove(note);
        }
    }


    public static void UnregisterHold(BaseNote note)
    {
        ScheduledHolds.Remove(note);
        if(PlayedHolds.ContainsKey(note))
        {
            PlayedHolds[note].sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            PlayedHolds.Remove(note);
        }
    }
}


public struct ScheduledSound
{
    public EventInstance sound;
    public float endTime;
    public ulong delaySamples;
}


public struct ScheduledHold
{
    public EventInstance sound;
    public float endTime;
    public ulong delaySamples;
    public ulong endDelaySamples;
}