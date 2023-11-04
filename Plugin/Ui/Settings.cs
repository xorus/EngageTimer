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
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using EngageTimer.Configuration;
using EngageTimer.Ui.Color;
using ImGuiNET;
using JetBrains.Annotations;

namespace EngageTimer.Ui;

public class Settings : Window
{
    private readonly UiBuilder _ui = Plugin.PluginInterface.UiBuilder;
    private int _exampleNumber = 9;

    private bool _mocking;
    private double _mockStart;
    private double _mockTarget;

    private string? _tempTexturePath;

    public Settings() : base("Settings", ImGuiWindowFlags.AlwaysAutoResize)
    {
        Plugin.Translator.LocaleChanged += (_, _) => UpdateWindowName();
        UpdateWindowName();
#if DEBUG
        IsOpen = true;
#endif
    }

    private void UpdateWindowName()
    {
        WindowName = Translator.TrId("Settings_Title");
    }

    private void ToggleMock()
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

    private void UpdateMock()
    {
        if (!_mocking) return;
        if (_mockTarget == 0 || _mockTarget < ImGui.GetTime()) _mockTarget = ImGui.GetTime() + 30d;

        Plugin.State.CountingDown = true;
        Plugin.State.CountDownValue = (float)(_mockTarget - ImGui.GetTime());
    }

    private bool _requestTextureCreation = false;
    private double _lastTextureCreation = 0;


    private void DebounceTextureCreation()
    {
        if (!_requestTextureCreation) return;

        var time = ImGui.GetTime();
        if (time - _lastTextureCreation < .05d + Plugin.NumberTextures.LastTextureCreationDuration)
            return; // 50ms + previous time taken
        _lastTextureCreation = time;
        Plugin.NumberTextures.CreateTextures();
        _requestTextureCreation = false;
    }

