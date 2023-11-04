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
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Dalamud.Interface.Animation;
using Dalamud.Interface.Animation.EasingFunctions;
using EngageTimer.Configuration;
using EngageTimer.Status;
using EngageTimer.Ui.CustomEasing;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace EngageTimer.Ui;

public sealed class CountDown
{
    private const byte VisibleFlag = 0x20;

    private const float BaseNumberScale = 1f;
    private const int GameCountdownWidth = 60; // just trust in the magic numbers
    private const float AnimationSize = .7f;

    private readonly Easing _easing = new OutCubic(new TimeSpan(0, 0, 0, 0, 1000));

    private readonly Easing _easingOpacity = new OpacityEasing(
        new TimeSpan(0, 0, 0, 0, 1000),
        1, -0.02, .71, 1
    );

    private bool _accurateMode;

    /**
     * This is a workaround for ImGui taking some time to render a window for the first time.
     * It can cause a small amount on lag when starting a countdown, which we do not want, and this is why I will
     * draw the countdown windows for the first frame the plugin gets loaded so ImGui doesn't get a chance to lag
     *
     * In my testing, this is about 10 to 20ms.
     */
    private bool _firstLoad = true;

    private int _lastSecond;
    private bool _originalAddonHidden;
    private bool _wasInMainViewport = true;

    public CountDown()
    {
        Plugin.Config.OnSave += ConfigurationOnOnSave;
        Plugin.State.StartCountingDown += Start;
        UpdateFromConfig();
    }

    public static bool ShowBackground { get; set; }

    private void ConfigurationOnOnSave(object? sender, EventArgs e)
    {
        UpdateFromConfig();
    }

    /**
     * Things I need to simplify/re-use in this class but dont need to compute every frame
     */
    private void UpdateFromConfig()
    {
        _accurateMode = Plugin.Config.Countdown.HideOriginalAddon && Plugin.Config.Countdown.AccurateMode;
    }

    // finds the original CountDown addon and toggles its visibility flag
    private unsafe void ToggleOriginalAddon()
    {
        var addon = Plugin.GameGui.GetAddonByName("ScreenInfo_CountDown");
        if (addon == IntPtr.Zero) return;

        try
        {
            var atkUnitBase = (AtkUnitBase*)addon;
            atkUnitBase->Flags ^= VisibleFlag;
            _originalAddonHidden = (atkUnitBase->Flags & VisibleFlag) == 0;
        }
        catch (Exception)
        {
            // invalid pointer, don't care and carry on
        }
    }

    // Fix for #19 (countdown overlapping when restarting)
    // ensures the countdown addon is marked as not hidden when starting or restarting a timer
    private void Start(object? sender, EventArgs e)
    {
        _originalAddonHidden = false;
    }

