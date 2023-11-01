// This file is part of EngageTimer
// Copyright (C) 2023 Xorus <xorus@posteo.net>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Plugin;
using EngageTimer.Configuration;
using EngageTimer.Status;
using ImGuiNET;
using XwContainer;

namespace EngageTimer.Ui;

public sealed class FloatingWindow : IDisposable
{
    private const float WindowPadding = 5f;
    private readonly ConfigurationFile _configuration;
    private readonly DalamudPluginInterface _pluginInterface;
    private readonly State _state;
    private readonly UiBuilder _ui;

    private bool _firstLoad = true;
    private ImFontPtr _font;
    private ImFontGlyphRangesBuilderPtr? _grBuilder;

    private float _maxTextWidth;
    private float _paddingLeft;
    private float _paddingRight;
    private bool _stopwatchVisible;
    private bool _triggerFontRebuild = true;
    private bool _useFont;

    public FloatingWindow(Container container)
    {
        _configuration = container.Resolve<ConfigurationFile>();
        _state = container.Resolve<State>();
        _pluginInterface = Bag.PluginInterface;
        _ui = Bag.PluginInterface.UiBuilder;
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
        if (!_configuration.FloatingWindow.Display) return;
        if (_triggerFontRebuild)
        {
            _ui.RebuildFonts();
            return;
        }

        var stopwatchActive = StopwatchActive();
        var countdownActive = CountdownActive();

        if (!_firstLoad && !stopwatchActive && !countdownActive) return;

        if (_useFont && _font.IsLoaded()) ImGui.PushFont(_font);
        DrawWindow(stopwatchActive, countdownActive);
        if (_useFont && _font.IsLoaded()) ImGui.PopFont();

        if (_firstLoad) _firstLoad = false;
    }

    private bool StopwatchActive()
    {
        var displayStopwatch = _configuration.FloatingWindow.EnableStopwatch;
        if (!displayStopwatch) return false;

        if (_configuration.FloatingWindow.AutoHide &&
            (DateTime.Now - _state.CombatEnd).TotalSeconds > _configuration.FloatingWindow.AutoHideTimeout)
            return false;

        return !_configuration.FloatingWindow.StopwatchOnlyInDuty || _state.InInstance;
    }

    private bool CountdownActive()
    {
        return _configuration.FloatingWindow.EnableCountdown && _state.CountingDown && _state.CountDownValue > 0;
    }

    private void DrawWindow(bool stopwatchActive, bool countdownActive)
    {
        // ImGui.SetNextWindowBgAlpha(_configuration.FloatingWindow.FloatingWindowBackgroundColor.Z);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, _configuration.FloatingWindow.BackgroundColor);

        var flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollbar;
        if (_configuration.FloatingWindow.Lock) flags |= ImGuiWindowFlags.NoMouseInputs;

        if (ImGui.Begin("EngageTimer stopwatch", ref _stopwatchVisible, flags))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, _configuration.FloatingWindow.TextColor);
            ImGui.SetWindowFontScale(_configuration.FloatingWindow.Scale);

            var stopwatchDecimals = _configuration.FloatingWindow.DecimalStopwatchPrecision > 0;

            var text = ""; // text to be displayed
            // the largest possible string, taking advantage that the default font has fixed number width
            var maxText = "";
            if (_configuration.FloatingWindow.EnableStopwatch)
                maxText = (_configuration.FloatingWindow.StopwatchAsSeconds ? "0000" : "00:00")
                          + (stopwatchDecimals
                              ? "." + new string('0', _configuration.FloatingWindow.DecimalStopwatchPrecision)
                              : "");
            else if (_configuration.FloatingWindow.EnableCountdown)
                maxText = (_configuration.FloatingWindow.CountdownNegativeSign ? "-" : "") + "00" +
                          (_configuration.FloatingWindow.DecimalCountdownPrecision > 0 ? "." : "") +
                          new string('0', _configuration.FloatingWindow.DecimalCountdownPrecision);

            var displayed = false;
            if (countdownActive)
            {
                var negative = _configuration.FloatingWindow.CountdownNegativeSign ? "-" : "";
                var format = "{0:0." + new string('0', _configuration.FloatingWindow.DecimalCountdownPrecision) +
                             "}";
                var number = _state.CountDownValue + (_configuration.FloatingWindow.AccurateMode ? 0 : 1);
                text = negative + string.Format(CultureInfo.InvariantCulture, format, number);
                displayed = true;
            }
            else if (stopwatchActive)
            {
                if (_configuration.FloatingWindow.StopwatchAsSeconds)
                    text = string.Format(CultureInfo.InvariantCulture,
                        "{0:0." + new string('0', _configuration.FloatingWindow.DecimalStopwatchPrecision) + "}",
                        _state.CombatDuration.TotalSeconds);
                else
                    text = stopwatchDecimals
                        ? _state.CombatDuration.ToString(@"mm\:ss\." + new string('f',
                            _configuration.FloatingWindow.DecimalStopwatchPrecision))
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
                    if (_configuration.FloatingWindow.Align == ConfigurationFile.TextAlign.Left)
                    {
                        _paddingRight = _maxTextWidth - textWidth;
                        _paddingLeft = 0f;
                    }
                    else if (_configuration.FloatingWindow.Align == ConfigurationFile.TextAlign.Center)
                    {
                        _paddingLeft = (_maxTextWidth - textWidth) / 2;
                        _paddingRight = (_maxTextWidth - textWidth) / 2;
                    }
                    else if (_configuration.FloatingWindow.Align == ConfigurationFile.TextAlign.Right)
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

                if (_state.PrePulling) ImGui.PushStyleColor(ImGuiCol.Text, _configuration.FloatingWindow.PrePullColor);
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
        _triggerFontRebuild = false;

        _grBuilder?.Destroy();
        try
        {
            // attempt to load the correct font (I'm only using numbers anyway)
            string[] fonts = { "NotoSansCJKsc-Medium.otf", "NotoSansCJKjp-Medium.otf" };
            string filePath = null;
            foreach (var font in fonts)
            {
                filePath = Path.Combine(_pluginInterface.DalamudAssetDirectory.FullName, "UIRes", font);
                if (File.Exists(filePath)) break;
                filePath = null;
            }

            if (filePath == null) throw new FileNotFoundException("Font file not found!");

            var grBuilder =
                new ImFontGlyphRangesBuilderPtr(ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder());
            // the "z" at the end of this range is still required because of a dalamud issue that somehow removes the
            // ":" from my text range.
            grBuilder.AddText("-0123456789:.z");
            grBuilder.BuildRanges(out var ranges);
            _font = ImGui.GetIO().Fonts.AddFontFromFileTTF(filePath,
                Math.Max(8, _configuration.FloatingWindow.FontSize),
                null, ranges.Data);

            _grBuilder = grBuilder;
        }
        catch (Exception e)
        {
            Bag.Logger.Error(e.Message);
        }

        _useFont = true;
    }
}