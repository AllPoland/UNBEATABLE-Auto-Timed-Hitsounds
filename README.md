# Auto-Timed Hitsounds
 
A work-in-progress mod for UNBEATABLE which changes hitsounds to play at the exact moment they are mapped to, instead of when the player presses a button.

# What's this do?

This mod replaces the hitsound behavior of UNBEATABLE so that it no longer relies on your button presses. Instead, hitsounds are scheduled ahead of time to play at the exact moment that each note was meant to be hit.

Special notes still adapt to your inputs to some degree - hold note sounds still stop when you let go, dodge notes are still muted if you were already positioned to avoid them, and spam notes still play sounds for your spamming. Some behavior is a *little* different from vanilla because of the fact that this mod needs to predict the future to some extent.

## Why do this?

I mean, why do anything, really?

I made this mod mainly for personal use, since I find button-based hitsounds to be unbearable in any rhythm game. The reasons for that being:
1. Button-based hitsounds mask the actual intended rhythm whenever you get a bit off, making it harder to get back on track
2. Button-based hitsounds give no telegraph for what rhythm was *actually* mapped, exacerbating issues like overmapping and inconsistent rhythm choice
3. Button-based hitsounds *always* have some amount of latency compared to your inputs, no matter your offsets or setup, while automatic hitsounds are *always* on time, no matter your offsets or setup
4. Automatic hitsounds sound better because they actually line up to the music
5. Automatic hitsounds feel better by making you sound more on-beat even if you're way off (while also helping you actually stay on beat!)

# Installation

- Download [BepInEx 5.4 or later](https://github.com/BepInEx/BepInEx/releases) (Make sure you grab the win_x64 version!!!) and extract the files into your UNBEATABLE game folder.
  - FOR LINUX USERS: BepInEx needs an extra setup step to run through Proton. Instructions for this can be found [here](https://docs.bepinex.dev/articles/advanced/proton_wine.html)

- Download the latest mod release from the [releases page](releases/latest) and extract to your game folder.

# TODO

This mod is far from done!! Currently, the mod is only mostly complete for use in arcade mode. Below are some things I still need to do.

- Support for brawl notes
- Story mode testing
- Config options (toggling features, adjusting scheduling values, etc.)
