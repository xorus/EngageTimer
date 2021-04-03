using System;
using ImGuiNET;

namespace EngageTimer.UI
{
    public class StopWatch
    {
        private readonly Configuration _configuration;
        private readonly State _state;
        private bool _stopwatchVisible;

        public StopWatch(Configuration configuration, State state)
        {
            _configuration = configuration;
            _state = state;
        }

        public bool StopwatchVisible
        {
            get => _stopwatchVisible;
            set => _stopwatchVisible = value;
        }

        public void Draw()
        {
            if (!_configuration.DisplayStopwatch)
                return;

            var autoHide = _configuration.AutoHideStopwatch &&
                           (DateTime.Now - _state.CombatEnd).TotalSeconds > _configuration.AutoHideTimeout;
            var countdownMode = _configuration.StopwatchCountdown && _state.CountingDown;

            if (autoHide && !countdownMode)
                return;

            ImGui.SetNextWindowBgAlpha(_configuration.StopwatchOpacity);

            var flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollbar |
                        ImGuiWindowFlags.AlwaysAutoResize;
            if (_configuration.StopwatchLock) flags = flags | ImGuiWindowFlags.NoMouseInputs;

            if (ImGui.Begin("EngageTimer stopwatch", ref _stopwatchVisible, flags))
            {
                ImGui.SetWindowFontScale(_configuration.StopwatchScale);
                ImGui.PushStyleColor(ImGuiCol.Text, _configuration.StopwatchColor);

                if (_configuration.StopwatchCountdown && _state.CountingDown && _state.CountDownValue > 0)
                {
                    ImGui.Text(string.Format("-{0:0.0}", _state.CountDownValue));
                }
                else
                {
                    if (_configuration.StopwatchTenths)
                        ImGui.Text(_state.CombatDuration.ToString(@"mm\:ss\.f"));
                    else
                        ImGui.Text(_state.CombatDuration.ToString(@"mm\:ss"));
                }

                ImGui.PopStyleColor();
                ImGui.SetWindowFontScale(1f);
            }

            ImGui.End();
        }
    }
}