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
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using EngageTimer.Configuration;
using EngageTimer.Ui.Color;
using ImGuiNET;

namespace EngageTimer.Ui.SettingsTab;

public static class CountdownTab
{
    private static bool _requestTextureCreation = false;
    private static double _lastTextureCreation = 0;
    private static int _exampleNumber = 9;
    private static string? _tempTexturePath;

    public static void DebounceTextureCreation()
    {
        if (!_requestTextureCreation) return;

        var time = ImGui.GetTime();
        if (time - _lastTextureCreation < .05d + Plugin.NumberTextures.LastTextureCreationDuration)
            return; // 50ms + previous time taken
        _lastTextureCreation = time;
        Plugin.NumberTextures.CreateTextures();
        _requestTextureCreation = false;
    }


    private static bool _mocking;
    private static double _mockStart;
    private static double _mockTarget;

    public static void UpdateMock()
    {
        if (!_mocking) return;
        if (_mockTarget == 0 || _mockTarget < ImGui.GetTime()) _mockTarget = ImGui.GetTime() + 30d;

        Plugin.State.CountingDown = true;
        Plugin.State.CountDownValue = (float)(_mockTarget - ImGui.GetTime());
    }

    private static void ToggleMock()
    {
        _mocking = !_mocking;
        if (_mocking)
        {
            Plugin.State.Mocked = true;
            Plugin.State.InCombat = false;
            Plugin.State.CountDownValue = 12.23f;
            Plugin.State.CountingDown = true;
            _mockStart = ImGui.GetTime();
        }
        else
        {
            Plugin.State.Mocked = false;
        }
    }

    public static void Draw()
    {
        var configuration = Plugin.Config;
        ImGui.PushTextWrapPos();
        ImGui.Text(Translator.Tr("Settings_CountdownTab_Info1"));
        if (ImGui.Button(
                (_mocking
                    ? Translator.Tr("Settings_CountdownTab_Test_Stop")
                    : Translator.Tr("Settings_CountdownTab_Test_Start"))
                + "###Settings_CountdownTab_Test")) ToggleMock();
        ImGui.PopTextWrapPos();
        ImGui.Separator();

        Components.AutoField(Plugin.Config.Countdown, "Display");
        Components.AutoField(Plugin.Config.Countdown, "HideOriginalAddon");
        Components.AutoField(Plugin.Config.Countdown, "EnableDecimals");
        Components.AutoField(Plugin.Config.Countdown, "DecimalPrecision", sameLine: true);
        Components.AutoField(Plugin.Config.Countdown, "EnableTickingSound");

        if (Plugin.Config.Countdown.EnableTickingSound)
        {
            ImGui.Indent();
            Components.AutoField(Plugin.Config.Countdown, "UseAlternativeSound");
            Components.AutoField(Plugin.Config.Countdown, "StartTickingFrom");
            ImGui.Unindent();
        }

        Components.AutoField(Plugin.Config.Countdown, "Animate", () => _requestTextureCreation = true);
        if (configuration.Countdown.Animate)
        {
            Components.AutoField(Plugin.Config.Countdown, "AnimateScale", () => _requestTextureCreation = true, true);
            Components.AutoField(Plugin.Config.Countdown, "AnimateOpacity", () => _requestTextureCreation = true, true);
        }

        Components.AutoField(Plugin.Config.Countdown, "EnableDisplayThreshold");
        Components.AutoField(Plugin.Config.Countdown, "DisplayThreshold", sameLine: true);

        ImGui.Separator();
        if (ImGui.CollapsingHeader(Translator.TrId("Settings_CountdownTab_PositioningTitle")))
            CountdownPositionAndSize();
        if (ImGui.CollapsingHeader(Translator.TrId("Settings_CountdownTab_Texture"), ImGuiTreeNodeFlags.DefaultOpen))
            CountdownNumberStyle();
        ImGui.Separator();

        var hideOriginal = !configuration.Countdown.HideOriginalAddon;
        if (hideOriginal) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
        Components.AutoField(Plugin.Config.Countdown, "AccurateMode");
        if (hideOriginal) ImGui.PopStyleVar();

        ImGui.Indent();
        ImGui.PushTextWrapPos(500f);
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
        ImGui.TextWrapped(Translator.Tr("Settings_CountdownTab_AccurateMode_Help"));
        ImGui.PopTextWrapPos();
        ImGui.PopStyleColor();
        ImGui.Unindent();
    }

