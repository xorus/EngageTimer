# EngageTimer

This plugin for the [FFXIV plugin platform Dalamud](https://github.com/goatcorp/FFXIVQuickLauncher)
can display a countdown on screen in the style of the original one, plus a stopwatch tracking the combat duration.

The original motivation for this plugin was to improve on the
original ["Acurate Countdown" plugin by Haplo064](https://github.com/Haplo064/Europe)
and make it synchronize somehow with my OBS recording setup.

## Countdown

Adds the missing numbers in the standard countdown thing!

You can also enable the "ticking" sound if you want.

![Countdown example](Doc/countdown.gif)

## Stopwatch

![Stopwatch example](Doc/stopwatch.gif)

Super simple combat timer you can place anywhere on your screen with optional tenths of seconds for some kinds of nerds.
Kinda customizable.

It can also display countdowns in a more precise manner.

## OBS overlay

The plugin lets you enable a web server that can be used to display your current timer information in a more accurate
way.

Alternatively you can access this web page from another device and see the timer thing.

![Web page](Doc/overlay.png)

In OBS as a browser source :

![OBS Setup](Doc/OBS.png)

## Other things I need to say

I have based myself on the work of ["Acurate Countdown" plugin by Haplo064](https://github.com/Haplo064/Europe)
for the countdown pointer and on the [Peeping Tom plugin by asclemens](https://git.sr.ht/~jkcclemens/PeepingTom) for the
idea of using NAudio lib to play the sound.

Smooth font size was adapted from ["Ping Plugin" by karashiiro](https://github.com/karashiiro/PingPlugin) 
