using Dalamud.Plugin;
using ImGuiNET;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Threading;

namespace EngageTimer
{
    public class PluginUI
    {
        private DalamudPluginInterface pluginInterface;
        private Configuration configuration;
        private Dictionary<int, ImGuiScene.TextureWrap> numberTextures = new();

        private int windowHeight;

        public PluginUI(DalamudPluginInterface pluginInterface, Configuration configuration, string dataPath)
        {
            this.pluginInterface = pluginInterface;
            this.configuration = configuration;

            for (int i = 0; i < 10; i++)
            {
                var texture = pluginInterface.UiBuilder.LoadImage(
                    Path.Combine(dataPath, "Data", /*"number_" +*/ i + ".png")
                    );
                windowHeight = Math.Max(windowHeight, texture.Height);
                numberTextures.Add(i, texture);
            }

            CountDownValue = 0f;
        }


        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = true;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        private bool stopwatchVisible = false;

        public bool CountingDown { get; set; }
        public float CountDownValue { get; set; }

        private int lastNumberPlayed = 0;

        /**
         * https://git.sr.ht/~jkcclemens/PeepingTom
         **/
        private void TickSound(int n)
        {
            if (!this.configuration.EnableTickingSound || lastNumberPlayed == n)
                return;
            lastNumberPlayed = n;

            new Thread(() =>
            {
                WaveStream reader;
                try
                {
                    reader = new WaveFileReader(Properties.Resources.Tick);
                }
                catch (Exception e)
                {
                    PluginLog.Log($"Could not play sound file: {e.Message}");
                    return;
                }

                using WaveChannel32 channel = new(reader)
                {
                    Volume = this.configuration.TickingSoundVolume,
                    PadWithZeroes = false,
                };

                using (reader)
                {
                    using var output = new WaveOutEvent
                    {
                        DeviceNumber = -1,
                    };
                    output.Init(channel);
                    output.Play();

                    while (output.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(500);
                    }
                }
            }).Start();
        }

        private void DrawCountDown()
        {
            if (SettingsVisible)
            {
                DrawSettings();
            }

            if (CountingDown && configuration.EnableTickingSound && CountDownValue > 5)
            {
                TickSound((int)Math.Ceiling(CountDownValue));
            }

            if (!CountingDown || !configuration.DisplayCountdown)
                return;

            var baseNumberScale = 1f;
            var numberScale = baseNumberScale;
            const int numberNegativeMargin = 20;

            var io = ImGui.GetIO();
            ImGui.SetNextWindowSize(new Vector2(io.DisplaySize.X, windowHeight + 30), ImGuiCond.Always);
            ImGui.SetNextWindowPos(new Vector2(0, io.DisplaySize.Y * 0.5f), ImGuiCond.Always, new Vector2(0, 0.5f));
            if (ImGui.Begin("EngageTimer Countdown", ref this.visible, ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize))
            {

                if (CountDownValue > 5)
                {
                    //var numberScale = baseNumberScale + (CountDownValue % 1f) - 0.5f;
                    var number = Math.Ceiling(CountDownValue).ToString();

                    var integers = new List<int>();
                    foreach (char c in number)
                    {
                        int i;
                        if (int.TryParse(c.ToString(), out i))
                        {
                            integers.Add(i);
                        }
                    }

                    // First loop to compute total width
                    var totalWidth = 0f;
                    foreach (var i in integers)
                    {
                        var texture = numberTextures[i];
                        totalWidth += texture.Width - numberNegativeMargin;
                    }
                    totalWidth += numberNegativeMargin;

                    // Center the cursor
                    ImGui.SetCursorPosX(io.DisplaySize.X / 2f - (totalWidth / 2f));

                    // Draw the images \o/
                    foreach (var i in integers)
                    {
                        var texture = numberTextures[i];
                        ImGui.Image(texture.ImGuiHandle, new Vector2(texture.Width * numberScale, texture.Height * numberScale));
                        ImGui.SameLine();
                        var cursorX = ImGui.GetCursorPosX();
                        ImGui.SetCursorPosX(cursorX - (numberNegativeMargin * numberScale));
                    }
                }
            }
            ImGui.End();
        }

        public void Draw()
        {
            if (!Visible)
                return;

            this.DrawCountDown();
            this.DrawStopwatch();
        }

