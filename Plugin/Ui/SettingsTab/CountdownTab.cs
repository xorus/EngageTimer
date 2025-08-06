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
using Dalamud.Plugin.Internal.Profiles;
using EngageTimer.Configuration;
using EngageTimer.Localization;
using EngageTimer.Ui.Color;
using Dalamud.Bindings.ImGui;

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
    private static double _mockTarget;

    public static void UpdateMock()
    {
        if (!_mocking) return;
        if (_mockTarget == 0 || _mockTarget < ImGui.GetTime()) _mockTarget = ImGui.GetTime() + 30d;

        Plugin.State.CountingDown = true;
        Plugin.State.CountDownValue = (float)(_mockTarget - ImGui.GetTime());
    }

    public static void OnClose()
    {
        _mocking = false;
        Plugin.State.Mocked = false;
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
        ImGui.Text("Settings_CountdownTab_Info1".Tr());
        if (ImGui.Button(_mocking.TrYesNo("Settings_CountdownTab_Test_Stop", "Settings_CountdownTab_Test_Start")
                         + "###Settings_CountdownTab_Test")) ToggleMock();
        ImGui.PopTextWrapPos();
        ImGui.Separator();

        Components.AutoField(Plugin.Config.Countdown, "Display");
        ImGui.Indent();
        CountdownHideOptions();
        ImGui.Unindent();
        Components.AutoField(Plugin.Config.Countdown, "EnableDecimals");
        Components.AutoField(Plugin.Config.Countdown, "DecimalPrecision", sameLine: true);
        Components.AutoField(Plugin.Config.Countdown, "EnableTickingSound");

        if (Plugin.Config.Countdown.HideOriginalAddon && Plugin.Config.Countdown.IgnoreOriginalAddon)
        {
            Plugin.Config.Countdown.IgnoreOriginalAddon = false;
            Plugin.Config.Save();
        }

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
        if (ImGui.CollapsingHeader("Settings_CountdownTab_PositioningTitle".TrId()))
            CountdownPositionAndSize();
        if (ImGui.CollapsingHeader("Settings_CountdownTab_Texture".TrId(), ImGuiTreeNodeFlags.DefaultOpen))
            CountdownNumberStyle();
        ImGui.Separator();

        var hideAccurate = !(configuration.Countdown.HideOriginalAddon || configuration.Countdown.IgnoreOriginalAddon);
        if (hideAccurate) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.3f);
        Components.AutoField(Plugin.Config.Countdown, "AccurateMode");
        if (hideAccurate) ImGui.PopStyleVar();

        ImGui.Indent();
        ImGui.PushTextWrapPos(500f);
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
        ImGui.TextWrapped("Settings_CountdownTab_AccurateMode_Help".Tr());
        ImGui.PopTextWrapPos();
        ImGui.PopStyleColor();
        ImGui.Unindent();
    }

    private static void CountdownHideOptions()
    {
        var cdStatus = 0;
        if (Plugin.Config.Countdown.HideOriginalAddon) cdStatus = 1;
        else if (Plugin.Config.Countdown.IgnoreOriginalAddon) cdStatus = 2;

        // ReSharper disable once ReplaceWithSingleAssignment.False - unreadable
        var changed = false;
        // ReSharper disable once ConvertIfToOrExpression - would hide the next button on the click frame
        if (ImGui.RadioButton("Settings_CountdownTab_DefaultOriginalCountDown".Tr(), ref cdStatus, 0))
            changed = true;
        Components.TooltipOnItemHovered("Settings_CountdownTab_DefaultOriginalCountDown_Help".Tr());
        if (ImGui.RadioButton("Settings_CountdownTab_HideOriginalCountDown".Tr(), ref cdStatus, 1))
            changed = true;
        Components.TooltipOnItemHovered("Settings_CountdownTab_HideOriginalCountDown_Help".Tr());
        if (ImGui.RadioButton("Settings_CountdownTab_IgnoreOriginalCountDown".Tr(), ref cdStatus, 2))
            changed = true;
        Components.TooltipOnItemHovered("Settings_CountdownTab_IgnoreOriginalCountDown_Help".Tr());
        if (!changed) return;

        switch (cdStatus)
        {
            case 0:
                Plugin.Config.Countdown.HideOriginalAddon = false;
                Plugin.Config.Countdown.IgnoreOriginalAddon = false;
                break;
            case 1:
                Plugin.Config.Countdown.HideOriginalAddon = true;
                Plugin.Config.Countdown.IgnoreOriginalAddon = false;
                break;
            case 2:
                Plugin.Config.Countdown.HideOriginalAddon = false;
                Plugin.Config.Countdown.IgnoreOriginalAddon = true;
                break;
        }

        Plugin.Config.Save();
    }

    private static void CountdownPositionAndSize()
    {
        CountDown.ShowOutline = true;
        ImGui.Indent();
        if (!Plugin.Config.Countdown.HideOriginalAddon)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            ImGui.TextWrapped("Settings_CountdownTab_PositionWarning".Tr());
            ImGui.PopStyleColor();
        }

        ImGui.TextWrapped("Settings_CountdownTab_MultiMonitorWarning".Tr());

        var countdownOffsetX = Plugin.Config.Countdown.WindowOffset.X * 100;
        if (ImGui.DragFloat("Settings_CountdownTab_OffsetX".TrId(), ref countdownOffsetX, .1f))
        {
            Plugin.Config.Countdown.WindowOffset =
                Plugin.Config.Countdown.WindowOffset with { X = countdownOffsetX / 100 };
            Plugin.Config.Save();
        }

        ImGui.SameLine();

        var countdownOffsetY = Plugin.Config.Countdown.WindowOffset.Y * 100;
        if (ImGui.DragFloat("Settings_CountdownTab_OffsetY".TrId(), ref countdownOffsetY, .1f))
        {
            Plugin.Config.Countdown.WindowOffset =
                Plugin.Config.Countdown.WindowOffset with { Y = countdownOffsetY / 100 };
            Plugin.Config.Save();
        }

        ImGui.SameLine();
        ImGui.Text("Settings_CountdownTab_OffsetText".Tr());
        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Undo.ToIconString() + "###reset_cd_offset"))
        {
            Plugin.Config.Countdown.WindowOffset = Vector2.Zero;
            Plugin.Config.Save();
        }

        var countdownScale = Plugin.Config.Countdown.Scale;
        ImGui.PushItemWidth(100f);
        if (ImGui.InputFloat("Settings_CountdownTab_CountdownScale".TrId(), ref countdownScale, .01f))
        {
            Plugin.Config.Countdown.Scale = Math.Clamp(countdownScale, 0.05f, 15f);
            Plugin.Config.Save();
        }

        ImGui.PopItemWidth();

        var align = (int)Plugin.Config.Countdown.Align;
        if (ImGui.Combo("Settings_CountdownTab_CountdownAlign".TrId(), ref align,
                "Settings_FWTab_TextAlign_Left".Tr() + "###Left\0" +
                "Settings_FWTab_TextAlign_Center".Tr() + "###Center\0" +
                "Settings_FWTab_TextAlign_Right".Tr() + "###Right"))
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
                texture.Handle,
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
        choiceString += "Settings_CountdownTab_Texture_custom".TrId();
        if (ImGui.Combo("###DropDown_" + "Settings_CountdownTab_Texture".Tr(), ref currentTexture,
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
            ImGui.InputText("Settings_CountdownTab_Texture_Custom_Path".TrId(), ref _tempTexturePath, 1024);
            ImGui.PopItemWidth();
            if (ImGui.Button("Settings_CountdownTab_Texture_Custom_Load".TrId()))
            {
                configuration.Countdown.TextureDirectory = _tempTexturePath;
                configuration.Save();
                Plugin.NumberTextures.Load();
            }
        }

        if (ImGui.CollapsingHeader("Settings_CountdownTab_NumberStyleTitle".TrId())) CountdownNumberColor();

        if (ImGui.CollapsingHeader("Settings_CountdownTab_NumberStyle_Advanced".TrId()))
        {
            Components.AutoField(Plugin.Config.Countdown, "LeadingZero");
            Components.Checkbox(Plugin.Config.Countdown.CustomNegativeMargin != null,
                "Settings_CountdownTab_NumberStyle_EnableCustomNegativeMargin".TrId(),
                v => Plugin.Config.Countdown.CustomNegativeMargin = v ? 20f : null);

            if (Plugin.Config.Countdown.CustomNegativeMargin != null)
            {
                ImGui.Indent();
                ImGui.PushItemWidth(100f);
                var nm = configuration.Countdown.CustomNegativeMargin ?? 20f;
                if (ImGui.InputFloat("Settings_CountdownTab_NumberStyle_CustomNegativeMargin".TrId(), ref nm,
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
            Components.ResettableSlider("lum", "± " + "Settings_CountdownTab_NumberLuminance".TrId(),
                c.Luminance, 0f, -1f, 1f, value =>
                {
                    c.Luminance = value;
                    _requestTextureCreation = true;
                });

            // --- Saturation ---
            Components.ResettableSlider("sat", "± " + "Settings_CountdownTab_NumberSaturation".TrId(),
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
                (c.NumberRecolorMode ? "" : "± ") + "Settings_CountdownTab_NumberHue".TrId(),
                c.Hue, 0, 0, 360, value =>
                {
                    c.Hue = value;
                    _requestTextureCreation = true;
                });
            if (c.NumberRecolorMode) ImGui.PopStyleColor(3);
        }
        ImGui.PopItemWidth();

        Components.Checkbox(c.NumberRecolorMode, "Settings_CountdownTab_NumberRecolor".TrId(),
            v =>
            {
                c.NumberRecolorMode = v;
                _requestTextureCreation = true;
            });
    }
}