    private static void CountdownPositionAndSize()
    {
        CountDown.ShowOutline = true;
        ImGui.Indent();
        if (!Plugin.Config.Countdown.HideOriginalAddon)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            ImGui.TextWrapped(Translator.Tr("Settings_CountdownTab_PositionWarning"));
            ImGui.PopStyleColor();
        }

        ImGui.TextWrapped(Translator.Tr("Settings_CountdownTab_MultiMonitorWarning"));

        var countdownOffsetX = Plugin.Config.Countdown.WindowOffset.X * 100;
        if (ImGui.DragFloat(Translator.TrId("Settings_CountdownTab_OffsetX"), ref countdownOffsetX, .1f))
        {
            Plugin.Config.Countdown.WindowOffset =
                new Vector2(countdownOffsetX / 100, Plugin.Config.Countdown.WindowOffset.Y);
            Plugin.Config.Save();
        }

        ImGui.SameLine();

        var countdownOffsetY = Plugin.Config.Countdown.WindowOffset.Y * 100;
        if (ImGui.DragFloat(Translator.TrId("Settings_CountdownTab_OffsetY"), ref countdownOffsetY, .1f))
        {
            Plugin.Config.Countdown.WindowOffset =
                new Vector2(Plugin.Config.Countdown.WindowOffset.X, countdownOffsetY / 100);
            Plugin.Config.Save();
        }

