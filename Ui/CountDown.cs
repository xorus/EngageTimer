using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Threading;
using Dalamud.Game.Gui;
using Dalamud.Interface.Animation;
using Dalamud.Interface.Animation.EasingFunctions;
using Dalamud.Logging;
using EngageTimer.UI.CustomEasing;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using NAudio.Wave;

namespace EngageTimer.UI
{
    public class CountDown
    {
        private readonly Configuration _configuration;
        private readonly State _state;
        private int _lastNumberPlayed;
        private readonly GameGui _gui;
        private readonly NumberTextures _numberTextures;
        private readonly string _path;
        private bool _accurateMode;

        public const string WindowName = "EngageTimer Countdown";

        public CountDown(Configuration configuration, State state, GameGui gui, NumberTextures numberTextures,
            string path)
        {
            _configuration = configuration;
            _state = state;
            _gui = gui;
            _numberTextures = numberTextures;
            _path = path;
            configuration.OnSave += ConfigurationOnOnSave;
            UpdateFromConfig();
        }

        private void ConfigurationOnOnSave(object? sender, EventArgs e)
        {
            UpdateFromConfig();
        }

        /**
         * Things I need to simplify/re-use in this class but dont need to compute every frame
         */
        private void UpdateFromConfig()
        {
            _accurateMode = _configuration.HideOriginalCountdown && _configuration.CountdownAccurateCountdown;
        }

        private const byte VisibleFlag = 0x20;
        private bool _originalAddonHidden;

        // finds the original CountDown addon and toggles its visibility flag
        private unsafe void ToggleOriginalAddon()
        {
            var addon = _gui.GetAddonByName("ScreenInfo_CountDown", 1);
            if (addon == IntPtr.Zero) return;

            try
            {
                var atkUnitBase = (AtkUnitBase*)addon;
                atkUnitBase->Flags ^= VisibleFlag;
                _originalAddonHidden = (atkUnitBase->Flags & VisibleFlag) == 0;
            }
            catch (Exception)
            {
                // invalid pointer, don't care and carry on
            }
        }

        private const float BaseNumberScale = 1f;
        private const int GameCountdownWidth = 60; // just trust in the magic numbers

        private readonly Easing _easing = new OutCubic(new TimeSpan(0, 0, 0, 0, 1000));

        private readonly Easing _easingOpacity = new OpacityEasing(
            new TimeSpan(0, 0, 0, 0, 1000),
            1, -0.02, .71, 1
        );

        private int _lastSecond;

        public static bool ResetWindow { get; set; } = false;

        public static bool ShowBackground { get; set; } = false;
        private const float AnimationSize = .7f;
        private bool _wasInMainViewport = true;