        public TimeSpan CombatDuration { get; set; } = new();
        public DateTime CombatEnd { get; set; } = new();
        public void DrawStopwatch()
        {
            if (!configuration.DisplayStopwatch)
                return;

            var autoHide = configuration.AutoHideStopwatch && (DateTime.Now - CombatEnd).TotalSeconds > configuration.AutoHideTimeout;
            var countdownMode = configuration.StopwatchCountdown && CountingDown;

            if (autoHide && !countdownMode)
                return;

            ImGui.SetNextWindowBgAlpha(configuration.StopwatchOpacity);

            var flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize;
            if (configuration.StopwatchLock)
            {
                flags = flags | ImGuiWindowFlags.NoMouseInputs;
            }

            if (ImGui.Begin("EngageTimer stopwatch", ref this.stopwatchVisible, flags))
            {
                ImGui.SetWindowFontScale(configuration.StopwatchScale);
                ImGui.PushStyleColor(ImGuiCol.Text, configuration.StopwatchColor);

                if (configuration.StopwatchCountdown && CountingDown && CountDownValue > 0)
                {
                    //if (configuration.StopwatchTenths)
                    ImGui.Text(String.Format("-{0:0.0}", CountDownValue));
                    //else
                    //ImGui.Text(String.Format("-{0:0}", CountDownValue));
                }
                else
                {
                    if (configuration.StopwatchTenths)
                        ImGui.Text(CombatDuration.ToString(@"mm\:ss\.f"));
                    else
                        ImGui.Text(CombatDuration.ToString(@"mm\:ss"));
                }
                ImGui.PopStyleColor();
                ImGui.SetWindowFontScale(1f);
            }
            ImGui.End();
        }

        public void DrawSettings()
        {
            if (ImGui.Begin("EngageTimer settings", ref this.settingsVisible, ImGuiWindowFlags.AlwaysAutoResize))
            {
                var enableTickingSound = configuration.EnableTickingSound;

                ImGui.PushItemWidth(100f);
                var displayCountdown = configuration.DisplayCountdown;
                if (ImGui.Checkbox("Display countdown", ref displayCountdown))
                {
                    configuration.DisplayCountdown = displayCountdown;
                    configuration.Save();
                }

                if (ImGui.Checkbox("Play a timer ticking sound", ref enableTickingSound))
                {
                    configuration.EnableTickingSound = enableTickingSound;
                    configuration.Save();
                }

                if (enableTickingSound)
                {
                    ImGui.Indent();
                    var volume = configuration.TickingSoundVolume * 100f;
                    if (ImGui.DragFloat("Sound volume", ref volume, .1f, 0f, 100f, "%.1f%%"))
                    {
                        configuration.TickingSoundVolume = Math.Max(0f, Math.Min(1f, volume / 100f));
                        configuration.Save();
                    }
                    ImGui.Unindent();
                }

                ImGui.Separator();

                var displayStopwatch = configuration.DisplayStopwatch;
                if (ImGui.Checkbox("Display stopwatch", ref displayStopwatch))
                {
                    configuration.DisplayStopwatch = displayStopwatch;
                    configuration.Save();
                }
                if (displayStopwatch)
                {
                    ImGui.Indent();
                    var stopwatchTenths = configuration.StopwatchTenths;
                    if (ImGui.Checkbox("Display tenths of seconds", ref stopwatchTenths))
                    {
                        configuration.StopwatchTenths = stopwatchTenths;
                        configuration.Save();
                    }

                    var stopwatchCountdown = configuration.StopwatchCountdown;
                    if (ImGui.Checkbox("Display countdown in stopwatch", ref stopwatchCountdown))
                    {
                        configuration.StopwatchCountdown = stopwatchCountdown;
                        configuration.Save();
                    }

                    var stopwatchLock = configuration.StopwatchLock;
                    if (ImGui.Checkbox("Lock stopwatch", ref stopwatchLock))
                    {
                        configuration.StopwatchLock = stopwatchLock;
                        configuration.Save();
                    }

                    var autoHideStopwatch = configuration.AutoHideStopwatch;
                    if (ImGui.Checkbox("Hide stopwatch after", ref autoHideStopwatch))
                    {
                        configuration.AutoHideStopwatch = autoHideStopwatch;
                        configuration.Save();
                    }
                    var autoHideTimeout = configuration.AutoHideTimeout;
                    ImGui.SameLine();
                    if (ImGui.InputFloat("seconds", ref autoHideTimeout, .1f, 1f, "%.1f%"))
                    {
                        configuration.AutoHideTimeout = Math.Max(0, autoHideTimeout);
                        configuration.Save();
                    }

                    if (ImGui.CollapsingHeader("Style"))
                    {
                        ImGui.Indent();
                        var stopwatchScale = configuration.StopwatchScale;
                        if (ImGui.SliderFloat("Scale", ref stopwatchScale, 0f, 10f))
                        {
                            configuration.StopwatchScale = Math.Max(1f, Math.Min(10f, stopwatchScale));
                            configuration.Save();
                        }

                        var stopwatchColor = configuration.StopwatchColor;
                        ImGui.PushItemWidth(300f);
                        if (ImGui.ColorEdit4("Text color", ref stopwatchColor))
                        {
                            configuration.StopwatchColor = stopwatchColor;
                            configuration.Save();
                        }
                        ImGui.PopItemWidth();

                        var stopwatchOpacity = configuration.StopwatchOpacity;
                        if (ImGui.SliderFloat("Background opacity", ref stopwatchOpacity, 0f, 1f))
                        {
                            configuration.StopwatchOpacity = stopwatchOpacity;
                            configuration.Save();
                        }
                        ImGui.Unindent();
                    }
                    ImGui.Unindent();
                }

                ImGui.PopItemWidth();
                ImGui.NewLine();
                ImGui.Separator();
                if (ImGui.Button("Close"))
                {
                    SettingsVisible = false;
                }
            }
            ImGui.End();
        }
    }
}
