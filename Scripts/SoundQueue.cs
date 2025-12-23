using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

namespace AutoTimedHitsounds;


public class SoundQueue<T>
{
    public Dictionary<T, ScheduledSound>[] PlayedSounds = [[],[]];
    public Dictionary<T, ScheduledSound>[] ScheduledSounds = [[],[]];

    public Dictionary<T, ScheduledHold> PlayedHolds = new Dictionary<T, ScheduledHold>();
    public Dictionary<T, ScheduledHold> ScheduledHolds = new Dictionary<T, ScheduledHold>();


    public void ScheduleNote(T note, EventReference sfx, float noteTime, float endTime, byte id = 0)
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


    public void ScheduleNote(T note, float hitTime, EventReference sfx, byte id = 0)
    {
        // Note and song time are stored in milliseconds
        float noteTime = hitTime;
        ScheduleNote(note, sfx, noteTime, noteTime, id);
    }


    public void ScheduleHold(T note, float startTime, float endTime, EventReference sfx)
    {
        float songPosition = TimeHelper.GetSongPosMS();

        float noteOffset = startTime - songPosition;
        if(noteOffset > Plugin.MaxScheduleOffset)
        {
            // It's too early to schedule this
            return;
        }

        int sampleRate = AudioSettings.GetSampleRate();
        int mult = sampleRate / 1000;
        ulong startSamples = (ulong)(startTime * mult);
        ulong endSamples = (ulong)(endTime * mult);

        ScheduledHold newHold = FMODHelper.ScheduleHold(sfx, startSamples, endSamples);
        newHold.endTime = endTime;

        ScheduledHolds.Add(note, newHold);
    }


    public bool ShouldNoteSchedule(T note, byte id = 0)
    {
        return !ScheduledSounds[id].ContainsKey(note) && !PlayedSounds[id].ContainsKey(note);
    }


    public bool ShouldHoldSchedule(T note)
    {
        return !ScheduledHolds.ContainsKey(note) && !PlayedHolds.ContainsKey(note);
    }


    public void UpdateScheduledSounds()
    {
        for(byte id = 0; id < ScheduledSounds.Length; id++)
        {
            List<T> finishedNotes = new List<T>();
            foreach(KeyValuePair<T, ScheduledSound> pair in ScheduledSounds[id])
            {
                if(FMODHelper.TryPlaySound(pair.Value))
                {
                    // This sound is now good to go
                    PlayedSounds[id].Add(pair.Key, pair.Value);
                    finishedNotes.Add(pair.Key);
                }
            }

            foreach(T note in finishedNotes)
            {
                ScheduledSounds[id].Remove(note);
            }
        }
    }


    public void UpdateScheduledHolds()
    {
        List<T> finishedHolds = new List<T>();
        foreach(KeyValuePair<T, ScheduledHold> pair in ScheduledHolds)
        {
            if(FMODHelper.TryPlayHoldSound(pair.Value))
            {
                // This sound is now good to go
                PlayedHolds.Add(pair.Key, pair.Value);
                finishedHolds.Add(pair.Key);
            }
        }

        foreach(T note in finishedHolds)
        {
            ScheduledHolds.Remove(note);
        }
    }


    public void DisposeOldSounds()
    {
        const float MinTimeOffsetToDispose = 500f;
        float songPosition = TimeHelper.GetSongPosMS();

        for(byte id = 0; id < PlayedSounds.Length; id++)
        {
            List<T> notesToDispose = new List<T>();
            foreach(KeyValuePair<T, ScheduledSound> pair in PlayedSounds[id])
            {
                float noteTime = pair.Value.endTime;
                float timeOffset = songPosition - noteTime;
                if(timeOffset >= MinTimeOffsetToDispose)
                {
                    notesToDispose.Add(pair.Key);
                }
            }

            foreach(T note in notesToDispose)
            {
                UnregisterNote(note, id);
            }
        }

        List<T> holdsToDispose = new List<T>();
        foreach(KeyValuePair<T, ScheduledHold> pair in PlayedHolds)
        {
            float endTime = pair.Value.endTime;
            float timeOffset = songPosition - endTime;
            if(timeOffset >= MinTimeOffsetToDispose)
            {
                holdsToDispose.Add(pair.Key);
            }
        }

        foreach(T note in holdsToDispose)
        {
            UnregisterHold(note);
        }
    }


    public int CountRegisteredNotes()
    {
        int registeredCount = 0;
        for(byte id = 0; id < PlayedSounds.Length; id++)
        {
            foreach(KeyValuePair<T, ScheduledSound> pair in PlayedSounds[id])
            {
                registeredCount++;
            }
        }
        foreach(KeyValuePair<T, ScheduledHold> pair in PlayedHolds)
        {
            registeredCount++;
        }
        return registeredCount;
    }


    public void UnregisterNote(T note, byte id = 0)
    {
        ScheduledSounds[id].Remove(note);
        if(PlayedSounds[id].ContainsKey(note))
        {
            PlayedSounds[id][note].sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            PlayedSounds[id].Remove(note);
        }
    }


    public void UnregisterHold(T hold)
    {
        ScheduledHolds.Remove(hold);
        if(PlayedHolds.ContainsKey(hold))
        {
            PlayedHolds[hold].sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            PlayedHolds.Remove(hold);
        }
    }


    public void UnregisterAllSounds()
    {
        for(byte id = 0; id < ScheduledSounds.Length; id++)
        {
            ScheduledSounds[id].Clear();
            foreach(KeyValuePair<T, ScheduledSound> pair in PlayedSounds[id])
            {
                pair.Value.sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
            PlayedSounds[id].Clear();
        }

        ScheduledHolds.Clear();
        foreach(KeyValuePair<T, ScheduledHold> pair in PlayedHolds)
        {
            pair.Value.sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        PlayedHolds.Clear();
    }
}