using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;

namespace AutoTimedHitsounds;

public class SoundQueue<T>
{
    public Dictionary<T, ScheduledSound>[] PlayedSounds = [[],[]];
    public Dictionary<T, ScheduledSound>[] ScheduledSounds = [[],[]];

    public Dictionary<T, ScheduledHold> PlayedHolds = new Dictionary<T, ScheduledHold>();
    public Dictionary<T, ScheduledHold> ScheduledHolds = new Dictionary<T, ScheduledHold>();


    public void ScheduleNote(T note, EventReference sfx, float noteTime, float endTime, byte id = 0)
    {
        // Use the input position for this part to make sure we schedule the sound before the note is in hit range
        float inputPosition = TimeHelper.GetInputPosMS();

        float noteOffset = noteTime - inputPosition;
        if(noteOffset > Plugin.MaxScheduleOffset)
        {
            // It's too early to schedule this
            return;
        }

        ulong noteSamples = TimeHelper.GetScheduleSamples(noteTime);

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
        // Use the input position for this part to make sure we schedule the sound before the note is in hit range
        float inputPosition = TimeHelper.GetInputPosMS();

        float noteOffset = startTime - inputPosition;
        if(noteOffset > Plugin.MaxScheduleOffset)
        {
            // It's too early to schedule this
            return;
        }

        ulong startSamples = TimeHelper.GetScheduleSamples(startTime);
        ulong endSamples = TimeHelper.GetScheduleSamples(endTime);

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
            EventInstance sound = PlayedSounds[id][note].sound;
            sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            sound.release();
            PlayedSounds[id].Remove(note);
        }
    }


    public void UnregisterHold(T hold)
    {
        ScheduledHolds.Remove(hold);
        if(PlayedHolds.ContainsKey(hold))
        {
            EventInstance sound = PlayedHolds[hold].sound;
            sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            sound.release();
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
                pair.Value.sound.release();
            }
            PlayedSounds[id].Clear();
        }

        ScheduledHolds.Clear();
        foreach(KeyValuePair<T, ScheduledHold> pair in PlayedHolds)
        {
            pair.Value.sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            pair.Value.sound.release();
        }
        PlayedHolds.Clear();
    }
}