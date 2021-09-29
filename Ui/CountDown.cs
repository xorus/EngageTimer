using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Threading;
using Dalamud.Game.Gui;
using Dalamud.Logging;
using Dalamud.Plugin;
using EngageTimer.Properties;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using ImGuiScene;
using NAudio.Wave;

namespace EngageTimer.UI
{
    public class CountDown
    {
        private readonly Configuration _configuration;
        private readonly State _state;
        private int _lastNumberPlayed;
        private readonly Dictionary<int, TextureWrap> _numberTextures = new();
        private int _windowHeight;
        private int _maxTextureWidth;
        private GameGui _gui;

        public CountDown(Configuration configuration, State state, GameGui gui)
        {
            _configuration = configuration;
            _state = state;
            _gui = gui;
        }

        public void Load(DalamudPluginInterface pluginInterface, string dataPath)
        {
            for (var i = 0; i < 10; i++)
            {
                var texture = pluginInterface.UiBuilder.LoadImage(
                    Path.Combine(dataPath, "Data", i + ".png")
                );
                _windowHeight = Math.Max(_windowHeight, texture.Height);
                _numberTextures.Add(i, texture);
                _maxTextureWidth = Math.Max(_maxTextureWidth, texture.Width);
            }
        }

        private const byte VisibleFlag = 0x20;
        private bool _originalAddonHidden = false;

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
        private const float NumberScale = BaseNumberScale;
        private const int NumberNegativeMargin = 10;
        private const int GameCountdownWidth = 60; // yes, this number came from my arse

        public void Draw()
        {
            if (_state.CountingDown && _configuration.EnableTickingSound && _state.CountDownValue > 5 && !_state.Mocked)
                TickSound((int)Math.Ceiling(_state.CountDownValue));

            // display is disabled
            if (!_configuration.DisplayCountdown)
                return;

            var hideCountdown = _configuration.HideOriginalCountdown;

            if (!_state.CountingDown || !_configuration.DisplayCountdown)
            {
                // re-enable the original addon at the last possible moment (when done counting down) to show "START"
                if (this._originalAddonHidden && hideCountdown) this.ToggleOriginalAddon();
                return;
            }


            if (hideCountdown && _state.CountDownValue <= 5 && !this._originalAddonHidden)
                this.ToggleOriginalAddon();

            var io = ImGui.GetIO();
            ImGui.SetNextWindowSize(new Vector2(io.DisplaySize.X, _windowHeight + 30), ImGuiCond.Always);
            ImGui.SetNextWindowPos(new Vector2(0, io.DisplaySize.Y * 0.5f), ImGuiCond.Always, new Vector2(0, 0.5f));
            const ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar
                                           | ImGuiWindowFlags.NoDecoration
                                           | ImGuiWindowFlags.NoScrollbar
                                           | ImGuiWindowFlags.NoInputs
                                           | ImGuiWindowFlags.NoBackground
                                           | ImGuiWindowFlags.NoMouseInputs
                                           | ImGuiWindowFlags.AlwaysAutoResize
                                           | ImGuiWindowFlags.NoResize;
            var visible = true;
            if (ImGui.Begin("EngageTimer Countdown", ref visible, flags))
            {
                if (_state.CountDownValue > 5 || hideCountdown)
                {
                    var number = (_configuration.CountdownAccurateCountdown && _originalAddonHidden)
                        ? Math.Floor(_state.CountDownValue).ToString(CultureInfo.InvariantCulture)
                        : Math.Ceiling(_state.CountDownValue).ToString(CultureInfo.InvariantCulture);

                    var integers = NumberList(number);

                    // First loop to compute total width
                    var totalWidth = 0f;
                    foreach (var i in integers)
                    {
                        var texture = _numberTextures[i];
                        totalWidth += texture.Width - NumberNegativeMargin;
                    }

                    totalWidth += NumberNegativeMargin;

                    // Center the cursor
                    ImGui.SetCursorPosX(io.DisplaySize.X / 2f - totalWidth / 2f);

                    // Draw the images \o/
                    foreach (var i in integers)
                    {
                        var texture = _numberTextures[i];
                        var cursorX = ImGui.GetCursorPosX();
                        ImGui.Image(texture.ImGuiHandle,
                            new Vector2(texture.Width * NumberScale, texture.Height * NumberScale));
                        ImGui.SameLine();
                        ImGui.SetCursorPosX(texture.Width + cursorX - NumberNegativeMargin * NumberScale);
                    }
                }
                else if (_configuration.EnableCountdownDecimal)
                {
                    ImGui.SetCursorPosX(io.DisplaySize.X / 2f + GameCountdownWidth);
                }

                if (_configuration.EnableCountdownDecimal)
                {
                    var decimalPart =
                        (_state.CountDownValue - Math.Truncate(_state.CountDownValue))
                        .ToString("F" + _configuration.CountdownDecimalPrecision, CultureInfo.InvariantCulture)
                        .Substring(2);
                    var smolNumberScale = NumberScale * .5f;
                    var smolMaxWidthScaled = _maxTextureWidth * smolNumberScale;
                    var cursorY = ImGui.GetCursorPosY();
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
                    foreach (var i in NumberList(decimalPart))
                    {
                        var texture = _numberTextures[i];
                        var cursorX = ImGui.GetCursorPosX();
                        var height = texture.Height * smolNumberScale;
                        ImGui.SetCursorPosY(cursorY + height);
                        ImGui.Image(texture.ImGuiHandle, new Vector2(texture.Width * smolNumberScale, height));
                        ImGui.SameLine();
                        ImGui.SetCursorPosX(cursorX + smolMaxWidthScaled - NumberNegativeMargin * smolNumberScale);
                    }
                }
            }

            ImGui.End();
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
                    reader = new WaveFileReader(Resources.Tick);
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