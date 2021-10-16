using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiNET;

namespace EngageTimer.UI
{
    public class FloatingWindow : IDisposable
    {
        private readonly Configuration _configuration;
        private readonly State _state;
        private readonly DalamudPluginInterface _pluginInterface;
        private bool _stopwatchVisible;
        private ImFontPtr _font;
        private bool _fontLoaded = false;

        public FloatingWindow(Configuration configuration, State state, DalamudPluginInterface pluginInterface)
        {
            _configuration = configuration;
            _state = state;
            _pluginInterface = pluginInterface;
            _pluginInterface.UiBuilder.BuildFonts += BuildFont;
        }

        public bool StopwatchVisible
        {
            get => _stopwatchVisible;
            set => _stopwatchVisible = value;
        }

        private float _maxTextWidth;
        private float _paddingLeft;
        private float _paddingRight;
        private const float WindowPadding = 5f;

        public void Draw()
        {
            if (!_configuration.DisplayFloatingWindow) return;
            if (!_fontLoaded)
            {
                _pluginInterface.UiBuilder.RebuildFonts();
                return;
            }

            var stopwatchActive = StopwatchActive();
            var countdownActive = CountdownActive();
            if (!stopwatchActive && !countdownActive) return;

            if (_font.IsLoaded()) ImGui.PushFont(_font);

            this.DrawWindow(stopwatchActive, countdownActive);

            if (_font.IsLoaded()) ImGui.PopFont();
        }

        private bool StopwatchActive()
        {
            var displayStopwatch = _configuration.FloatingWindowStopwatch;
            if (!displayStopwatch) return false;

            if (_configuration.AutoHideStopwatch &&
                (DateTime.Now - _state.CombatEnd).TotalSeconds > _configuration.AutoHideTimeout)
                return false;

            return !_configuration.FloatingWindowDisplayStopwatchOnlyInDuty || _state.InInstance;
        }

        private bool CountdownActive()
        {
            return _configuration.FloatingWindowCountdown && _state.CountingDown && _state.CountDownValue > 0;
        }

        private void DrawWindow(bool stopwatchActive, bool countdownActive)
        {
            // ImGui.SetNextWindowBgAlpha(_configuration.FloatingWindowBackgroundColor.Z);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, _configuration.FloatingWindowBackgroundColor);

            var flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollbar;
            if (_configuration.FloatingWindowLock) flags |= ImGuiWindowFlags.NoMouseInputs;

            if (ImGui.Begin("EngageTimer stopwatch", ref _stopwatchVisible, flags))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, _configuration.FloatingWindowTextColor);
                ImGui.SetWindowFontScale(_configuration.FloatingWindowScale);

                var stopwatchDecimals = _configuration.FloatingWindowDecimalStopwatchPrecision > 0;

                var text = ""; // text to be displayed
                // the largest possible string, taking advantage that the default font has fixed number width
                var maxText = (_configuration.FloatingWindowStopwatchAsSeconds ? "0000" : "00") +
                              (stopwatchDecimals
                                  ? new string('0', _configuration.FloatingWindowDecimalStopwatchPrecision)
                                  : "");

                var displayed = false;
                if (countdownActive)
                {
                    text = string.Format(
                        (_configuration.FloatingWindowCountdownNegativeSign ? "-" : "")
                        + "{0:0." + new string('0', _configuration.FloatingWindowDecimalCountdownPrecision) + "}",
                        _state.CountDownValue + (_configuration.FloatingWindowAccurateCountdown ? 0 : 1));
                    displayed = true;
                }
                else if (stopwatchActive)
                {
                    if (_configuration.FloatingWindowStopwatchAsSeconds)
                    {
                        text = Math.Floor(_state.CombatDuration.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                        if (stopwatchDecimals)
                            text += "." + _state.CombatDuration.ToString(
                                new string('f', _configuration.FloatingWindowDecimalStopwatchPrecision)
                            );
                    }
                    else
                    {
                        text = stopwatchDecimals
                            ? _state.CombatDuration.ToString(@"mm\:ss\." + new string('f',
                                _configuration.FloatingWindowDecimalStopwatchPrecision))
                            : _state.CombatDuration.ToString(@"mm\:ss");
                    }

                    displayed = true;
                }

                if (displayed)
                {
                    #region Text Align

                    var textWidth = ImGui.CalcTextSize(text).X;
                    _maxTextWidth = Math.Max(ImGui.CalcTextSize(maxText).X, textWidth); // Math.max juuuuuuuuust in case

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
                }

                ImGui.PopStyleColor();
                ImGui.End();
            }

            ImGui.PopStyleColor();
        }

        /**
         * UI font code adapted from ping plugin by karashiiro
         * https://github.com/karashiiro/PingPlugin/blob/feex/PingPlugin/PingUI.cs
         */
        private void BuildFont()
        {
            try
            {
                var filePath = Path.Combine(_pluginInterface.DalamudAssetDirectory.FullName, "UIRes",
                    "NotoSansCJKjp-Medium.otf");
                if (!File.Exists(filePath)) throw new FileNotFoundException("Font file not found!");
                _font = ImGui.GetIO().Fonts.AddFontFromFileTTF(filePath, Math.Max(8, _configuration.FontSize), null);
            }
            catch (Exception e)
            {
                PluginLog.LogError(e.Message);
            }

            _fontLoaded = true;
        }

        public void Dispose()
        {
            _pluginInterface.UiBuilder.BuildFonts -= BuildFont;
            _pluginInterface.UiBuilder.RebuildFonts();
            // _font.Destroy(); - crashes when I do this
        }
    }
}