    public override void Draw()
    {
        DebounceTextureCreation();
        UpdateMock();

        if (ImGui.BeginTabBar("EngageTimerSettingsTabBar", ImGuiTabBarFlags.None))
        {
            ImGui.PushItemWidth(100f);
            if (ImGui.BeginTabItem(Translator.TrId("Settings_CountdownTab_Title")))
            {
                CountdownTabContent();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(Translator.TrId("Settings_FWTab_Title")))
            {
                FloatingWindowTabContent();
                ImGui.EndTabItem();
            }


            if (ImGui.BeginTabItem(Translator.TrId("Settings_DtrTab_Title")))
            {
                DtrTabContent();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(Translator.TrId("Settings_Web_Title")))
            {
                WebServerTabContent();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("About")) //TransId("Settings_Web_Title")))
            {
                ImGui.PushTextWrapPos();
                ImGui.Text("Hi there! I'm Xorus.");
                ImGui.Text("If you have any suggestions or bugs to report, the best way is to leave it in the" +
                           "issues section of my GitHub repository.");

                if (ImGui.Button("https://github.com/xorus/EngageTimer"))
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://github.com/xorus/EngageTimer",
                        UseShellExecute = true
                    });

                ImGui.Text("If you don't want to/can't use GitHub, just use the feedback button in the plugin" +
                           "list. I don't get notifications for those, but I try to keep up with them as much " +
                           "as I can.");
                ImGui.Text(
                    "Please note that if you leave a discord username as contact info, I may not be able to " +
                    "DM you back if you are not on the Dalamud Discord server because of discord privacy settings." +
                    "I might try to DM you / add you as a friend in those cases.");
                ImGui.PopTextWrapPos();

                if (ImGui.Button("Not a big red ko-fi button"))
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://ko-fi.com/xorus",
                        UseShellExecute = true
                    });

                ImGui.EndTabItem();
            }

            ImGui.PopItemWidth();
            ImGui.EndTabBar();
        }

        ImGui.NewLine();
        ImGui.Separator();
        if (ImGui.Button(Translator.TrId("Settings_Close"))) IsOpen = false;
    }

    private void DtrTabContent()
    {
        ImGui.PushTextWrapPos();
        ImGui.Text(Translator.Tr("Settings_DtrTab_Info"));
        ImGui.PopTextWrapPos();
        ImGui.Separator();

        var enabled = Plugin.Config.Dtr.CombatTimeEnabled;
        if (ImGui.Checkbox(Translator.TrId("Settings_DtrCombatTimer_Enable"), ref enabled))
        {
            Plugin.Config.Dtr.CombatTimeEnabled = enabled;
            Plugin.Config.DebouncedSave();
        }

        Components.AutoField(Plugin.Config.Dtr, "CombatTimePrefix");
        ImGui.SameLine();
        Components.AutoField(Plugin.Config.Dtr, "CombatTimeSuffix");
        ImGui.SameLine();

        if (ImGui.Button(Translator.TrId("Settings_DtrCombatTimer_Defaults")))
        {
            Plugin.Config.Dtr.CombatTimePrefix = DtrConfiguration.DefaultCombatTimePrefix;
            Plugin.Config.Dtr.CombatTimeSuffix = DtrConfiguration.DefaultCombatTimeSuffix;
            Plugin.Config.DebouncedSave();
        }

        Components.AutoField(Plugin.Config.Dtr, "CombatTimeAlwaysDisableOutsideDuty");
        Components.AutoField(Plugin.Config.Dtr, "CombatTimeDecimalPrecision");
        Components.AutoField(Plugin.Config.Dtr, "CombatTimeEnableHideAfter");
        ImGui.SameLine();
        Components.AutoField(Plugin.Config.Dtr, "CombatTimeHideAfter");
    }

    private void CountdownTabContent()
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

    private void CountdownPositionAndSize()
    {
        CountDown.ShowBackground = true;
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
            Plugin.Config.DebouncedSave();
        }

        ImGui.SameLine();

        var countdownOffsetY = Plugin.Config.Countdown.WindowOffset.Y * 100;
        if (ImGui.DragFloat(Translator.TrId("Settings_CountdownTab_OffsetY"), ref countdownOffsetY, .1f))
        {
            Plugin.Config.Countdown.WindowOffset =
                new Vector2(Plugin.Config.Countdown.WindowOffset.X, countdownOffsetY / 100);
            Plugin.Config.DebouncedSave();
        }

        ImGui.SameLine();
        ImGui.Text(Translator.Tr("Settings_CountdownTab_OffsetText"));
        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Undo.ToIconString() + "###reset_cd_offset"))
        {
            Plugin.Config.Countdown.WindowOffset = Vector2.Zero;
            Plugin.Config.DebouncedSave();
        }

        var countdownScale = Plugin.Config.Countdown.Scale;
        ImGui.PushItemWidth(100f);
        if (ImGui.InputFloat(Translator.TrId("Settings_CountdownTab_CountdownScale"), ref countdownScale, .01f))
        {
            Plugin.Config.Countdown.Scale = Math.Clamp(countdownScale, 0.05f, 15f);
            Plugin.Config.DebouncedSave();
        }

        ImGui.PopItemWidth();

        var align = (int)Plugin.Config.Countdown.Align;
        if (ImGui.Combo(Translator.TrId("Settings_CountdownTab_CountdownAlign"), ref align,
                Translator.Tr("Settings_FWTab_TextAlign_Left") + "###Left\0" +
                Translator.Tr("Settings_FWTab_TextAlign_Center") + "###Center\0" +
                Translator.Tr("Settings_FWTab_TextAlign_Right") + "###Right"))
        {
            Plugin.Config.Countdown.Align = (ConfigurationFile.TextAlign)align;
            Plugin.Config.DebouncedSave();
        }


        ImGui.Unindent();
    }

    private void FloatingWindowTabContent()
    {
        ImGui.PushTextWrapPos();
        ImGui.Text(Translator.Tr("Settings_FWTab_Help"));
        ImGui.PopTextWrapPos();
        ImGui.Separator();

        Components.AutoField(Plugin.Config.FloatingWindow, "Display");
        Components.AutoField(Plugin.Config.FloatingWindow, "Lock");
        ImGuiComponents.HelpMarker(Translator.Tr("Settings_FWTab_Lock_Help"));

        Components.AutoField(Plugin.Config.FloatingWindow, "AutoHide");
        Components.AutoField(Plugin.Config.FloatingWindow, "AutoHideTimeout", sameLine: true);

        ImGui.Separator();

        Components.AutoField(Plugin.Config.FloatingWindow, "EnableCountdown");
        Components.AutoField(Plugin.Config.FloatingWindow, "DecimalCountdownPrecision", sameLine: true);

        Components.AutoField(Plugin.Config.FloatingWindow, "EnableStopwatch");
        Components.AutoField(Plugin.Config.FloatingWindow, "DecimalStopwatchPrecision", sameLine: true);

        ImGui.Separator();
        if (ImGui.CollapsingHeader(Translator.TrId("Settings_FWTab_Styling"))) FwStyling();
        ImGui.Separator();

        Components.AutoField(Plugin.Config.FloatingWindow, "AccurateMode");
        ImGuiComponents.HelpMarker(Translator.Tr("Settings_FWTab_AccurateCountdown_Help"));

        Components.AutoField(Plugin.Config.FloatingWindow, "StopwatchOnlyInDuty");
        ImGuiComponents.HelpMarker(Translator.Tr("Settings_FWTab_DisplayStopwatchOnlyInDuty_Help"));

        Components.AutoField(Plugin.Config.FloatingWindow, "CountdownNegativeSign");
        Components.AutoField(Plugin.Config.FloatingWindow, "StopwatchAsSeconds");
        Components.AutoField(Plugin.Config.FloatingWindow, "ShowPrePulling");
        ImGuiComponents.HelpMarker(Translator.Tr("Settings_FWTab_ShowPrePulling_Help"));

        if (!Plugin.Config.FloatingWindow.ShowPrePulling) return;
        ImGui.Indent();
        ImGui.PushItemWidth(110f);
        Components.AutoField(Plugin.Config.FloatingWindow, "PrePullOffset");
        ImGui.PopItemWidth();
        ImGuiComponents.HelpMarker(Translator.Tr("Settings_FWTab_PrePullOffset_Help"));

        Components.AutoField(Plugin.Config.FloatingWindow, "PrePullColor");

        ImGui.Unindent();
    }

    private void FwStyling()
    {
        ImGui.Indent();

        ImGui.BeginGroup();
        Components.AutoField(Plugin.Config.FloatingWindow, "Scale");

        var configuration = Plugin.Config;
        var textAlign = (int)configuration.FloatingWindow.Align;
        if (ImGui.Combo(Translator.TrId("Settings_FWTab_TextAlign"), ref textAlign,
                Translator.Tr("Settings_FWTab_TextAlign_Left") + "###Left\0" +
                Translator.Tr("Settings_FWTab_TextAlign_Center") + "###Center\0" +
                Translator.Tr("Settings_FWTab_TextAlign_Right") + "###Right"))
        {
            configuration.FloatingWindow.Align = (ConfigurationFile.TextAlign)textAlign;
            configuration.DebouncedSave();
        }

        var fontSize = configuration.FloatingWindow.FontSize;
        if (ImGui.InputInt(Translator.TrId("Settings_FWTab_FontSize"), ref fontSize, 4))
        {
            configuration.FloatingWindow.FontSize = Math.Max(0, fontSize);
            configuration.DebouncedSave();

            if (configuration.FloatingWindow.FontSize >= 8) _ui.RebuildFonts();
        }

        ImGui.EndGroup();
        ImGui.SameLine();
        ImGui.BeginGroup();
        Components.AutoField(Plugin.Config.FloatingWindow, "TextColor");
        Components.AutoField(Plugin.Config.FloatingWindow, "BackgroundColor");
        ImGui.EndGroup();

        ImGui.Unindent();
    }

    private void WebServerTabContent()
    {
        ImGui.PushTextWrapPos();
        Components.Text("Settings_Web_Help");
        Components.Text("Settings_Web_HelpAdd");
        ImGui.Text($"http://localhost:{Plugin.Config.WebServer.Port}/");
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Copy))
            ImGui.SetClipboardText($"http://localhost:{Plugin.Config.WebServer.Port}/");

        ImGui.Text(Translator.Tr("Settings_Web_HelpSize"));
        ImGui.PopTextWrapPos();
        ImGui.Separator();

        Components.AutoField(Plugin.Config.WebServer, "Enable");
        Components.AutoField(Plugin.Config.WebServer, "Port", sameLine: true);
        Components.AutoField(Plugin.Config.WebServer, "EnableStopwatchTimeout");
        Components.AutoField(Plugin.Config.WebServer, "StopwatchTimeout", sameLine: true);
    }

    private void CountdownNumberStyle()
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
            configuration.DebouncedSave();
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
                configuration.DebouncedSave();
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
                if (ImGui.InputFloat(Translator.TrId("Settings_CountdownTab_NumberStyle_CustomNegativeMargin"), ref nm, 1f))
                {
                    configuration.Countdown.CustomNegativeMargin = nm;
                    configuration.Save();
                }

                ImGui.PopItemWidth();
            }
        }

        ImGui.EndGroup();
        ImGui.EndGroup();
    }

    private void CountdownNumberColor()
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