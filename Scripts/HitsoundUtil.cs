using System.Collections.Generic;
using Rhythm;
using UnityEngine;

namespace AutoTimedHitsounds;

public static class HitsoundUtil
{
    public struct BaseHold
    {
        public float hitTime;
        public float endTime;
        public float lengthTime;
        public Height height;
        public Side side;


        public BaseHold(HoldNote hold)
        {
            hitTime = hold.hitTime;
            endTime = hold.endTime;
            lengthTime = endTime - hitTime;
            height = hold.height;
            side = hold.side;
        }


        public BaseHold(BrawlNote.Attack hold)
        {
            hitTime = hold.hitTime;
            endTime = hold.endTime;
            lengthTime = endTime - hitTime;
            height = hold.height;
            side = hold.side;
        }
    }


    public static List<BaseHold> GetActiveHoldNotes()
    {
        List<BaseHold> holds = new List<BaseHold>();

        foreach(KeyValuePair<BaseNote, ScheduledHold> pair in HitsoundManager.BaseQueue.ScheduledHolds)
        {
            HoldNote holdNote = pair.Key as HoldNote;
            if(holdNote != null)
            {
                holds.Add(new BaseHold(holdNote));
            }
        }
        foreach(KeyValuePair<BaseNote, ScheduledHold> pair in HitsoundManager.BaseQueue.PlayedHolds)
        {
            HoldNote holdNote = pair.Key as HoldNote;
            if(holdNote != null)
            {
                holds.Add(new BaseHold(holdNote));
            }
        }

        foreach(KeyValuePair<BrawlNote.Attack, ScheduledHold> pair in HitsoundManager.BrawlQueue.ScheduledHolds)
        {
            BrawlNote.Attack attack = pair.Key;
            if(attack.response == BrawlNote.Attack.ResponseType.Hold)
            {
                holds.Add(new BaseHold(pair.Key));
            }
        }
        foreach(KeyValuePair<BrawlNote.Attack, ScheduledHold> pair in HitsoundManager.BrawlQueue.PlayedHolds)
        {
            BrawlNote.Attack attack = pair.Key;
            if(attack.response == BrawlNote.Attack.ResponseType.Hold)
            {
                holds.Add(new BaseHold(pair.Key));
            }
        }

        return holds;
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

        List<BaseHold> holdNotes = GetActiveHoldNotes();
        foreach(BaseHold hold in holdNotes)
        {
            Height holdHeight = hold.height;

            if(holdHeight == height)
            {
                // This hold is the same height as our note, so it wouldn't cause an assist
                continue;
            }

            if(hold.endTime < hitTime + Mathf.Epsilon)
            {
                continue;
            }
            
            // After becoming stunned, the hold's hitTime is set to endTime (thereby setting its length to zero)
            // This is jank but these are the things we do when the state is private
            bool isHeld = Mathf.Abs(hold.lengthTime) < Mathf.Epsilon;
            if(isHeld || hold.hitTime <= hitTime - Mathf.Epsilon)
            {
                return true;
            }
        }

        return false;
    }
}