        ImGui.SameLine();
        ImGui.Text(Translator.Tr("Settings_CountdownTab_OffsetText"));
        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Undo.ToIconString() + "###reset_cd_offset"))
        {
            Plugin.Config.Countdown.WindowOffset = Vector2.Zero;
            Plugin.Config.Save();
        }

        var countdownScale = Plugin.Config.Countdown.Scale;
        ImGui.PushItemWidth(100f);
        if (ImGui.InputFloat(Translator.TrId("Settings_CountdownTab_CountdownScale"), ref countdownScale, .01f))
        {
            Plugin.Config.Countdown.Scale = Math.Clamp(countdownScale, 0.05f, 15f);
            Plugin.Config.Save();
        }

        ImGui.PopItemWidth();

        var align = (int)Plugin.Config.Countdown.Align;
        if (ImGui.Combo(Translator.TrId("Settings_CountdownTab_CountdownAlign"), ref align,
                Translator.Tr("Settings_FWTab_TextAlign_Left") + "###Left\0" +
                Translator.Tr("Settings_FWTab_TextAlign_Center") + "###Center\0" +
                Translator.Tr("Settings_FWTab_TextAlign_Right") + "###Right"))
        {
            Plugin.Config.Countdown.Align = (ConfigurationFile.TextAlign)align;
            Plugin.Config.Save();
        }


        ImGui.Unindent();
    }

    private static void CountdownNumberStyle()
    {
        var texture = Plugin.NumberTextures.GetTexture(_exampleNumber);
        const float scale = .5f;
        ImGui.BeginGroup();
        if (ImGui.ImageButton(
                texture.ImGuiHandle,
                new Vector2(texture.Width * scale, texture.Height * scale)
            ))
        {
            _exampleNumber -= 1;
            if (_exampleNumber < 0) _exampleNumber = 9;
        }

        ImGui.SameLine();

        var choices = CountdownConfiguration.BundledTextures;
        var choiceString = "";
        var currentTexture = choices.Count();
        var configuration = Plugin.Config;
        for (var i = 0; i < choices.Count(); i++)
        {
            choiceString += Translator.TrId("Settings_CountdownTab_Texture_" + choices[i], choices[i]) + "\0";
            if (configuration.Countdown.TexturePreset == choices[i]) currentTexture = i;
        }

        ImGui.BeginGroup();
        ImGui.PushItemWidth(200f);
        choiceString += Translator.TrId("Settings_CountdownTab_Texture_custom");
        if (ImGui.Combo("###DropDown_" + Translator.Tr("Settings_CountdownTab_Texture"), ref currentTexture,
                choiceString))
        {
            configuration.Countdown.TexturePreset = currentTexture < choices.Count() ? choices[currentTexture] : "";
            configuration.Save();
            Plugin.NumberTextures.Load();
        }

        ImGui.PopItemWidth();

        Components.AutoField(Plugin.Config.Countdown, "Monospaced", sameLine: true);
        if (configuration.Countdown.TexturePreset == "")
        {
            _tempTexturePath ??= configuration.Countdown.TextureDirectory ?? "";
            ImGui.PushItemWidth(400f);
            ImGui.InputText(Translator.TrId("Settings_CountdownTab_Texture_Custom_Path"), ref _tempTexturePath, 1024);
            ImGui.PopItemWidth();
            if (ImGui.Button(Translator.TrId("Settings_CountdownTab_Texture_Custom_Load")))
            {
                configuration.Countdown.TextureDirectory = _tempTexturePath;
                configuration.Save();
                Plugin.NumberTextures.Load();
            }
        }

        if (ImGui.CollapsingHeader(Translator.TrId("Settings_CountdownTab_NumberStyleTitle"))) CountdownNumberColor();

        if (ImGui.CollapsingHeader(Translator.TrId("Settings_CountdownTab_NumberStyle_Advanced")))
        {
            Components.AutoField(Plugin.Config.Countdown, "LeadingZero");
            Components.Checkbox(Plugin.Config.Countdown.CustomNegativeMargin != null,
                Translator.TrId("Settings_CountdownTab_NumberStyle_EnableCustomNegativeMargin"),
                v => Plugin.Config.Countdown.CustomNegativeMargin = v ? 20f : null);

            if (Plugin.Config.Countdown.CustomNegativeMargin != null)
            {
                ImGui.Indent();
                ImGui.PushItemWidth(100f);
                var nm = configuration.Countdown.CustomNegativeMargin ?? 20f;
                if (ImGui.InputFloat(Translator.TrId("Settings_CountdownTab_NumberStyle_CustomNegativeMargin"), ref nm,
                        1f))
                {
                    configuration.Countdown.CustomNegativeMargin = nm;
                    configuration.SaveNow();
                }

                ImGui.PopItemWidth();
            }
        }

        ImGui.EndGroup();
        ImGui.EndGroup();
    }

    private static void CountdownNumberColor()
    {
        var c = Plugin.Config.Countdown;

        // --- Luminance ---
        ImGui.PushItemWidth(250f);
        {
            Components.ResettableSlider("lum", "± " + Translator.TrId("Settings_CountdownTab_NumberLuminance"),
                c.Luminance, 0f, -1f, 1f, value =>
                {
                    c.Luminance = value;
                    _requestTextureCreation = true;
                });

            // --- Saturation ---
            Components.ResettableSlider("sat", "± " + Translator.TrId("Settings_CountdownTab_NumberSaturation"),
                c.Saturation, 0f, -1f, 1f, value =>
                {
                    c.Saturation = value;
                    _requestTextureCreation = true;
                });

            // --- Hue ---
            if (c.NumberRecolorMode)
            {
                ImGui.PushStyleColor(ImGuiCol.FrameBg, HslConv.HslToVector4Rgb(c.Hue, 0.3f, 0.3f));
                ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, HslConv.HslToVector4Rgb(c.Hue, 0.5f, 0.3f));
                ImGui.PushStyleColor(ImGuiCol.FrameBgActive, HslConv.HslToVector4Rgb(c.Hue, 0.7f, 0.3f));
            }

            Components.ResettableDraggable("hue",
                (c.NumberRecolorMode ? "" : "± ") + Translator.TrId("Settings_CountdownTab_NumberHue"),
                c.Hue, 0, 0, 360, value =>
                {
                    c.Hue = value;
                    _requestTextureCreation = true;
                });
            if (c.NumberRecolorMode) ImGui.PopStyleColor(3);
        }
        ImGui.PopItemWidth();

        Components.Checkbox(c.NumberRecolorMode, Translator.TrId("Settings_CountdownTab_NumberRecolor"),
            v =>
            {
                c.NumberRecolorMode = v;
                _requestTextureCreation = true;
            });
    }
}