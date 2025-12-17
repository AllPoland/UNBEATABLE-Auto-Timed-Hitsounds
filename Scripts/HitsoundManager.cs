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

    public static List<ScheduledHold> PlayedHolds = new List<ScheduledHold>();
    public static List<ScheduledHold> ScheduledHolds = new List<ScheduledHold>();

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


    public static void ScheduleHold(HoldNote note, EventReference sfx)
    {
        SongInstance.getTimelinePosition(out int songPosition);

        float startTime = note.hitTime;
        float noteOffset = startTime - songPosition;
        if(noteOffset > Plugin.MaxScheduleOffset || noteOffset < Plugin.MinScheduleOffset)
        {
            // It's either too late or too early to schedule this
            // Plugin.Logger.LogInfo($"song time: {songPosition}, note time: {noteTime}, offset: {noteOffset}");
            return;
        }

        float endTime = note.endTime;

        int sampleRate = AudioSettings.GetSampleRate();
        ulong songSamples = (ulong)(songPosition / 1000f * sampleRate);
        ulong startSamples = (ulong)(startTime / 1000f * sampleRate);
        ulong endSamples = (ulong)(endTime / 1000f * sampleRate);

        ScheduledHold newHold = FMODHelper.ScheduleHold(note, sfx, songSamples, startSamples, endSamples);
        ScheduledHolds.Add(newHold);
    }


    public static bool ShouldNoteSchedule(BaseNote note, byte id = 0)
    {
        return !ScheduledNotes.Any(x => x.note == note && x.id == id) && !PlayedNotes.Any(x => x.note == note && x.id == id);
    }


    public static bool ShouldHoldSchedule(HoldNote note)
    {
        return !ScheduledHolds.Any(x => x.note == note) && !PlayedHolds.Any(x => x.note == note);
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


    public static void UpdateScheduledHolds()
    {
        for(int i = ScheduledHolds.Count - 1; i >= 0; i--)
        {
            ScheduledHold hold = ScheduledHolds[i];
            if(FMODHelper.TryPlayHoldSound(hold))
            {
                // This sound is now good to go
                PlayedHolds.Add(hold);
                ScheduledHolds.RemoveAt(i);
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


public struct ScheduledHold
{
    public HoldNote note;
    public EventInstance sound;
    public ulong delaySamples;
    public ulong endDelaySamples;
}