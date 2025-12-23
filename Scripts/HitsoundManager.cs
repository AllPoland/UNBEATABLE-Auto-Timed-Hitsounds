using System.Collections.Generic;
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


    public static void ScheduleNote(BaseNote note, EventReference sfx, float noteTime, float endTime, byte id = 0)
    {
        float songPosition = TimeHelper.GetSongPosMS();

        float noteOffset = noteTime - songPosition;
        if(noteOffset > Plugin.MaxScheduleOffset || noteOffset < Plugin.MinScheduleOffset)
        {
            // It's either too late or too early to schedule this
            return;
        }

        int sampleRate = AudioSettings.GetSampleRate();
        ulong noteSamples = (ulong)(noteTime * (sampleRate / 1000));

        ScheduledSound newSound = FMODHelper.ScheduleSound(sfx, noteSamples);
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
        float songPosition = TimeHelper.GetSongPosMS();

        float startTime = note.hitTime;
        float noteOffset = startTime - songPosition;
        if(noteOffset > Plugin.MaxScheduleOffset)
        {
            // It's too early to schedule this
            return;
        }

        float endTime = note.endTime;

        int sampleRate = AudioSettings.GetSampleRate();
        int mult = sampleRate / 1000;
        ulong startSamples = (ulong)(startTime * mult);
        ulong endSamples = (ulong)(endTime * mult);

        ScheduledHold newHold = FMODHelper.ScheduleHold(sfx, startSamples, endSamples);
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
        const float MinTimeOffsetToDispose = 500f;
        float songPosition = TimeHelper.GetSongPosMS();

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


    public static int CountRegisteredNotes()
    {
        int registeredCount = 0;
        for(byte id = 0; id < PlayedSounds.Length; id++)
        {
            foreach(KeyValuePair<BaseNote, ScheduledSound> pair in PlayedSounds[id])
            {
                registeredCount++;
            }
        }
        foreach(KeyValuePair<BaseNote, ScheduledHold> pair in PlayedHolds)
        {
            registeredCount++;
        }

        Plugin.Logger.LogInfo($"registered count: {registeredCount}");
        return registeredCount;
    }


    public static void UnregisterNote(BaseNote note, byte id = 0)
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
    public ulong startSamples;
}


public struct ScheduledHold
{
    public EventInstance sound;
    public float endTime;
    public ulong startSamples;
    public ulong endSamples;
}