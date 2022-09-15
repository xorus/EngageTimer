using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Plugin;
using EngageTimer.Status;
using ImGuiNET;
using XwContainer;

namespace EngageTimer.Ui;

public sealed class FloatingWindow : IDisposable
{
    private const float WindowPadding = 5f;
    private readonly Configuration _configuration;
    private readonly DalamudPluginInterface _pluginInterface;
    private readonly State _state;
    private readonly UiBuilder _ui;

    private bool _firstLoad = true;
    private ImFontPtr _font;
    private bool _fontLoaded;

    private float _maxTextWidth;
    private float _paddingLeft;
    private float _paddingRight;
    private bool _stopwatchVisible;
    private ImFontGlyphRangesBuilderPtr? _grBuilder = null;

    public FloatingWindow(Container container)
    {
        _configuration = container.Resolve<Configuration>();
        _state = container.Resolve<State>();
        _pluginInterface = container.Resolve<DalamudPluginInterface>();
        _ui = container.Resolve<UiBuilder>();
        _ui.BuildFonts += BuildFont;
    }

    public bool StopwatchVisible
    {
        get => _stopwatchVisible;
        set => _stopwatchVisible = value;
    }

    public void Dispose()
    {
        _ui.BuildFonts -= BuildFont;
        _grBuilder?.Destroy();
        _ui.RebuildFonts();
    }

    public void Draw()
    {
        if (!_configuration.DisplayFloatingWindow) return;
        if (!_fontLoaded)
        {
            _ui.RebuildFonts();
            return;
        }

        var stopwatchActive = StopwatchActive();
        var countdownActive = CountdownActive();

        if (!_firstLoad && !stopwatchActive && !countdownActive) return;

        if (_font.IsLoaded()) ImGui.PushFont(_font);
        DrawWindow(stopwatchActive, countdownActive);
        if (_font.IsLoaded()) ImGui.PopFont();

        if (_firstLoad) _firstLoad = false;
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
            var maxText = "";
            if (_configuration.FloatingWindowStopwatch)
                maxText = (_configuration.FloatingWindowStopwatchAsSeconds ? "0000" : "00:00")
                          + (stopwatchDecimals
                              ? "." + new string('0', _configuration.FloatingWindowDecimalStopwatchPrecision)
                              : "");
            else if (_configuration.FloatingWindowCountdown)
                maxText = (_configuration.FloatingWindowCountdownNegativeSign ? "-" : "") + "00" +
                          (_configuration.FloatingWindowDecimalCountdownPrecision > 0 ? "." : "") +
                          new string('0', _configuration.FloatingWindowDecimalCountdownPrecision);

            var displayed = false;
            if (countdownActive)
            {
                var negative = _configuration.FloatingWindowCountdownNegativeSign ? "-" : "";
                var format = "{0:0." + new string('0', _configuration.FloatingWindowDecimalCountdownPrecision) +
                             "}";
                var number = _state.CountDownValue + (_configuration.FloatingWindowAccurateCountdown ? 0 : 1);
                text = negative + string.Format(CultureInfo.InvariantCulture, format, number);
                displayed = true;
            }
            else if (stopwatchActive)
            {
                if (_configuration.FloatingWindowStopwatchAsSeconds)
                    text = string.Format(CultureInfo.InvariantCulture,
                        "{0:0." + new string('0', _configuration.FloatingWindowDecimalStopwatchPrecision) + "}",
                        _state.CombatDuration.TotalSeconds);
                else
                    text = stopwatchDecimals
                        ? _state.CombatDuration.ToString(@"mm\:ss\." + new string('f',
                            _configuration.FloatingWindowDecimalStopwatchPrecision))
                        : _state.CombatDuration.ToString(@"mm\:ss");

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

                if (_state.PrePulling) ImGui.PushStyleColor(ImGuiCol.Text, _configuration.FloatingWindowPrePullColor);
                ImGui.Text(text);
                if (_state.PrePulling) ImGui.PopStyleColor();
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
    private unsafe void BuildFont()
    {
        _grBuilder?.Destroy();
        try
        {
            var filePath = Path.Combine(_pluginInterface.DalamudAssetDirectory.FullName, "UIRes",
                "NotoSansCJKjp-Medium.otf");
            if (!File.Exists(filePath)) throw new FileNotFoundException("Font file not found!");
            var grBuilder =
                new ImFontGlyphRangesBuilderPtr(ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder());
            grBuilder.AddText("-0123456789:.z");
            grBuilder.BuildRanges(out var ranges);
            _font = ImGui.GetIO().Fonts.AddFontFromFileTTF(filePath,
                Math.Max(8, _configuration.FontSize),
                null, ranges.Data);

            _grBuilder = grBuilder;
        }
        catch (Exception e)
        {
            PluginLog.LogError(e.Message);
        }

        _fontLoaded = true;
    }
}