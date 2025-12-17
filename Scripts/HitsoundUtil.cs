using System.Collections.Generic;
using Rhythm;
using UnityEngine;

namespace AutoTimedHitsounds;

public static class HitsoundUtil
{
    public static List<HoldNote> GetActiveHoldNotes()
    {
        List<HoldNote> holdNotes = new List<HoldNote>();
        foreach(KeyValuePair<BaseNote, ScheduledHold> pair in HitsoundManager.ScheduledHolds)
        {
            HoldNote holdNote = pair.Key as HoldNote;
            if(holdNote != null)
            {
                holdNotes.Add(holdNote);
            }
        }
        foreach(KeyValuePair<BaseNote, ScheduledHold> pair in HitsoundManager.PlayedHolds)
        {
            HoldNote holdNote = pair.Key as HoldNote;
            if(holdNote != null)
            {
                holdNotes.Add(holdNote);
            }
        }
        return holdNotes;
    }


    public static bool UseAssistSound(BaseNote note, Height height, float hitTime)
    {
        RhythmPlayer player = note.controller.player;

        if(player.heldTop && height == Height.Top)
        {
            return false;
        }
        if(player.heldLow && height == Height.Low)
        {
            return false;
        }

        List<HoldNote> holdNotes = GetActiveHoldNotes();
        foreach(HoldNote holdNote in holdNotes)
        {
            Height holdHeight = holdNote.height;

            if(holdHeight == height)
            {
                // This hold is the same height as our note, so it wouldn't cause an assist
                continue;
            }
            
            // After becoming stunned, the hold's hitTime is set to endTime (thereby setting its length to zero)
            // This is jank but these are the things we do when the state is private
            bool isHeld = Mathf.Abs(holdNote.lengthTime) < Mathf.Epsilon;
            if(isHeld)
            {
                // Make sure the player is actually holding this note
                if(player.heldTop && holdHeight != Height.Top)
                {
                    continue;
                }
                if(player.heldLow && holdHeight != Height.Low)
                {
                    continue;
                }

                if(holdNote.endTime >= hitTime + Mathf.Epsilon)
                {
                    return true;
                }
            }
            else if(holdNote.hitTime <= hitTime - Mathf.Epsilon && holdNote.endTime >= hitTime + Mathf.Epsilon)
            {
                // The hold isn't *currently* held, but it will be by the time we hit this note
                return true;
            }
        }

        return false;
    }
}