using System;
using System.Diagnostics;
using Dalamud.Interface;
using ImGuiNET;
using Swan.Logging;

namespace EngageTimer.UI
{
    public class Settings
    {
        private readonly Configuration _configuration;
        private readonly UiBuilder _uiBuilder;

        private bool _visible;

        public Settings(Configuration configuration, UiBuilder uiBuilder)
        {
            _configuration = configuration;
            _uiBuilder = uiBuilder;
        }

        public bool Visible
        {
            get => _visible;
            set => _visible = value;
        }

        public void Draw()
        {
            if (!Visible)
                return;

            if (ImGui.Begin("EngageTimer settings", ref _visible, ImGuiWindowFlags.AlwaysAutoResize))
            {
                var enableTickingSound = _configuration.EnableTickingSound;

                ImGui.PushItemWidth(100f);
                var displayCountdown = _configuration.DisplayCountdown;
                if (ImGui.Checkbox("Display big countdown", ref displayCountdown))
                {
                    _configuration.DisplayCountdown = displayCountdown;
                    _configuration.Save();
                }

                if (ImGui.Checkbox("Enable countdown ticking sound", ref enableTickingSound))
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

                ImGui.PopItemWidth();
                ImGui.Separator();

                var displayStopwatch = _configuration.DisplayStopwatch;
                if (ImGui.Checkbox("Display stopwatch", ref displayStopwatch))
                {
                    _configuration.DisplayStopwatch = displayStopwatch;
                    _configuration.Save();
                }

                if (displayStopwatch)
                {
                    ImGui.Indent();

                    var stopwatchTenths = _configuration.StopwatchTenths;
                    if (ImGui.Checkbox("Display ", ref stopwatchTenths))
                    {
                        _configuration.StopwatchTenths = stopwatchTenths;
                        _configuration.Save();
                    }

                    ImGui.SameLine();
                    ImGui.PushItemWidth(70f);
                    var stopwatchDecimalPrecision = _configuration.StopwatchDecimalPrecision;
                    if (ImGui.InputInt("decimals in stopwatch window", ref stopwatchDecimalPrecision, 1, 0))
                    {
                        stopwatchDecimalPrecision = Math.Max(1, Math.Min(3, stopwatchDecimalPrecision));
                        _configuration.StopwatchDecimalPrecision = stopwatchDecimalPrecision;
                        _configuration.Save();
                    }

                    ImGui.PopItemWidth();

                    var stopwatchCountdown = _configuration.StopwatchCountdown;
                    if (ImGui.Checkbox("Display countdown in stopwatch", ref stopwatchCountdown))
                    {
                        _configuration.StopwatchCountdown = stopwatchCountdown;
                        _configuration.Save();
                    }

                    var stopwatchLock = _configuration.StopwatchLock;
                    if (ImGui.Checkbox("Lock stopwatch window", ref stopwatchLock))
                    {
                        _configuration.StopwatchLock = stopwatchLock;
                        _configuration.Save();
                    }

                    var autoHideStopwatch = _configuration.AutoHideStopwatch;
                    if (ImGui.Checkbox("Hide stopwatch after", ref autoHideStopwatch))
                    {
                        _configuration.AutoHideStopwatch = autoHideStopwatch;
                        _configuration.Save();
                    }

                    var autoHideTimeout = _configuration.AutoHideTimeout;
                    ImGui.SameLine();
                    if (ImGui.InputFloat("seconds", ref autoHideTimeout, .1f, 1f, "%.1f%"))
                    {
                        _configuration.AutoHideTimeout = Math.Max(0, autoHideTimeout);
                        _configuration.Save();
                    }

                    if (ImGui.CollapsingHeader("Style"))
                    {
                        ImGui.Indent();
                        var textAlign = (int) _configuration.StopwatchTextAlign;
                        if (ImGui.Combo("Text align", ref textAlign, "Left\0Center\0Right"))
                        {
                            _configuration.StopwatchTextAlign = (Configuration.TextAlign) textAlign;
                        }

                        var fontSize = _configuration.FontSize;
                        ImGui.SameLine();
                        if (ImGui.InputInt("Font size", ref fontSize, 4))
                        {
                            _configuration.FontSize = Math.Max(0, fontSize);
                            _configuration.Save();

                            if (_configuration.FontSize >= 8)
                            {
                                _uiBuilder.RebuildFonts();
                            }
                        }

                        var stopwatchColor = _configuration.StopwatchColor;
                        ImGui.PushItemWidth(300f);
                        if (ImGui.ColorEdit4("Text color", ref stopwatchColor))
                        {
                            _configuration.StopwatchColor = stopwatchColor;
                            _configuration.Save();
                        }

                        ImGui.PopItemWidth();

                        var stopwatchOpacity = _configuration.StopwatchOpacity;
                        if (ImGui.SliderFloat("Background opacity", ref stopwatchOpacity, 0f, 1f))
                        {
                            _configuration.StopwatchOpacity = stopwatchOpacity;
                            _configuration.Save();
                        }

                        ImGui.Unindent();
                    }

                    ImGui.Unindent();
                }

                if (ImGui.CollapsingHeader("Web server (for OBS overlay)"))
                {
                    ImGui.Indent();

                    var status = _configuration.EnableWebServer ? "is available" : "will be available";
                    ImGui.Text(
                        $"Overlay {status} on http://localhost:{_configuration.WebServerPort}/ (listening on all interfaces)");

                    var enableWebServer = _configuration.EnableWebServer;
                    if (ImGui.Checkbox("Enable webserver", ref enableWebServer))
                    {
                        _configuration.EnableWebServer = enableWebServer;
                        _configuration.Save();
                    }

                    var webServerPort = _configuration.WebServerPort;
                    if (ImGui.InputInt("Port", ref webServerPort))
                    {
                        _configuration.WebServerPort = webServerPort;
                        _configuration.Save();
                    }

                    var enableWebStopwatchTimeout = _configuration.EnableWebStopwatchTimeout;
                    if (ImGui.Checkbox("Hide timer after ", ref enableWebStopwatchTimeout))
                    {
                        _configuration.EnableWebStopwatchTimeout = enableWebStopwatchTimeout;
                        _configuration.Save();
                    }

                    var webStopwatchTimeout = _configuration.WebStopwatchTimeout;
                    ImGui.SameLine();
                    if (ImGui.DragFloat("seconds", ref webStopwatchTimeout))
                    {
                        _configuration.WebStopwatchTimeout = webStopwatchTimeout;
                        _configuration.Save();
                    }

                    ImGui.Unindent();
                }

                ImGui.PopItemWidth();
                ImGui.NewLine();
                ImGui.Separator();
                if (ImGui.Button("Close")) Visible = false;
            }

            ImGui.End();
        }
    }
}