using System;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Logging;
using ImGuiNET;

namespace EngageTimer.UI
{
    public class Settings
    {
        private readonly Configuration _configuration;
        private readonly UiBuilder _uiBuilder;
        private readonly State _state;

        private bool _visible;

        public Settings(Configuration configuration, State state, UiBuilder uiBuilder)
        {
            _configuration = configuration;
            _uiBuilder = uiBuilder;
            _state = state;
        }

        public bool Visible
        {
            get => _visible;
            set => _visible = value;
        }

        public void Draw()
        {
            if (!Visible) return;
            // _state.Mocked = true;
            // _state.InCombat = false;
            // _state.CountDownValue = 12.23f;
            // _state.CountingDown = true;

            if (ImGui.Begin("EngageTimer settings", ref _visible, ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (ImGui.BeginTabBar("EngageTimerSettingsTabBar", ImGuiTabBarFlags.None))
                {
                    var countdownAccurateCountdown = _configuration.CountdownAccurateCountdown;
                    var floatingWindowAccurateCoundown = _configuration.FloatingWindowAccurateCountdown;

                    ImGui.PushItemWidth(100f);
                    if (ImGui.BeginTabItem("Big countdown"))
                    {
                        ImGui.PushTextWrapPos();
                        ImGui.Text("The \"big countdown\" feature adds the missing numbers to the in-game timer !");
                        ImGui.Text("To test this out, simply start a combat countdown.");
                        ImGui.PopTextWrapPos();
                        ImGui.Separator();

                        var displayCountdown = _configuration.DisplayCountdown;
                        if (ImGui.Checkbox("Enable big countdown", ref displayCountdown))
                        {
                            _configuration.DisplayCountdown = displayCountdown;
                            _configuration.Save();
                        }

                        var hideOriginalCountdown = _configuration.HideOriginalCountdown;
                        if (ImGui.Checkbox("Hide original countdown", ref hideOriginalCountdown))
                        {
                            _configuration.HideOriginalCountdown = hideOriginalCountdown;
                            _configuration.Save();
                        }

                        ImGuiComponents.HelpMarker("Also replace numbers before 5");

                        var enableCountdownDecimal = _configuration.EnableCountdownDecimal;
                        if (ImGui.Checkbox("Display", ref enableCountdownDecimal))
                        {
                            _configuration.EnableCountdownDecimal = enableCountdownDecimal;
                            _configuration.Save();
                        }

                        ImGui.SameLine();
                        ImGui.PushItemWidth(70f);
                        var countdownDecimalPrecision = _configuration.CountdownDecimalPrecision;
                        if (ImGui.InputInt("decimals in countdown", ref countdownDecimalPrecision, 1, 0))
                        {
                            countdownDecimalPrecision = Math.Max(1, Math.Min(3, countdownDecimalPrecision));
                            _configuration.CountdownDecimalPrecision = countdownDecimalPrecision;
                            _configuration.Save();
                        }

                        var enableTickingSound = _configuration.EnableTickingSound;
                        if (ImGui.Checkbox("Play a ticking sound for all numbers", ref enableTickingSound))
                        {
                            _configuration.EnableTickingSound = enableTickingSound;
                            _configuration.Save();
                        }

                        if (enableTickingSound)
                        {
                            ImGui.Indent();
                            var volume = _configuration.TickingSoundVolume * 100f;
                            if (ImGui.DragFloat("Sound volume", ref volume, .1f, 0f, 100f, "%.1f%%"))
                            {
                                _configuration.TickingSoundVolume = Math.Max(0f, Math.Min(1f, volume / 100f));
                                _configuration.Save();
                            }

                            ImGui.Unindent();
                        }

                        ImGui.Separator();

                        var countdownAccurateCountdownDisabled = !_configuration.HideOriginalCountdown;
                        if (countdownAccurateCountdownDisabled)
                        {
                            countdownAccurateCountdownDisabled = true;
                            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                        }

                        if (ImGui.Checkbox("Enable accurate countdown mode for big countdown when possible",
                            ref countdownAccurateCountdown))
                        {
                            _configuration.CountdownAccurateCountdown = countdownAccurateCountdown;
                            _configuration.Save();
                        }

                        if (countdownAccurateCountdownDisabled)
                        {
                            ImGui.PopStyleVar();
                        }

                        ImGui.Indent();
                        ImGui.TextDisabled("The game countdown shows the \"START\" text at 1 instead of 0.");
                        ImGui.PushTextWrapPos();
                        ImGui.TextDisabled("This setting only works when the original countdown is hidden.");
                        ImGui.PopTextWrapPos();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Floating window"))
                    {
                        ImGui.PushTextWrapPos();
                        ImGui.Text("The floating window is a movable window that can display the countdown and the " +
                                   "current combat duration.");
                        ImGui.PopTextWrapPos();
                        ImGui.Separator();

                        var displayFloatingWindow = _configuration.DisplayFloatingWindow;
                        if (ImGui.Checkbox("Enable floating window", ref displayFloatingWindow))
                        {
                            _configuration.DisplayFloatingWindow = displayFloatingWindow;
                            _configuration.Save();
                        }

                        var floatingWindowLock = _configuration.FloatingWindowLock;
                        if (ImGui.Checkbox("Lock floating window", ref floatingWindowLock))
                        {
                            _configuration.FloatingWindowLock = floatingWindowLock;
                            _configuration.Save();
                        }

                        ImGuiComponents.HelpMarker("Disables clicking and moving the window.");

                        var autoHideStopwatch = _configuration.AutoHideStopwatch;
                        if (ImGui.Checkbox("Hide", ref autoHideStopwatch))
                        {
                            _configuration.AutoHideStopwatch = autoHideStopwatch;
                            _configuration.Save();
                        }

                        var autoHideTimeout = _configuration.AutoHideTimeout;
                        ImGui.SameLine();
                        if (ImGui.InputFloat("seconds after combat end", ref autoHideTimeout, .1f, 1f, "%.1f%"))
                        {
                            _configuration.AutoHideTimeout = Math.Max(0, autoHideTimeout);
                            _configuration.Save();
                        }

                        ImGui.Separator();

                        var floatingWindowCountdown = _configuration.FloatingWindowCountdown;
                        if (ImGui.Checkbox("Display countdown" + (floatingWindowCountdown ? " with" : ""),
                            ref floatingWindowCountdown))
                        {
                            _configuration.FloatingWindowCountdown = floatingWindowCountdown;
                            _configuration.Save();
                        }

                        if (floatingWindowCountdown)
                        {
                            ImGui.SameLine();
                            ImGui.PushItemWidth(70f);
                            var fwDecimalCountdownPrecision = _configuration.FloatingWindowDecimalCountdownPrecision;
                            // the little space is necessary because imgui id's the fields by label
                            if (ImGui.InputInt("decimals ", ref fwDecimalCountdownPrecision, 1, 0))
                            {
                                fwDecimalCountdownPrecision = Math.Max(0, Math.Min(3, fwDecimalCountdownPrecision));
                                _configuration.FloatingWindowDecimalCountdownPrecision = fwDecimalCountdownPrecision;
                                _configuration.Save();
                            }

                            ImGui.PopItemWidth();
                        }

                        ImGuiComponents.HelpMarker("Shows the current countdown value (e.g. -13)");

                        var floatingWindowStopwatch = _configuration.FloatingWindowStopwatch;
                        if (ImGui.Checkbox("Display combat timer" + (floatingWindowStopwatch ? " with" : ""),
                            ref floatingWindowStopwatch))
                        {
                            _configuration.FloatingWindowStopwatch = floatingWindowStopwatch;
                            _configuration.Save();
                        }

                        if (floatingWindowStopwatch)
                        {
                            ImGui.SameLine();
                            ImGui.PushItemWidth(70f);
                            var fwDecimalStopwatchPrecision = _configuration.FloatingWindowDecimalStopwatchPrecision;
                            if (ImGui.InputInt("decimals", ref fwDecimalStopwatchPrecision, 1, 0))
                            {
                                fwDecimalStopwatchPrecision = Math.Max(0, Math.Min(3, fwDecimalStopwatchPrecision));
                                _configuration.FloatingWindowDecimalStopwatchPrecision = fwDecimalStopwatchPrecision;
                                _configuration.Save();
                            }

                            ImGui.PopItemWidth();
                        }

                        ImGuiComponents.HelpMarker("Shows the current combat duration");
                        ImGui.Separator();

                        ImGui.Text("Styling");
                        ImGui.Indent();

                        var textAlign = (int)_configuration.StopwatchTextAlign;
                        if (ImGui.Combo("Text align", ref textAlign, "Left\0Center\0Right"))
                        {
                            _configuration.StopwatchTextAlign = (Configuration.TextAlign)textAlign;
                        }

                        var fontSize = _configuration.FontSize;
                        if (ImGui.InputInt("Font size", ref fontSize, 4))
                        {
                            _configuration.FontSize = Math.Max(0, fontSize);
                            _configuration.Save();

                            if (_configuration.FontSize >= 8)
                            {
                                _uiBuilder.RebuildFonts();
                            }
                        }

                        var floatingWindowTextColor = ImGuiComponents.ColorPickerWithPalette(1, "Text color",
                            _configuration.FloatingWindowTextColor);
                        if (floatingWindowTextColor != _configuration.FloatingWindowTextColor)
                        {
                            _configuration.FloatingWindowTextColor = floatingWindowTextColor;
                            _configuration.Save();
                        }

                        ImGui.SameLine();
                        ImGui.Text("Text color and opacity");

                        var floatingWindowBackgroundColor = ImGuiComponents.ColorPickerWithPalette(2, "Text color",
                            _configuration.FloatingWindowBackgroundColor);
                        if (floatingWindowBackgroundColor != _configuration.FloatingWindowBackgroundColor)
                        {
                            _configuration.FloatingWindowBackgroundColor = floatingWindowBackgroundColor;
                            _configuration.Save();
                        }

                        ImGui.SameLine();
                        ImGui.Text("Background color and opacity");
                        ImGui.Unindent();
                        ImGui.Separator();

                        if (ImGui.Checkbox("Enable accurate countdown in floating window",
                            ref floatingWindowAccurateCoundown))
                        {
                            _configuration.FloatingWindowAccurateCountdown = floatingWindowAccurateCoundown;
                            _configuration.Save();
                        }

                        ImGuiComponents.HelpMarker("See the Big countdown tab for an explanation");

                        var fWDisplayStopwatchOnlyInDuty = _configuration.FloatingWindowDisplayStopwatchOnlyInDuty;
                        if (ImGui.Checkbox("Hide stopwatch when not bound by duty",
                            ref fWDisplayStopwatchOnlyInDuty))
                        {
                            _configuration.FloatingWindowDisplayStopwatchOnlyInDuty = fWDisplayStopwatchOnlyInDuty;
                            _configuration.Save();
                        }

                        ImGuiComponents.HelpMarker("Basically hides the stopwatch when you are in the overworld");

                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Web Server (OBS)"))
                    {
                        var enableWebServer = _configuration.EnableWebServer;

                        ImGui.PushTextWrapPos();
                        ImGui.Text("This feature allows you to add a countdown and stopwatch overlay into " +
                                   "your OBS (or other software) streams and recordings via a browser source.");
                        if (enableWebServer)
                        {
                            ImGui.Text(
                                "To add it to your scene, create a browser source in your scene and add the " +
                                "following URL : ");
                        }
                        else
                        {
                            ImGui.Text("To add it to your scene, enable the webserver " +
                                       "then create a browser source in your scene and add the following URL : ");
                        }

                        ImGui.Text($"http://localhost:{_configuration.WebServerPort}/");
                        ImGui.SameLine();
                        if (ImGuiComponents.IconButton(FontAwesomeIcon.Copy))
                        {
                            ImGui.SetClipboardText($"http://localhost:{_configuration.WebServerPort}/");
                        }

                        ImGui.Text("Recommended window size is 300x100.");
                        ImGui.PopTextWrapPos();
                        ImGui.Separator();

                        if (ImGui.Checkbox("Enable webserver on port", ref enableWebServer))
                        {
                            _configuration.EnableWebServer = enableWebServer;
                            _configuration.Save();
                        }

                        ImGui.SameLine();
                        var webServerPort = _configuration.WebServerPort;
                        if (ImGui.InputInt("", ref webServerPort))
                        {
                            _configuration.WebServerPort = webServerPort;
                            _configuration.Save();
                        }

                        var enableWebStopwatchTimeout = _configuration.EnableWebStopwatchTimeout;
                        if (ImGui.Checkbox("Hide overlay", ref enableWebStopwatchTimeout))
                        {
                            _configuration.EnableWebStopwatchTimeout = enableWebStopwatchTimeout;
                            _configuration.Save();
                        }

                        var webStopwatchTimeout = _configuration.WebStopwatchTimeout;
                        ImGui.SameLine();
                        if (ImGui.DragFloat("seconds after combat ends", ref webStopwatchTimeout))
                        {
                            _configuration.WebStopwatchTimeout = webStopwatchTimeout;
                            _configuration.Save();
                        }
                    }

                    ImGui.PopItemWidth();
                    ImGui.EndTabBar();
                }

                ImGui.NewLine();
                ImGui.Separator();
                if (ImGui.Button("Close")) Visible = false;
            }

            ImGui.End();
        }
    }
}