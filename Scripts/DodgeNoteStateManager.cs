using System.Collections.Generic;
using Rhythm;

namespace AutoTimedHitsounds;

public static class DodgeNoteStateManager
{
    public static Dictionary<DodgeNote, DodgeNoteState> States = [];


    public static DodgeNoteState GetState(DodgeNote note)
    {
        if(States.TryGetValue(note, out DodgeNoteState state))
        {
            return state;
        }
        
        state = new DodgeNoteState();
        States[note] = state;
        return state;
    }


    public static void UnregisterNote(DodgeNote note)
    {
        States.Remove(note);
    }
}


public struct DodgeNoteState
{
    public bool markedForSound;

    public DodgeNoteState()
    {
        markedForSound = false;
    }
}