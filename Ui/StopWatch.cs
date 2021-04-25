using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;

namespace EngageTimer.UI
{
    public class StopWatch : IDisposable
    {
        private readonly Configuration _configuration;
        private readonly State _state;
        private readonly DalamudPluginInterface _pluginInterface;
        private bool _stopwatchVisible;
        private ImFontPtr _font;
        private bool _fontLoaded = false;

        public StopWatch(Configuration configuration, State state, DalamudPluginInterface pluginInterface)
        {
            _configuration = configuration;
            _state = state;
            _pluginInterface = pluginInterface;

            _pluginInterface.UiBuilder.OnBuildFonts += BuildFont;
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
            if (!_configuration.DisplayStopwatch) return;
            if (!_fontLoaded)
            {
                _pluginInterface.UiBuilder.RebuildFonts();
                return;
            }

            var autoHide = _configuration.AutoHideStopwatch &&
                           (DateTime.Now - _state.CombatEnd).TotalSeconds > _configuration.AutoHideTimeout;
            var countdownMode = _configuration.StopwatchCountdown && _state.CountingDown;
            if (autoHide && !countdownMode)
                return;

            if (_font.IsLoaded()) ImGui.PushFont(_font);

            this.DrawWindow();

            if (_font.IsLoaded()) ImGui.PopFont();
        }

        private void DrawWindow()
        {
            ImGui.SetNextWindowBgAlpha(_configuration.StopwatchOpacity);

            var flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollbar;
            if (_configuration.StopwatchLock) flags = flags | ImGuiWindowFlags.NoMouseInputs;

            if (ImGui.Begin("EngageTimer stopwatch", ref _stopwatchVisible, flags))
            {
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
            }

            ImGui.End();
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
            _pluginInterface.UiBuilder.OnBuildFonts -= BuildFont;
            _pluginInterface.UiBuilder.RebuildFonts();
            // _font.Destroy(); - crashes when I do this
        }
    }
}