using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using Rhythm;
using UnityEngine;

namespace AutoTimedHitsounds;

public static class HitsoundManager
{
    public static List<ScheduledNote> PlayedNotes = new List<ScheduledNote>();
    public static List<ScheduledNote> ScheduledNotes = new List<ScheduledNote>();

    public static EventInstance SongInstance;


    public static void ScheduleNote(BaseNote note, EventReference sfx, float noteTime, byte id = 0)
    {
        SongInstance.getTimelinePosition(out int songPosition);

        float noteOffset = noteTime - songPosition;
        if(noteOffset > Plugin.MaxScheduleOffset || noteOffset < Plugin.MinScheduleOffset)
        {
            // It's either too late or too early to schedule this
            // Plugin.Logger.LogInfo($"song time: {songPosition}, note time: {noteTime}, offset: {noteOffset}");
            return;
        }

        int sampleRate = AudioSettings.GetSampleRate();
        ulong songSamples = (ulong)(songPosition / 1000f * sampleRate);
        ulong noteSamples = (ulong)(noteTime / 1000f * sampleRate);

        ScheduledNote newNote = FMODHelper.ScheduleSound(note, sfx, songSamples, noteSamples, id);

        ScheduledNotes.Add(newNote);
    }


    public static void ScheduleNote(BaseNote note, EventReference sfx, byte id = 0)
    {
        // Note and song time are stored in milliseconds
        float noteTime = note.hitTime;
        ScheduleNote(note, sfx, noteTime, id);
    }


    public static bool ShouldNoteSchedule(BaseNote note, byte id = 0)
    {
        return !ScheduledNotes.Any(x => x.note == note && x.id == id) && !PlayedNotes.Any(x => x.note == note && x.id == id);
    }


    public static void UpdateScheduledNotes()
    {
        for(int i = ScheduledNotes.Count - 1; i >= 0; i--)
        {
            ScheduledNote note = ScheduledNotes[i];
            if(FMODHelper.TryPlaySound(note))
            {
                // This sound is now good to go
                PlayedNotes.Add(note);
                ScheduledNotes.RemoveAt(i);
            }
        }
    }
}


public struct ScheduledNote
{
    public BaseNote note;
    public EventInstance sound;
    public ulong delaySamples;
    public byte id;
}