    public void Draw()
    {
#if DEBUG
        if (ImGui.Begin("egdebug"))
        {
            ImGui.Text("_originalAddonHidden: " + _originalAddonHidden);
            ImGui.Text("CountingDown: " + Plugin.State.CountingDown);
            ImGui.Text("CountDownValue: " + Plugin.State.CountDownValue);
            ImGui.Text("Mocked: " + Plugin.State.Mocked);
            ImGui.Text("CombatDuration: " + Plugin.State.CombatDuration);
            ImGui.Text("CombatEnd: " + Plugin.State.CombatEnd);
            ImGui.Text("CombatStart: " + Plugin.State.CombatStart);
            ImGui.Text("InCombat: " + Plugin.State.InCombat);
            ImGui.Text("InInstance: " + Plugin.State.InInstance);
            ImGui.Text($"texture build time: {Plugin.NumberTextures.LastTextureCreationDuration}ms");
        }

        ImGui.End();
#endif

        // display is disabled
        if (!Plugin.Config.Countdown.Display) return;

        if (!_firstLoad && (!Plugin.State.CountingDown || !Plugin.Config.Countdown.Display))
        {
            // re-enable the original addon at the last possible moment (when done counting down) to show "START"
            if (_originalAddonHidden && Plugin.Config.Countdown.HideOriginalAddon) ToggleOriginalAddon();
            return;
        }

        if (Plugin.Config.Countdown.HideOriginalAddon && Plugin.State.CountDownValue <= 5 && !_originalAddonHidden)
            ToggleOriginalAddon();

        var showMainCountdown = _firstLoad || Plugin.State.CountDownValue > 5 || Plugin.Config.Countdown.HideOriginalAddon;
        if (showMainCountdown && Plugin.Config.Countdown.EnableDisplayThreshold &&
            Plugin.State.CountDownValue > Plugin.Config.Countdown.DisplayThreshold)
            return;

        var numberScale = BaseNumberScale;
        var maxNumberScale = numberScale;
        if (showMainCountdown)
        {
            numberScale *= Plugin.Config.Countdown.Scale;
            maxNumberScale = numberScale;
            // numberScale += (_state.CountDownValue % 1) * 0.7f;

            if (Plugin.Config.Countdown.Animate)
            {
                var second = (int)Plugin.State.CountDownValue;
                if (_lastSecond != second)
                {
                    _easing.Restart();
                    _easingOpacity.Restart();
                    _lastSecond = second;
                }

                _easing.Update();
                _easingOpacity.Update();
                if (Plugin.Config.Countdown.AnimateScale)
                {
                    maxNumberScale = numberScale + AnimationSize;
                    numberScale += AnimationSize * (1 - (float)_easing.Value);
                }
            }
        }

        var negativeMargin = Plugin.Config.Countdown.CustomNegativeMargin ?? (
            Plugin.Config.Countdown.Monospaced
                ? Plugin.NumberTextures.NumberNegativeMarginMono
                : Plugin.NumberTextures.NumberNegativeMargin
        ) * numberScale;

        var io = ImGui.GetIO();
        var windowPosition = new Vector2(io.DisplaySize.X * 0.5f, io.DisplaySize.Y * 0.5f) +
                             Plugin.Config.Countdown.WindowOffset * io.DisplaySize;
        var windowSize = new Vector2(
            maxNumberScale * (Plugin.NumberTextures.MaxTextureWidth *
                              (Plugin.Config.Countdown.EnableDecimals
                                  ? 3f + Plugin.Config.Countdown.DecimalPrecision * .5f
                                  : 2
                              )),
            Plugin.NumberTextures.MaxTextureHeight * maxNumberScale + 30);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.SetNextWindowPos(windowPosition, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

        var flags = ImGuiWindowFlags.NoTitleBar
                    | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar
                    | ImGuiWindowFlags.NoDecoration
                    | ImGuiWindowFlags.NoFocusOnAppearing
                    | ImGuiWindowFlags.NoNavFocus
                    | ImGuiWindowFlags.NoInputs
                    | ImGuiWindowFlags.NoMouseInputs
            ;
        if (_wasInMainViewport) flags |= ImGuiWindowFlags.NoBackground;

        var visible = true;
        // prevent a big 0 appearing on screen when "initializing" the countdown window by alpha-ing it
        if (_firstLoad) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0f);
        if (ImGui.Begin("EngageTimer Countdown", ref visible, flags))
        {
            _wasInMainViewport = ImGui.GetWindowViewport().ID == ImGui.GetMainViewport().ID;
            if (ShowBackground)
            {
                var d = ImGui.GetBackgroundDrawList();
                d.AddRect(
                    ImGui.GetWindowPos(),
                    ImGui.GetWindowPos() + ImGui.GetWindowSize(),
                    ImGui.GetColorU32(ImGuiCol.Text), 0f, ImDrawFlags.None,
                    7f + (float)Math.Sin(ImGui.GetTime() * 2) * 5f);
                ShowBackground = false;
            }

            DrawCountdown(showMainCountdown, numberScale, negativeMargin, false);
            if (Plugin.Config.Countdown.Animate && Plugin.Config.Countdown.AnimateOpacity)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, (float)_easingOpacity.Value);
                DrawCountdown(showMainCountdown, numberScale, negativeMargin, true);
                ImGui.PopStyleVar();
            }
        }

        if (_firstLoad)
        {
            ImGui.PopStyleVar();
            _firstLoad = false;
        }

        ImGui.PopStyleVar();
        ImGui.End();
    }

    private void DrawCountdown(bool showMainCountdown, float numberScale, float negativeMarginScaled,
        bool alternateMode)
    {
        var windowSize = ImGui.GetWindowSize();
        var numberTextures = Plugin.NumberTextures;
        var totalHeight = numberTextures.MaxTextureHeight * numberScale;
        ImGui.SetCursorPosY((windowSize.Y - totalHeight) / 2f);

        var mainTotalWidth = 0f;
        List<int>? mainNumbers = null;

        if (showMainCountdown)
        {
            var number = _accurateMode
                ? Math.Floor(Plugin.State.CountDownValue).ToString(CultureInfo.InvariantCulture)
                : Math.Ceiling(Plugin.State.CountDownValue).ToString(CultureInfo.InvariantCulture);

            if (Plugin.Config.Countdown.LeadingZero && number.Length == 1) number = "0" + number;
            
            mainNumbers = NumberList(number);
            if (mainNumbers == null) return;
            // First loop to compute total width
            if (Plugin.Config.Countdown.Monospaced)
                mainTotalWidth = (numberTextures.MaxTextureWidth * numberScale - negativeMarginScaled) *
                                 mainNumbers.Count;
            else
                // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                foreach (var i in mainNumbers)
                {
                    var texture = numberTextures.GetAltTexture(i);
                    mainTotalWidth += texture.Width * numberScale - negativeMarginScaled;
                }

            mainTotalWidth += negativeMarginScaled;
        }

        var decimalTotalWidth = 0f;
        List<int>? decimalNumbers = null;

        var smolNumberScale = numberScale * .5f;
        var smolMaxWidthScaled = numberTextures.MaxTextureWidth * smolNumberScale;
        var smolNumberCursorY = 0f;
        if (Plugin.Config.Countdown.EnableDecimals)
        {
            var decimalPart =
                (Plugin.State.CountDownValue - Math.Truncate(Plugin.State.CountDownValue))
                .ToString("F" + Plugin.Config.Countdown.DecimalPrecision, CultureInfo.InvariantCulture)[2..];

            // align the small numbers on the number baseline
            var offsetY = numberTextures.MaxTextureHeight * numberScale
                          - numberTextures.MaxTextureHeight * smolNumberScale
                          - numberTextures.NumberBottomMargin * smolNumberScale;

            smolNumberCursorY = ImGui.GetCursorPosY() + offsetY;
            decimalNumbers = NumberList(decimalPart);
            if (decimalNumbers == null) return;
            decimalTotalWidth = numberTextures.MaxTextureWidth * (decimalNumbers.Count * smolNumberScale) -
                                (decimalNumbers.Count - 1) * negativeMarginScaled * smolNumberScale;
        }

        // draw main
        if (mainNumbers != null)
        {
            if (Plugin.Config.Countdown.Align == ConfigurationFile.TextAlign.Left)
                ImGui.SetCursorPosX(0f);
            else if (Plugin.Config.Countdown.Align == ConfigurationFile.TextAlign.Center)
                ImGui.SetCursorPosX(windowSize.X / 2f - mainTotalWidth / 2f);
            else if (Plugin.Config.Countdown.Align == ConfigurationFile.TextAlign.Right)
                ImGui.SetCursorPosX(windowSize.X - (mainTotalWidth + decimalTotalWidth));

            // Draw the images \o/
            foreach (var i in mainNumbers)
                DrawNumber(alternateMode, i,
                    numberScale, negativeMarginScaled,
                    numberTextures.MaxTextureWidth * numberScale, Plugin.Config.Countdown.Monospaced);
        }

        if (mainNumbers == null && decimalNumbers != null) ImGui.SetCursorPosX(windowSize.X / 2f + GameCountdownWidth);

        if (decimalNumbers == null) return;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + negativeMarginScaled);
        foreach (var i in decimalNumbers)
        {
            ImGui.SetCursorPosY(smolNumberCursorY);
            // small numbers are always fixed width
            DrawNumber(alternateMode, i, smolNumberScale,
                negativeMarginScaled * smolNumberScale, smolMaxWidthScaled, true);
        }
    }

    private void DrawNumber(bool alternateMode, int i, float numberScale, float negativeMarginScaled,
        float maxWidthScaled,
        bool fixedWidth)
    {
        var texture = alternateMode ? Plugin.NumberTextures.GetAltTexture(i) : Plugin.NumberTextures.GetTexture(i);
        var width = texture.Width * numberScale;
        var cursorX = ImGui.GetCursorPosX();
        if (fixedWidth) ImGui.SetCursorPosX(cursorX + (maxWidthScaled - width) / 2);

        ImGui.Image(texture.ImGuiHandle, new Vector2(texture.Width * numberScale, texture.Height * numberScale));
        ImGui.SameLine();

        if (fixedWidth)
            ImGui.SetCursorPosX(cursorX + maxWidthScaled - negativeMarginScaled);
        else
            ImGui.SetCursorPosX(cursorX + width - negativeMarginScaled);
    }

    private static List<int>? NumberList(string number)
    {
        var integers = new List<int>();
        foreach (var c in number)
        {
            int i;
            if (int.TryParse(c.ToString(), out i)) integers.Add(i);
        }

        return integers;
    }
}