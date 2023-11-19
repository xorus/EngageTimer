# v2.3.0.0 - unreleased

- Big code rewrite and a bit of optimization
- Fix save errors when spinning color sliders like a maniac in configuration
- Reorganized configuration file to preserve my sanity
- "Hide original addon" now uses AddonLifecycle events instead of searching for the original countdown every frame
- Reduce CountdownHook CPU usage by fixing a stupid event spam mistake
- Optimize countdown display code
- Fix the "first-draw" workaround that draws the countdown window once on plugin activation so it does not the micro
  freeze caused by ImGUI initializing the window does not occur when starting a countdown 

# v2.2.6.0

2023-10-04 - API9

- Countdown Ticks: option to make them start at a certain time (e.g. only tick 1 to 10 numbers)
- Updated to api 9

# v2.2.5.6

2023-07-29 - API8

- Fix font loading on non-global client
- Add chinese translation by @yuwuhuo via Crowdin

# v2.2.5.1

2023-02-04

- awkded new number styles
- add setting for display threshold: allows you to specify when the timer will become visible (can be useful if you
  don't want to see numbers irrelevant to your pre-pull)

# v2.2.5.0

2023-02-02

- add setting for display threshold: allows you to specify when the timer will become visible (can be useful if you
  don't want to see numbers irrelevant to your pre-pull)

# v2.2.4.0

2022-09-09

- Floating Window: option to change the countdown color when casting a spell that will result in a pre-pull.
- Floating Window: uses the global font-scale, you might need to adjust the font size after the update.
- Settings: support for closing the settings window with Escape (WindowSystem features)

# v2.2.2.1

2022-06-20

- added a swell `/eg` command to enable/disable/toggle the countdown/window/info bar (example `/eg countdown off`)
- fix lag when starting a countdown for the first time after loading the plugin (will play a cursor noise when enabling
  the plugin, I couldn't find a way around it yet)
- updated French and German translation thanks to contributors on [crowdin](https://crwd.in/engagetimer)
- internal reworking and optimizations