using System;
using System.Numerics;
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

        private float _maxTextWidth = 0f;

        private float _paddingLeft = 0f;
        private float _paddingRight = 0f;
        private const float WindowPadding = 5f;

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

            var flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollbar;
            if (_configuration.StopwatchLock) flags = flags | ImGuiWindowFlags.NoMouseInputs;

            if (ImGui.Begin("EngageTimer stopwatch", ref _stopwatchVisible, flags))
            {
                ImGui.SetWindowFontScale(_configuration.StopwatchScale);
                ImGui.PushStyleColor(ImGuiCol.Text, _configuration.StopwatchColor);

                string text; // text to be displayed
                string maxText = _configuration.StopwatchTenths ? "00:00.0" : "00:00"; // the largest possible string

                if (_configuration.StopwatchCountdown && _state.CountingDown && _state.CountDownValue > 0)
                {
                    text = $"-{_state.CountDownValue:0.0}";
                }
                else
                {
                    if (_configuration.StopwatchTenths)
                        text = _state.CombatDuration.ToString(@"mm\:ss\.f");
                    else
                        text = _state.CombatDuration.ToString(@"mm\:ss");
                }

                #region Text Align

                var textWidth = ImGui.CalcTextSize(text).X;
                _maxTextWidth = Math.Max(ImGui.CalcTextSize(maxText).X, textWidth); // Math.max just in case

                if (textWidth < _maxTextWidth)
                {
                    if (_configuration.StopwatchTextAlign == Configuration.TextAlign.Left)
                    {
                        _paddingRight = _maxTextWidth - textWidth;
                        _paddingLeft = 0f;
                    }
                    else if (_configuration.StopwatchTextAlign == Configuration.TextAlign.Center)
                    {
                        _paddingLeft = (_maxTextWidth - textWidth) / 2;
                        _paddingRight = (_maxTextWidth - textWidth) / 2;
                    }
                    else if (_configuration.StopwatchTextAlign == Configuration.TextAlign.Right)
                    {
                        _paddingRight = 0f;
                        _paddingLeft = _maxTextWidth - textWidth;
                    }
                }
                else
                {
                    _paddingLeft = 0f;
                    _paddingRight = 0f;
                }

                var size = ImGui.CalcTextSize(text);
                ImGui.SetCursorPosY(0f);
                ImGui.SetCursorPosX(_paddingLeft + WindowPadding);
                ImGui.SetWindowSize(new Vector2(
                    size.X + _paddingLeft + _paddingRight + WindowPadding * 2f,
                    size.Y + WindowPadding * 1f
                ));

                #endregion

                ImGui.Text(text);

                ImGui.PopStyleColor();
                ImGui.SetWindowFontScale(1f);
            }

            ImGui.End();
        }
    }
}