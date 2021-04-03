using System;
using ImGuiNET;

namespace EngageTimer.UI
{
    public class Settings
    {
        private readonly Configuration _configuration;

        private bool _visible;

        public Settings(Configuration configuration)
        {
            _configuration = configuration;
        }

        public bool Visible
        {
            get => _visible;
            set => _visible = value;
        }

        public void Draw()
        {
            if (ImGui.Begin("EngageTimer settings", ref _visible, ImGuiWindowFlags.AlwaysAutoResize))
            {
                var enableTickingSound = _configuration.EnableTickingSound;

                ImGui.PushItemWidth(100f);
                var displayCountdown = _configuration.DisplayCountdown;
                if (ImGui.Checkbox("Display countdown", ref displayCountdown))
                {
                    _configuration.DisplayCountdown = displayCountdown;
                    _configuration.Save();
                }

                if (ImGui.Checkbox("Play the timer ticking sound", ref enableTickingSound))
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
                    if (ImGui.Checkbox("Display tenths of seconds", ref stopwatchTenths))
                    {
                        _configuration.StopwatchTenths = stopwatchTenths;
                        _configuration.Save();
                    }

                    var stopwatchCountdown = _configuration.StopwatchCountdown;
                    if (ImGui.Checkbox("Display countdown in stopwatch", ref stopwatchCountdown))
                    {
                        _configuration.StopwatchCountdown = stopwatchCountdown;
                        _configuration.Save();
                    }

                    var stopwatchLock = _configuration.StopwatchLock;
                    if (ImGui.Checkbox("Lock stopwatch", ref stopwatchLock))
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
                        var stopwatchScale = _configuration.StopwatchScale;
                        if (ImGui.SliderFloat("Scale", ref stopwatchScale, 0f, 10f))
                        {
                            _configuration.StopwatchScale = Math.Max(1f, Math.Min(10f, stopwatchScale));
                            _configuration.Save();
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