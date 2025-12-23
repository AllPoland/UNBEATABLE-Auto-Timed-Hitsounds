using System.Collections.Generic;
using FMODUnity;
using HarmonyLib;
using Rhythm;

namespace AutoTimedHitsounds.Patches;

public class BrawlNotePatch
{
    private static void TryScheduleNormalSound(BrawlNote.Attack attack, float hitTime)
    {
        SoundQueue<BrawlNote.Attack> queue = HitsoundManager.BrawlQueue;
        if(!queue.ShouldNoteSchedule(attack))
        {
            return;
        }

        RhythmController controller = RhythmController.Instance;
        bool useAssistSound = HitsoundUtil.UseAssistSound(attack.height, attack.hitTime);
        EventReference sfx = useAssistSound ? controller.hitAssistSFX : controller.hitSFX;
        queue.ScheduleNote(attack, attack.hitTime, sfx);
    }


    private static void TryScheduleHoldSound(BrawlNote.Attack attack)
    {
        SoundQueue<BrawlNote.Attack> queue = HitsoundManager.BrawlQueue;
        if(!queue.ShouldHoldSchedule(attack))
        {
            return;
        }

        queue.ScheduleHold(attack, attack.hitTime, attack.endTime, RhythmController.Instance.holdSFX);
    }


    private static void UpdateAttacks(BrawlNote __instance, List<BrawlNote.Attack> attacks)
    {
        foreach(BrawlNote.Attack attack in attacks)
        {
            switch(attack.response)
            {
                case BrawlNote.Attack.ResponseType.Dodge:
                case BrawlNote.Attack.ResponseType.Spam:
                case BrawlNote.Attack.ResponseType.Parry:
                    TryScheduleNormalSound(attack, attack.hitTime);
                    break;
                case BrawlNote.Attack.ResponseType.Hold:
                    TryScheduleHoldSound(attack);
                    SoundQueue<BrawlNote.Attack> queue = HitsoundManager.BrawlQueue;
                    if(queue.ShouldNoteSchedule(attack))
                    {
                        queue.ScheduleNote(attack, __instance.controller.hitSFX, attack.endTime, attack.endTime);
                    }
                    break;
            }
        }
    }


    [HarmonyPatch(typeof(BrawlNote), "RhythmUpdate_Active")]
    [HarmonyPrefix]
    static bool RhythmUpdate_ActivePrefix(BrawlNote __instance)
    {
        UpdateAttacks(__instance, [.. __instance.attacks]);

        // Perform the original method
        return true;
    }


    [HarmonyPatch(typeof(BrawlNote), "RhythmUpdate_Stunned")]
    [HarmonyPrefix]
    static bool RhythmUpdate_StunnedPrefix(BrawlNote __instance)
    {
        BrawlNote.Attack currentAttack = __instance.attacks.Peek();
        switch(currentAttack.response)
        {
            case BrawlNote.Attack.ResponseType.Spam:
                if(__instance.IsSwungAt())
                {
                    RuntimeManager.PlayOneShot(__instance.controller.hitSFX);
                }
                break;
            case BrawlNote.Attack.ResponseType.Hold:
                SoundQueue<BrawlNote.Attack> queue = HitsoundManager.BrawlQueue;
                if(queue.ShouldNoteSchedule(currentAttack))
                {
                    queue.ScheduleNote(currentAttack, __instance.controller.hitSFX, currentAttack.endTime, currentAttack.endTime);
                }

                bool holding = __instance.player.input.AnyPressedAt(currentAttack.height);
                if(!holding && !__instance.WithinHitRange(currentAttack.endTime))
                {
                    // Why must you be a private field raaaaaaaaaaaah
                    Traverse traverse = Traverse.Create(__instance);
                    float giveTimer = traverse.Field("giveTimer").GetValue<float>();
                    if(giveTimer <= 0f)
                    {
                        // The hold note has been released early
                        queue.UnregisterHold(currentAttack);
                    }
                }
                break;
        }

        // Ignore the first note in the main update because we've already handled it here
        List<BrawlNote.Attack> attacks = [.. __instance.attacks];
        attacks.RemoveAt(0);
        UpdateAttacks(__instance, attacks);

        // Perform the original method
        return true;
    }
}