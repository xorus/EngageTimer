using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Threading;
using Dalamud.Plugin;
using EngageTimer.Properties;
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

        public CountDown(Configuration configuration, State state)
        {
            _configuration = configuration;
            _state = state;
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

        public void Draw()
        {
            if (_state.CountingDown && _configuration.EnableTickingSound && _state.CountDownValue > 5)
                TickSound((int) Math.Ceiling(_state.CountDownValue));

            if (!_state.CountingDown || !_configuration.DisplayCountdown)
                return;

            const float baseNumberScale = 1f;
            const float numberScale = baseNumberScale;
            const int numberNegativeMargin = 10;
            const int gameCountdownWidth = 60; // yes, this number came from my arse

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
                if (_state.CountDownValue > 5)
                {
                    var number = Math.Ceiling(_state.CountDownValue).ToString(CultureInfo.InvariantCulture);
                    var integers = NumberList(number);

                    // First loop to compute total width
                    var totalWidth = 0f;
                    foreach (var i in integers)
                    {
                        var texture = _numberTextures[i];
                        totalWidth += texture.Width - numberNegativeMargin;
                    }

                    totalWidth += numberNegativeMargin;

                    // Center the cursor
                    ImGui.SetCursorPosX(io.DisplaySize.X / 2f - totalWidth / 2f);

                    // Draw the images \o/
                    foreach (var i in integers)
                    {
                        var texture = _numberTextures[i];
                        var cursorX = ImGui.GetCursorPosX();
                        ImGui.Image(texture.ImGuiHandle,
                            new Vector2(texture.Width * numberScale, texture.Height * numberScale));
                        ImGui.SameLine();
                        ImGui.SetCursorPosX(texture.Width + cursorX - numberNegativeMargin * numberScale);
                    }
                }
                else if (_configuration.EnableCountdownDecimal)
                {
                    ImGui.SetCursorPosX(io.DisplaySize.X / 2f + gameCountdownWidth);
                }

                if (_configuration.EnableCountdownDecimal)
                {
                    var decimalPart =
                        (_state.CountDownValue - Math.Truncate(_state.CountDownValue))
                        .ToString("F" + _configuration.CountdownDecimalPrecision, CultureInfo.InvariantCulture)
                        .Substring(2);
                    var smolNumberScale = numberScale * .5f;
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
                        ImGui.SetCursorPosX(cursorX + smolMaxWidthScaled - numberNegativeMargin * smolNumberScale);
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