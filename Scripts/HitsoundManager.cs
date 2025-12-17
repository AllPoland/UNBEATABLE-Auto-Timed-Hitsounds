using System.Collections.Generic;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Rhythm;
using UnityEngine;

namespace AutoTimedHitsounds;

public static class HitsoundManager
{
    public static Dictionary<ScheduledNote, ScheduledSound> PlayedSounds = new Dictionary<ScheduledNote, ScheduledSound>();
    public static Dictionary<ScheduledNote, ScheduledSound> ScheduledSounds = new Dictionary<ScheduledNote, ScheduledSound>();

    public static Dictionary<BaseNote, ScheduledHold> PlayedHolds = new Dictionary<BaseNote, ScheduledHold>();
    public static Dictionary<BaseNote, ScheduledHold> ScheduledHolds = new Dictionary<BaseNote, ScheduledHold>();

    public static EventInstance SongInstance;


    public static void ScheduleNote(BaseNote note, EventReference sfx, float noteTime, byte id = 0)
    {
        RESULT result = SongInstance.getTimelinePosition(out int songPosition);
        if(result != RESULT.OK)
        {
            Plugin.Logger.LogWarning($"Failed to get song position: {result}");
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
        newSound.time = noteTime;

        ScheduledNote newNote = new ScheduledNote
        {
            note = note,
            id = id
        };
        ScheduledSounds.Add(newNote, newSound);
    }


    public static void ScheduleNote(BaseNote note, EventReference sfx, byte id = 0)
    {
        // Note and song time are stored in milliseconds
        float noteTime = note.hitTime;
        ScheduleNote(note, sfx, noteTime, id);
    }


    public static void ScheduleHold(HoldNote note, EventReference sfx)
    {
        RESULT result = SongInstance.getTimelinePosition(out int songPosition);
        if(result != RESULT.OK)
        {
            Plugin.Logger.LogWarning($"Failed to get song position: {result}");
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
        ScheduledNote key = new ScheduledNote
        {
            note = note,
            id = id
        };

        return !ScheduledSounds.ContainsKey(key) && !PlayedSounds.ContainsKey(key);
    }


    public static bool ShouldHoldSchedule(HoldNote note)
    {
        return !ScheduledHolds.ContainsKey(note) && !PlayedHolds.ContainsKey(note);
    }


    public static void UpdateScheduledSounds()
    {
        List<ScheduledNote> finishedNotes = new List<ScheduledNote>();
        foreach(KeyValuePair<ScheduledNote, ScheduledSound> pair in ScheduledSounds)
        {
            if(FMODHelper.TryPlaySound(pair.Value))
            {
                // This sound is now good to go
                PlayedSounds.Add(pair.Key, pair.Value);
                finishedNotes.Add(pair.Key);
            }
        }

        foreach(ScheduledNote note in finishedNotes)
        {
            ScheduledSounds.Remove(note);
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

        List<ScheduledNote> notesToDispose = new List<ScheduledNote>();
        foreach(KeyValuePair<ScheduledNote, ScheduledSound> pair in PlayedSounds)
        {
            float noteTime = pair.Value.time;
            float timeOffset = songPosition - noteTime;
            if(timeOffset >= MinTimeOffsetToDispose)
            {
                notesToDispose.Add(pair.Key);
            }
        }

        foreach(ScheduledNote note in notesToDispose)
        {
            UnregisterNote(note.note, note.id);
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
        ScheduledNote key = new ScheduledNote
        {
            note = note,
            id = id
        };

        ScheduledSounds.Remove(key);
        if(PlayedSounds.ContainsKey(key))
        {
            PlayedSounds[key].sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            PlayedSounds.Remove(key);
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


public struct ScheduledNote
{
    public BaseNote note;
    public byte id;
}


public struct ScheduledSound
{
    public EventInstance sound;
    public float time;
    public ulong delaySamples;
}


public struct ScheduledHold
{
    public EventInstance sound;
    public float endTime;
    public ulong delaySamples;
    public ulong endDelaySamples;
}