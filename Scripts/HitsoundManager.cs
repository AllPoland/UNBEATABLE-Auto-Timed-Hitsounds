using FMOD.Studio;
using Rhythm;

namespace AutoTimedHitsounds;

public static class HitsoundManager
{
    public static SoundQueue<BaseNote> BaseQueue = new SoundQueue<BaseNote>();
    public static SoundQueue<BrawlNote.Attack> BrawlQueue = new SoundQueue<BrawlNote.Attack>();


    public static void UpdateScheduledSounds()
    {
        BaseQueue.UpdateScheduledSounds();
        BaseQueue.UpdateScheduledHolds();

        BrawlQueue.UpdateScheduledSounds();
        BrawlQueue.UpdateScheduledHolds();
    }


    public static void DisposeOldSounds()
    {
        BaseQueue.DisposeOldSounds();
        BrawlQueue.DisposeOldSounds();
    }


    public static int CountRegisteredNotes()
    {
        int registeredCount = BaseQueue.CountRegisteredNotes();
        registeredCount += BrawlQueue.CountRegisteredNotes();

        Plugin.Logger.LogInfo($"registered count: {registeredCount}");
        return registeredCount;
    }


    public static void UnregisterAllSounds()
    {
        BaseQueue.UnregisterAllSounds();
        BrawlQueue.UnregisterAllSounds();
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