        public void Draw()
        {
            if (_state.CountingDown && _configuration.EnableTickingSound && _state.CountDownValue > 5 && !_state.Mocked)
                TickSound((int)Math.Ceiling(_state.CountDownValue));


            // display is disabled
            if (!_configuration.DisplayCountdown)
                return;

            if (!_state.CountingDown || !_configuration.DisplayCountdown)
            {
                // re-enable the original addon at the last possible moment (when done counting down) to show "START"
                if (this._originalAddonHidden && _configuration.HideOriginalCountdown) this.ToggleOriginalAddon();
                return;
            }


            if (_configuration.HideOriginalCountdown && _state.CountDownValue <= 5 && !this._originalAddonHidden)
                this.ToggleOriginalAddon();

            var showMainCountdown = _state.CountDownValue > 5 || _configuration.HideOriginalCountdown;

            var numberScale = BaseNumberScale;
            var maxNumberScale = numberScale;
            if (showMainCountdown)
            {
                numberScale *= _configuration.CountdownScale;
                maxNumberScale = numberScale;
                // numberScale += (_state.CountDownValue % 1) * 0.7f;

                if (_configuration.CountdownAnimate)
                {
                    var second = (int)_state.CountDownValue;
                    if (_lastSecond != second)
                    {
                        _easing.Restart();
                        _easingOpacity.Restart();
                        _lastSecond = second;
                    }

                    _easing.Update();
                    _easingOpacity.Update();
                    if (_configuration.CountdownAnimateScale)
                    {
                        maxNumberScale = numberScale + AnimationSize;
                        numberScale += AnimationSize * (1 - (float)_easing.Value);
                    }
                }
            }

            var negativeMargin = _configuration.CountdownCustomNegativeMargin ?? (
                _configuration.CountdownMonospaced
                    ? _numberTextures.NumberNegativeMarginMono
                    : _numberTextures.NumberNegativeMargin
            ) * numberScale;

            var io = ImGui.GetIO();
            var windowPosition = new Vector2(io.DisplaySize.X * 0.5f, io.DisplaySize.Y * 0.5f) +
                                 _configuration.CountdownWindowOffset;
            var windowSize = new Vector2(
                maxNumberScale * (_numberTextures.MaxTextureWidth *
                                  (_configuration.EnableCountdownDecimal
                                      ? 3f + _configuration.CountdownDecimalPrecision * .5f
                                      : 2
                                  )),
                _numberTextures.MaxTextureHeight * maxNumberScale + 30);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGui.SetNextWindowPos(windowPosition, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

            var flags = /*ImGuiWindowFlags.AlwaysAutoResize
                        | ImGuiWindowFlags.NoResize
                        |*/ ImGuiWindowFlags.NoTitleBar
                            | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar
                            | ImGuiWindowFlags.NoDecoration
                            | ImGuiWindowFlags.NoFocusOnAppearing
                            | ImGuiWindowFlags.NoNavFocus
                            | ImGuiWindowFlags.NoInputs
                            // | ImGuiWindowFlags.NoBackground
                            | ImGuiWindowFlags.NoMouseInputs
                ;

            if (_wasInMainViewport) flags |= ImGuiWindowFlags.NoBackground;

            var visible = true;
            if (ImGui.Begin(WindowName, ref visible, flags))
            {
                _wasInMainViewport = ImGui.GetWindowViewport().ID == ImGui.GetMainViewport().ID;
                if (ShowBackground)
                {
                    var d = ImGui.GetBackgroundDrawList();
                    d.AddRect(
                        ImGui.GetWindowPos(),
                        ImGui.GetWindowPos() + ImGui.GetWindowSize(),
                        ImGui.GetColorU32(ImGuiCol.Text), 0f, ImDrawFlags.None,
                        7f + ((float)Math.Sin(ImGui.GetTime() * 2) * 5f));
                    ShowBackground = false;
                }

                DrawCountdown(io, showMainCountdown, numberScale, negativeMargin, false);
                if (_configuration.CountdownAnimate && _configuration.CountdownAnimateOpacity)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, (float)_easingOpacity.Value);
                    DrawCountdown(io, showMainCountdown, numberScale, negativeMargin, true);
                    ImGui.PopStyleVar();
                }
            }

            ImGui.PopStyleVar();
            ImGui.End();
        }

