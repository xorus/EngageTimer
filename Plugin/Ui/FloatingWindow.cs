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
using System.Numerics;
using EngageTimer.Configuration;
using ImGuiNET;

namespace EngageTimer.Ui;

public sealed class FloatingWindow
{
    private const float WindowPadding = 5f;

    private float _maxTextWidth;
    private float _paddingLeft;
    private float _paddingRight;
    private bool _stopwatchVisible;

    public bool StopwatchVisible
    {
        get => _stopwatchVisible;
        set => _stopwatchVisible = value;
    }

    public void Draw()
    {
        if (!Plugin.Config.FloatingWindow.Display) return;
        var stopwatchActive = StopwatchActive();
        var countdownActive = CountdownActive();

        if (!stopwatchActive && !countdownActive) return;

        using (Plugin.FloatingWindowFont.FontHandle?.Push())
        {
            DrawWindow(stopwatchActive, countdownActive);
        }
    }

    private static bool StopwatchActive()
    {
        var displayStopwatch = Plugin.Config.FloatingWindow.EnableStopwatch;
        if (!displayStopwatch) return false;

        if (Plugin.Config.FloatingWindow.AutoHide &&
            (DateTime.Now - Plugin.State.CombatEnd).TotalSeconds > Plugin.Config.FloatingWindow.AutoHideTimeout)
            return false;

        return !Plugin.Config.FloatingWindow.StopwatchOnlyInDuty || Plugin.State.InInstance;
    }

    private static bool CountdownActive()
    {
        return Plugin.Config.FloatingWindow.EnableCountdown && Plugin.State.CountingDown &&
               Plugin.State.CountDownValue > 0;
    }

    private void DrawWindow(bool stopwatchActive, bool countdownActive)
    {
        // ImGui.SetNextWindowBgAlpha(_configuration.FloatingWindow.FloatingWindowBackgroundColor.Z);
        var pushVar = false;
        if (Plugin.Config.FloatingWindow.ForceHideWindowBorder)
        {
            // prevent glitches is user is spamming the checkbox button
            pushVar = true;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        }

        ImGui.PushStyleColor(ImGuiCol.WindowBg, Plugin.Config.FloatingWindow.BackgroundColor);

        var flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoFocusOnAppearing;
        if (Plugin.Config.FloatingWindow.Lock) flags |= ImGuiWindowFlags.NoMouseInputs;

        if (ImGui.Begin("EngageTimer stopwatch", ref _stopwatchVisible, flags))
        {
            var color = Plugin.State.OverrideFwColor ?? Plugin.Config.FloatingWindow.TextColor;
            // use time to change color every half second 
            if (Plugin.State.BlinkStopwatch && ImGui.GetTime() % 1 < 0.5)
                color = Plugin.Config.FloatingWindow.TextColor;

            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.SetWindowFontScale(Plugin.Config.FloatingWindow.Scale);

            var stopwatchDecimals = Plugin.Config.FloatingWindow.DecimalStopwatchPrecision > 0;

            var text = ""; // text to be displayed
            // the largest possible string, taking advantage that the default font has fixed number width
            var maxText = "";
            if (Plugin.Config.FloatingWindow.EnableStopwatch)
                maxText = (Plugin.Config.FloatingWindow.StopwatchAsSeconds ? "0000" : "00:00")
                          + (stopwatchDecimals
                              ? "." + new string('0', Plugin.Config.FloatingWindow.DecimalStopwatchPrecision)
                              : "");
            else if (Plugin.Config.FloatingWindow.EnableCountdown)
                maxText = (Plugin.Config.FloatingWindow.CountdownNegativeSign ? "-" : "") + "00" +
                          (Plugin.Config.FloatingWindow.DecimalCountdownPrecision > 0 ? "." : "") +
                          new string('0', Plugin.Config.FloatingWindow.DecimalCountdownPrecision);

            var displayed = false;
            if (countdownActive)
            {
                var negative = Plugin.Config.FloatingWindow.CountdownNegativeSign ? "-" : "";
                var format = "{0:0." + new string('0', Plugin.Config.FloatingWindow.DecimalCountdownPrecision) +
                             "}";
                var number = Plugin.State.CountDownValue + (Plugin.Config.FloatingWindow.AccurateMode ? 0 : 1);
                if (Plugin.Config.FloatingWindow.DecimalCountdownPrecision == 0) number = (float)Math.Floor(number);
                text = negative + string.Format(CultureInfo.InvariantCulture, format, number);
                displayed = true;
            }
            else if (stopwatchActive)
            {
                if (Plugin.Config.FloatingWindow.StopwatchAsSeconds)
                    text = string.Format(CultureInfo.InvariantCulture,
                        "{0:0." + new string('0', Plugin.Config.FloatingWindow.DecimalStopwatchPrecision) + "}",
                        Plugin.State.CombatDuration.TotalSeconds);
                else
                    text = stopwatchDecimals
                        ? Plugin.State.CombatDuration.ToString(@"mm\:ss\." + new string('f',
                            Plugin.Config.FloatingWindow.DecimalStopwatchPrecision))
                        : Plugin.State.CombatDuration.ToString(@"mm\:ss");

                displayed = true;
            }

            if (displayed)
            {
                #region Text Align

                var textWidth = ImGui.CalcTextSize(text).X;
                _maxTextWidth = Math.Max(ImGui.CalcTextSize(maxText).X, textWidth); // Math.max juuuuuuuuust in case

                if (textWidth < _maxTextWidth)
                {
                    if (Plugin.Config.FloatingWindow.Align == ConfigurationFile.TextAlign.Left)
                    {
                        _paddingRight = _maxTextWidth - textWidth;
                        _paddingLeft = 0f;
                    }
                    else if (Plugin.Config.FloatingWindow.Align == ConfigurationFile.TextAlign.Center)
                    {
                        _paddingLeft = (_maxTextWidth - textWidth) / 2;
                        _paddingRight = (_maxTextWidth - textWidth) / 2;
                    }
                    else if (Plugin.Config.FloatingWindow.Align == ConfigurationFile.TextAlign.Right)
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

                if (Plugin.State.PrePulling)
                    ImGui.PushStyleColor(ImGuiCol.Text, Plugin.Config.FloatingWindow.PrePullColor);
                ImGui.Text(text);
                if (Plugin.State.PrePulling) ImGui.PopStyleColor();
            }

            ImGui.PopStyleColor();
            ImGui.End();
        }

        ImGui.PopStyleColor();
        if (pushVar) ImGui.PopStyleVar();
    }
}