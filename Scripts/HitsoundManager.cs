using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using Rhythm;

namespace AutoTimedHitsounds;

public static class HitsoundManager
{
    public static List<ScheduledNote> PlayedNotes = new List<ScheduledNote>();
    public static List<ScheduledNote> ScheduledNotes = new List<ScheduledNote>();


    public static void ScheduleNote(BaseNote note, EventReference sfx, float noteTime)
    {
        // Note and song time are stored in milliseconds
        noteTime /= 1000f;
        float songTime = note.songPosition / 1000f;

        // Plugin.Logger.LogInfo($"note time: {noteTime}, song time: {songTime}");
        ScheduledNote newNote = FMODHelper.ScheduleSound(note, sfx, songTime, noteTime);

        ScheduledNotes.Add(newNote);
    }


    public static void ScheduleNote(BaseNote note, EventReference sfx)
    {
        // Note and song time are stored in milliseconds
        float noteTime = note.hitTime;
        ScheduleNote(note, sfx, noteTime);
    }


    public static bool ShouldNoteSchedule(BaseNote note)
    {
        return !ScheduledNotes.Any(x => x.note == note) && !PlayedNotes.Any(x => x.note == note);
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
    public float delaySeconds;
}