        private void DrawCountdown(ImGuiIOPtr io, bool showMainCountdown, float numberScale, float negativeMarginScaled,
            bool alternateMode)
        {
            var displaySize = ImGui.GetWindowSize();

            var totalHeight = _numberTextures.MaxTextureHeight * numberScale;
            ImGui.SetCursorPosY((displaySize.Y - totalHeight) / 2f);

            if (showMainCountdown)
            {
                var number = _accurateMode
                    ? Math.Floor(_state.CountDownValue).ToString(CultureInfo.InvariantCulture)
                    : Math.Ceiling(_state.CountDownValue).ToString(CultureInfo.InvariantCulture);

                if (_configuration.CountdownLeadingZero && number.Length == 1) number = "0" + number;

                var integers = NumberList(number);
                // First loop to compute total width
                var totalWidth = 0f;
                if (_configuration.CountdownMonospaced)
                {
                    totalWidth = (_numberTextures.MaxTextureWidth * numberScale - negativeMarginScaled) *
                                 integers.Count;
                }
                else
                {
                    foreach (var i in integers)
                    {
                        var texture = _numberTextures.GetAltTexture(i);
                        totalWidth += texture.Width * numberScale - negativeMarginScaled;
                    }
                }

                totalWidth += negativeMarginScaled;

                // Center the cursor
                ImGui.SetCursorPosX(displaySize.X / 2f - totalWidth / 2f);

                // Draw the images \o/
                foreach (var i in integers)
                {
                    DrawNumber(alternateMode, i,
                        numberScale, negativeMarginScaled,
                        _numberTextures.MaxTextureWidth * numberScale, _configuration.CountdownMonospaced);
                }
            }
            else if (_configuration.EnableCountdownDecimal)
            {
                ImGui.SetCursorPosX(displaySize.X / 2f + GameCountdownWidth);
            }

            if (_configuration.EnableCountdownDecimal)
            {
                var decimalPart =
                    (_state.CountDownValue - Math.Truncate(_state.CountDownValue))
                    .ToString("F" + _configuration.CountdownDecimalPrecision, CultureInfo.InvariantCulture)[2..];
                var smolNumberScale = numberScale * .5f;
                var smolMaxWidthScaled = _numberTextures.MaxTextureWidth * smolNumberScale;

                // align the small numbers on the number baseline
                var offsetY = _numberTextures.MaxTextureHeight * numberScale
                              - _numberTextures.MaxTextureHeight * smolNumberScale
                              - _numberTextures.NumberBottomMargin * smolNumberScale;

                var cursorY = ImGui.GetCursorPosY() + offsetY;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + negativeMarginScaled);
                foreach (var i in NumberList(decimalPart))
                {
                    ImGui.SetCursorPosY(cursorY);
                    // small numbers are always fixed width
                    DrawNumber(alternateMode, i, smolNumberScale,
                        negativeMarginScaled * smolNumberScale, smolMaxWidthScaled, true);
                }
            }
        }

        private void DrawNumber(bool alternateMode, int i, float numberScale, float negativeMarginScaled,
            float maxWidthScaled,
            bool fixedWidth)
        {
            var texture = alternateMode ? _numberTextures.GetAltTexture(i) : _numberTextures.GetTexture(i);
            var width = texture.Width * numberScale;
            var cursorX = ImGui.GetCursorPosX();
            if (fixedWidth)
            {
                ImGui.SetCursorPosX(cursorX + (maxWidthScaled - width) / 2);
            }

            ImGui.Image(texture.ImGuiHandle,
                new Vector2(texture.Width * numberScale, texture.Height * numberScale));
            ImGui.SameLine();

            if (fixedWidth)
            {
                ImGui.SetCursorPosX(cursorX + maxWidthScaled - negativeMarginScaled);
            }
            else
            {
                ImGui.SetCursorPosX(cursorX + width - negativeMarginScaled);
            }
        }

        private static List<int> NumberList(string number)
        {
            var integers = new List<int>();
            foreach (var c in number)
            {
                int i;
                if (int.TryParse(c.ToString(), out i)) integers.Add(i);
            }

            return integers;
        }

        /**
         * https://git.sr.ht/~jkcclemens/PeepingTom
         */
        private void TickSound(int n)
        {
            if (!_configuration.EnableTickingSound || _lastNumberPlayed == n)
                return;
            _lastNumberPlayed = n;

            new Thread(() =>
            {
                WaveStream reader;
                try
                {
                    reader = new WaveFileReader(Path.Combine(_path, "Data", "tick.wav"));
                }
                catch (Exception e)
                {
                    PluginLog.Log($"Could not play sound file: {e.Message}");
                    return;
                }

                using WaveChannel32 channel = new(reader)
                {
                    Volume = _configuration.TickingSoundVolume,
                    PadWithZeroes = false
                };

                using (reader)
                {
                    using var output = new WaveOutEvent
                    {
                        DeviceNumber = -1
                    };
                    output.Init(channel);
                    output.Play();

                    while (output.PlaybackState == PlaybackState.Playing) Thread.Sleep(500);
                }
            }).Start();
        }
    }
}