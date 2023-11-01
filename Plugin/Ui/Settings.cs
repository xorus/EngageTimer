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

namespace EngageTimer.Ui;

public class Settings : Window
{
    private readonly UiBuilder _ui = Plugin.PluginInterface.UiBuilder;
    private int _exampleNumber = 9;

    private bool _mocking;
    private double _mockStart;
    private double _mockTarget;

    private string _tempTexturePath;

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

    public override void Draw()
    {
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

        var configuration = Plugin.Config;
        var enabled = configuration.Dtr.CombatTimeEnabled;
        if (ImGui.Checkbox(Translator.TrId("Settings_DtrCombatTimer_Enable"), ref enabled))
        {
            configuration.Dtr.CombatTimeEnabled = enabled;
            configuration.Save();
        }

        var prefix = configuration.Dtr.CombatTimePrefix;
        if (ImGui.InputText(Translator.TrId("Settings_DtrCombatTimer_Prefix"), ref prefix, 50))
        {
            configuration.Dtr.CombatTimePrefix = prefix;
            configuration.Save();
        }

        ImGui.SameLine();
        var suffix = configuration.Dtr.CombatTimeSuffix;
        if (ImGui.InputText(Translator.TrId("Settings_DtrCombatTimer_Suffix"), ref suffix, 50))
        {
            configuration.Dtr.CombatTimeSuffix = suffix;
            configuration.Save();
        }

        ImGui.SameLine();
        if (ImGui.Button(Translator.TrId("Settings_DtrCombatTimer_Defaults")))
        {
            configuration.Dtr.CombatTimePrefix = DtrConfiguration.DefaultCombatTimePrefix;
            configuration.Dtr.CombatTimeSuffix = DtrConfiguration.DefaultCombatTimeSuffix;
            configuration.Save();
        }


        var outside = configuration.Dtr.CombatTimeAlwaysDisableOutsideDuty;
        if (ImGui.Checkbox(Translator.TrId("Settings_DtrCombatTimer_AlwaysDisableOutsideDuty"), ref outside))
        {
            configuration.Dtr.CombatTimeAlwaysDisableOutsideDuty = outside;
            configuration.Save();
        }

        var decimals = configuration.Dtr.CombatTimeDecimalPrecision;
        if (ImGui.InputInt(Translator.TrId("Settings_DtrCombatTimer_DecimalPrecision"), ref decimals, 1, 0))
        {
            configuration.Dtr.CombatTimeDecimalPrecision = Math.Max(0, Math.Min(3, decimals));
            configuration.Save();
        }

        var enableHideAfter = configuration.Dtr.CombatTimeEnableHideAfter;
        if (ImGui.Checkbox(Translator.TrId("Settings_DtrCombatTimer_HideAfter"), ref enableHideAfter))
        {
            configuration.Dtr.CombatTimeEnableHideAfter = enableHideAfter;
            configuration.Save();
        }

        ImGui.SameLine();
        var hideAfter = configuration.Dtr.CombatTimeHideAfter;
        if (ImGui.InputFloat(Translator.TrId("Settings_DtrCombatTimer_HideAfterRight"), ref hideAfter, 0.1f, 1f, "%.1f%"))
        {
            configuration.Dtr.CombatTimeHideAfter = Math.Max(0, hideAfter);
            configuration.Save();
        }
    }

    private void CountdownTabContent()
    {
        var configuration = Plugin.Config;
        var countdownAccurateCountdown = configuration.Countdown.AccurateMode;

        ImGui.PushTextWrapPos();
        ImGui.Text(Translator.Tr("Settings_CountdownTab_Info1"));
        if (ImGui.Button(
                (_mocking
                    ? Translator.Tr("Settings_CountdownTab_Test_Stop")
                    : Translator.Tr("Settings_CountdownTab_Test_Start"))
                + "###Settings_CountdownTab_Test"))
            ToggleMock();

        ImGui.PopTextWrapPos();
        ImGui.Separator();

        var displayCountdown = configuration.Countdown.Display;
        if (ImGui.Checkbox(Translator.TrId("Settings_CountdownTab_Enable"),
                ref displayCountdown))
        {
            configuration.Countdown.Display = displayCountdown;
            configuration.Save();
        }

        var hideOriginalCountdown = configuration.Countdown.HideOriginalAddon;
        if (ImGui.Checkbox(Translator.TrId("Settings_CountdownTab_HideOriginalCountDown"),
                ref hideOriginalCountdown))
        {
            configuration.Countdown.HideOriginalAddon = hideOriginalCountdown;
            configuration.Save();
        }

        ImGuiComponents.HelpMarker(Translator.Tr("Settings_CountdownTab_HideOriginalCountDown_Help"));

        var enableCountdownDecimal = configuration.Countdown.EnableDecimals;
        if (ImGui.Checkbox(Translator.TrId("Settings_CountdownTab_CountdownDecimals_Left"),
                ref enableCountdownDecimal))
        {
            configuration.Countdown.EnableDecimals = enableCountdownDecimal;
            configuration.Save();
        }

        ImGui.SameLine();
        ImGui.PushItemWidth(70f);
        var countdownDecimalPrecision = configuration.Countdown.DecimalPrecision;
        if (ImGui.InputInt(Translator.TrId("Settings_CountdownTab_CountdownDecimals_Right"),
                ref countdownDecimalPrecision, 1, 0))
        {
            countdownDecimalPrecision = Math.Max(1, Math.Min(3, countdownDecimalPrecision));
            configuration.Countdown.DecimalPrecision = countdownDecimalPrecision;
            configuration.Save();
        }

        ImGui.PopItemWidth();

        var enableTickingSound = configuration.Countdown.EnableTickingSound;
        if (ImGui.Checkbox(Translator.TrId("Settings_CountdownTab_Audio_Enable"), ref enableTickingSound))
        {
            configuration.Countdown.EnableTickingSound = enableTickingSound;
            configuration.Save();
        }

        if (enableTickingSound)
        {
            ImGui.Indent();
            var alternativeSound = configuration.Countdown.UseAlternativeSound;
            if (ImGui.Checkbox(Translator.TrId("Settings_CountdownTab_Audio_UseAlternativeSound"),
                    ref alternativeSound))
            {
                configuration.Countdown.UseAlternativeSound = alternativeSound;
                configuration.Save();
            }

            var tickFrom = configuration.Countdown.StartTickingFrom;
            // ImGui.Text(Trans("Settings_CountdownTab_TickFrom"));
            if (ImGui.InputInt(Translator.TrId("Settings_CountdownTab_TickFrom"), ref tickFrom, 1, 0))
            {
                configuration.Countdown.StartTickingFrom = Math.Min(30, Math.Max(5, tickFrom));
                configuration.Save();
            }

            ImGuiComponents.HelpMarker(Translator.Tr("Settings_CountdownTab_TickFrom_Help"));

            ImGui.Unindent();
        }

        var animate = configuration.Countdown.Animate;
        var numberTextures = Plugin.NumberTextures;
        if (ImGui.Checkbox(Translator.TrId("Settings_CountdownTab_Animate"), ref animate))
        {
            configuration.Countdown.Animate = animate;
            configuration.Save();
            numberTextures.CreateTextures();
        }

        if (animate)
        {
            ImGui.SameLine();
            var animateScale = configuration.Countdown.AnimateScale;
            if (ImGui.Checkbox(Translator.TrId("Settings_CountdownTab_AnimateScale"), ref animateScale))
            {
                configuration.Countdown.AnimateScale = animateScale;
                configuration.Save();
                numberTextures.CreateTextures();
            }

            ImGui.SameLine();
            var animateOpacity = configuration.Countdown.AnimateOpacity;
            if (ImGui.Checkbox(Translator.TrId("Settings_CountdownTab_AnimateOpacity"), ref animateOpacity))
            {
                configuration.Countdown.AnimateOpacity = animateOpacity;
                configuration.Save();
                numberTextures.CreateTextures();
            }
        }

        var enableCountdownDisplayThreshold = configuration.Countdown.EnableDisplayThreshold;
        if (ImGui.Checkbox(Translator.TrId("Settings_CountdownTab_CountdownDisplayThreshold"),
                ref enableCountdownDisplayThreshold))
        {
            configuration.Countdown.EnableDisplayThreshold = enableCountdownDisplayThreshold;
            configuration.Save();
        }

        ImGui.SameLine();

        var countdownDisplayThreshold = configuration.Countdown.DisplayThreshold;
        if (ImGui.InputInt("###Settings_CountdownTab_CountdownDisplayThreshold_Value",
                ref countdownDisplayThreshold, 1))
        {
            countdownDisplayThreshold = Math.Clamp(countdownDisplayThreshold, 0, 30);
            configuration.Countdown.DisplayThreshold = countdownDisplayThreshold;
            configuration.Save();
        }

        ImGui.SameLine();
        ImGuiComponents.HelpMarker(Translator.Tr("Settings_CountdownTab_CountdownDisplayThreshold_Help"));

        ImGui.Separator();
        if (ImGui.CollapsingHeader(Translator.TrId("Settings_CountdownTab_PositioningTitle"))) CountdownPositionAndSize();
        if (ImGui.CollapsingHeader(Translator.TrId("Settings_CountdownTab_Texture"), ImGuiTreeNodeFlags.DefaultOpen))
            CountdownNumberStyle();
        ImGui.Separator();

        var countdownAccurateCountdownDisabled = !configuration.Countdown.HideOriginalAddon;
        if (countdownAccurateCountdownDisabled) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);

        if (ImGui.Checkbox(Translator.TrId("Settings_CountdownTab_AccurateMode"),
                ref countdownAccurateCountdown))
        {
            configuration.Countdown.AccurateMode = countdownAccurateCountdown;
            configuration.Save();
        }

        if (countdownAccurateCountdownDisabled) ImGui.PopStyleVar();

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
        var configuration = Plugin.Config;
        if (!configuration.Countdown.HideOriginalAddon)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            ImGui.TextWrapped(Translator.Tr("Settings_CountdownTab_PositionWarning"));
            ImGui.PopStyleColor();
        }

        ImGui.TextWrapped(Translator.Tr("Settings_CountdownTab_MultiMonitorWarning"));

        var countdownOffsetX = configuration.Countdown.WindowOffset.X * 100;
        if (ImGui.DragFloat(Translator.TrId("Settings_CountdownTab_OffsetX"), ref countdownOffsetX, .1f))
        {
            configuration.Countdown.WindowOffset =
                new Vector2(countdownOffsetX / 100, configuration.Countdown.WindowOffset.Y);
            configuration.Save();
        }

        ImGui.SameLine();

        var countdownOffsetY = configuration.Countdown.WindowOffset.Y * 100;
        if (ImGui.DragFloat(Translator.TrId("Settings_CountdownTab_OffsetY"), ref countdownOffsetY, .1f))
        {
            configuration.Countdown.WindowOffset =
                new Vector2(configuration.Countdown.WindowOffset.X, countdownOffsetY / 100);
            configuration.Save();
        }

        ImGui.SameLine();
        ImGui.Text(Translator.Tr("Settings_CountdownTab_OffsetText"));
        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Undo.ToIconString() + "###reset_cd_offset"))
        {
            configuration.Countdown.WindowOffset = Vector2.Zero;
            configuration.Save();
        }

        var countdownScale = configuration.Countdown.Scale;
        ImGui.PushItemWidth(100f);
        if (ImGui.InputFloat(Translator.TrId("Settings_CountdownTab_CountdownScale"), ref countdownScale, .01f))
        {
            configuration.Countdown.Scale = Math.Clamp(countdownScale, 0.05f, 15f);
            configuration.Save();
        }

        ImGui.PopItemWidth();

        var align = (int)configuration.Countdown.Align;
        if (ImGui.Combo(Translator.TrId("Settings_CountdownTab_CountdownAlign"), ref align,
                Translator.Tr("Settings_FWTab_TextAlign_Left") + "###Left\0" +
                Translator.Tr("Settings_FWTab_TextAlign_Center") + "###Center\0" +
                Translator.Tr("Settings_FWTab_TextAlign_Right") + "###Right"))
        {
            configuration.Countdown.Align = (ConfigurationFile.TextAlign)align;
            configuration.Save();
        }


        ImGui.Unindent();
    }

    private void FloatingWindowTabContent()
    {
        var configuration = Plugin.Config;
        var floatingWindowAccurateCountdown = configuration.FloatingWindow.AccurateMode;

        ImGui.PushTextWrapPos();
        ImGui.Text(Translator.Tr("Settings_FWTab_Help"));
        ImGui.PopTextWrapPos();
        ImGui.Separator();

        var displayFloatingWindow = configuration.FloatingWindow.Display;
        if (ImGui.Checkbox(Translator.TrId("Settings_FWTab_Display"), ref displayFloatingWindow))
        {
            configuration.FloatingWindow.Display = displayFloatingWindow;
            configuration.Save();
        }

        var floatingWindowLock = configuration.FloatingWindow.Lock;
        if (ImGui.Checkbox(Translator.TrId("Settings_FWTab_Lock"), ref floatingWindowLock))
        {
            configuration.FloatingWindow.Lock = floatingWindowLock;
            configuration.Save();
        }

        ImGuiComponents.HelpMarker(Translator.Tr("Settings_FWTab_Lock_Help"));

        var autoHideStopwatch = configuration.FloatingWindow.AutoHide;
        if (ImGui.Checkbox(Translator.TrId("Settings_FWTab_AutoHide_Left"), ref autoHideStopwatch))
        {
            configuration.FloatingWindow.AutoHide = autoHideStopwatch;
            configuration.Save();
        }

        var autoHideTimeout = configuration.FloatingWindow.AutoHideTimeout;
        ImGui.SameLine();
        if (ImGui.InputFloat(Translator.TrId("Settings_FWTab_AutoHide_Right"), ref autoHideTimeout, .1f, 1f,
                "%.1f%"))
        {
            configuration.FloatingWindow.AutoHideTimeout = Math.Max(0, autoHideTimeout);
            configuration.Save();
        }

        ImGui.Separator();

        var floatingWindowCountdown = configuration.FloatingWindow.EnableCountdown;
        if (ImGui.Checkbox(
                Translator.TrId("Settings_FWTab_CountdownPrecision" +
                                (floatingWindowCountdown ? "_With" : "") + "_Left"),
                ref floatingWindowCountdown))
        {
            configuration.FloatingWindow.EnableCountdown = floatingWindowCountdown;
            configuration.Save();
        }

        if (floatingWindowCountdown)
        {
            ImGui.SameLine();
            ImGui.PushItemWidth(70f);
            var fwDecimalCountdownPrecision = configuration.FloatingWindow.DecimalCountdownPrecision;
            // the little space is necessary because imgui id's the fields by label
            if (ImGui.InputInt(
                    Translator.TrId("Settings_FWTab_CountdownPrecision_Right"),
                    ref fwDecimalCountdownPrecision, 1, 0))
            {
                fwDecimalCountdownPrecision = Math.Max(0, Math.Min(3, fwDecimalCountdownPrecision));
                configuration.FloatingWindow.DecimalCountdownPrecision = fwDecimalCountdownPrecision;
                configuration.Save();
            }

            ImGui.PopItemWidth();
        }

        ImGuiComponents.HelpMarker(Translator.Tr("Settings_FWTab_CountdownPrecision_Help"));

        var floatingWindowStopwatch = configuration.FloatingWindow.EnableStopwatch;
        if (ImGui.Checkbox(
                Translator.TrId("Settings_FWTab_StopwatchPrecision" +
                                (floatingWindowStopwatch ? "_With" : "") + "_Left"),
                ref floatingWindowStopwatch))
        {
            configuration.FloatingWindow.EnableStopwatch = floatingWindowStopwatch;
            configuration.Save();
        }

        if (floatingWindowStopwatch)
        {
            ImGui.SameLine();
            ImGui.PushItemWidth(70f);
            var fwDecimalStopwatchPrecision = configuration.FloatingWindow.DecimalStopwatchPrecision;
            if (ImGui.InputInt(Translator.TrId("Settings_FWTab_StopwatchPrecision_Right"),
                    ref fwDecimalStopwatchPrecision, 1, 0))
            {
                fwDecimalStopwatchPrecision = Math.Max(0, Math.Min(3, fwDecimalStopwatchPrecision));
                configuration.FloatingWindow.DecimalStopwatchPrecision = fwDecimalStopwatchPrecision;
                configuration.Save();
            }

            ImGui.PopItemWidth();
        }

        ImGuiComponents.HelpMarker(Translator.Tr("Settings_FWTab_StopwatchPrecision_Help"));

        ImGui.Separator();
        if (ImGui.CollapsingHeader(Translator.TrId("Settings_FWTab_Styling"))) FwStyling();
        ImGui.Separator();

        if (ImGui.Checkbox(Translator.TrId("Settings_FWTab_AccurateCountdown"),
                ref floatingWindowAccurateCountdown))
        {
            configuration.FloatingWindow.AccurateMode = floatingWindowAccurateCountdown;
            configuration.Save();
        }

        ImGuiComponents.HelpMarker(Translator.Tr("Settings_FWTab_AccurateCountdown_Help"));

        var fWDisplayStopwatchOnlyInDuty = configuration.FloatingWindow.StopwatchOnlyInDuty;
        if (ImGui.Checkbox(Translator.TrId("Settings_FWTab_DisplayStopwatchOnlyInDuty"),
                ref fWDisplayStopwatchOnlyInDuty))
        {
            configuration.FloatingWindow.StopwatchOnlyInDuty = fWDisplayStopwatchOnlyInDuty;
            configuration.Save();
        }

        ImGuiComponents.HelpMarker(Translator.Tr("Settings_FWTab_DisplayStopwatchOnlyInDuty_Help"));

        var negativeSign = configuration.FloatingWindow.CountdownNegativeSign;
        if (ImGui.Checkbox(Translator.TrId("Settings_FWTab_CountdownNegativeSign"), ref negativeSign))
        {
            configuration.FloatingWindow.CountdownNegativeSign = negativeSign;
            configuration.Save();
        }

        var displaySeconds = configuration.FloatingWindow.StopwatchAsSeconds;
        if (ImGui.Checkbox(Translator.TrId("Settings_FWTab_StopwatchAsSeconds"), ref displaySeconds))
        {
            configuration.FloatingWindow.StopwatchAsSeconds = displaySeconds;
            configuration.Save();
        }

        var prePullWarning = configuration.FloatingWindow.ShowPrePulling;
        if (ImGui.Checkbox(Translator.TrId("Settings_FWTab_ShowPrePulling"), ref prePullWarning))
        {
            configuration.FloatingWindow.ShowPrePulling = prePullWarning;
            configuration.Save();
        }

        ImGuiComponents.HelpMarker(Translator.Tr("Settings_FWTab_ShowPrePulling_Help"));

        if (prePullWarning)
        {
            ImGui.Indent();
            var offset = configuration.FloatingWindow.PrePullOffset;
            ImGui.PushItemWidth(110f);
            if (ImGui.InputFloat(Translator.Tr("Settings_FWTab_PrePullOffset"), ref offset, 0.1f, 1f, "%.3fs"))
            {
                configuration.FloatingWindow.PrePullOffset = offset;
                configuration.Save();
            }

            ImGui.PopItemWidth();
            ImGuiComponents.HelpMarker(Translator.Tr("Settings_FWTab_PrePullOffset_Help"));

            ImGui.SameLine();
            var prePullColor = ImGuiComponents.ColorPickerWithPalette(10,
                Translator.TrId("Settings_FWTab_TextColor"),
                configuration.FloatingWindow.PrePullColor);
            if (prePullColor != configuration.FloatingWindow.PrePullColor)
            {
                configuration.FloatingWindow.PrePullColor = prePullColor;
                configuration.Save();
            }

            ImGui.SameLine();
            ImGui.Text(Translator.Tr("Settings_FWTab_TextColor"));

            ImGui.Unindent();
        }
    }

    private void FwStyling()
    {
        ImGui.Indent();

        ImGui.BeginGroup();
        var configuration = Plugin.Config;
        var fwScale = configuration.FloatingWindow.Scale;
        ImGui.PushItemWidth(100f);
        if (ImGui.DragFloat(Translator.TrId("Settings_CountdownTab_FloatingWindowScale"), ref fwScale, .01f))
        {
            configuration.FloatingWindow.Scale = Math.Clamp(fwScale, 0.05f, 15f);
            configuration.Save();
        }

        var textAlign = (int)configuration.FloatingWindow.Align;
        if (ImGui.Combo(Translator.TrId("Settings_FWTab_TextAlign"), ref textAlign,
                Translator.Tr("Settings_FWTab_TextAlign_Left") + "###Left\0" +
                Translator.Tr("Settings_FWTab_TextAlign_Center") + "###Center\0" +
                Translator.Tr("Settings_FWTab_TextAlign_Right") + "###Right"))
        {
            configuration.FloatingWindow.Align = (ConfigurationFile.TextAlign)textAlign;
            configuration.Save();
        }

        var fontSize = configuration.FloatingWindow.FontSize;
        if (ImGui.InputInt(Translator.TrId("Settings_FWTab_FontSize"), ref fontSize, 4))
        {
            configuration.FloatingWindow.FontSize = Math.Max(0, fontSize);
            configuration.Save();

            if (configuration.FloatingWindow.FontSize >= 8) _ui.RebuildFonts();
        }

        ImGui.EndGroup();
        ImGui.SameLine();
        ImGui.BeginGroup();
        var floatingWindowTextColor = ImGuiComponents.ColorPickerWithPalette(1,
            Translator.TrId("Settings_FWTab_TextColor"),
            configuration.FloatingWindow.TextColor);
        if (floatingWindowTextColor != configuration.FloatingWindow.TextColor)
        {
            configuration.FloatingWindow.TextColor = floatingWindowTextColor;
            configuration.Save();
        }

        ImGui.SameLine();
        ImGui.Text(Translator.Tr("Settings_FWTab_TextColor"));

        var floatingWindowBackgroundColor = ImGuiComponents.ColorPickerWithPalette(2,
            Translator.TrId("Settings_FWTab_BackgroundColor"),
            configuration.FloatingWindow.BackgroundColor);
        if (floatingWindowBackgroundColor != configuration.FloatingWindow.BackgroundColor)
        {
            configuration.FloatingWindow.BackgroundColor = floatingWindowBackgroundColor;
            configuration.Save();
        }

        ImGui.SameLine();
        ImGui.Text(Translator.Tr("Settings_FWTab_BackgroundColor"));
        ImGui.EndGroup();

        ImGui.Unindent();
    }

    private void WebServerTabContent()
    {
        var configuration = Plugin.Config;
        var enableWebServer = configuration.WebServer.Enable;

        ImGui.PushTextWrapPos();
        ImGui.Text(Translator.Tr("Settings_Web_Help"));
        ImGui.Text(Translator.Tr("Settings_Web_HelpAdd"));

        ImGui.Text($"http://localhost:{configuration.WebServer.WebServer}/");
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Copy))
            ImGui.SetClipboardText($"http://localhost:{configuration.WebServer.WebServer}/");

        ImGui.Text(Translator.Tr("Settings_Web_HelpSize"));
        ImGui.PopTextWrapPos();
        ImGui.Separator();

        if (ImGui.Checkbox(Translator.TrId("Settings_Web_EnablePort"), ref enableWebServer))
        {
            configuration.WebServer.Enable = enableWebServer;
            configuration.Save();
        }

        ImGui.SameLine();
        var webServerPort = configuration.WebServer.WebServer;
        if (ImGui.InputInt("###EngageTimer_WebPort", ref webServerPort))
        {
            configuration.WebServer.WebServer = webServerPort;
            configuration.Save();
        }

        var enableWebStopwatchTimeout = configuration.WebServer.EnableStopwatchTimeout;
        if (ImGui.Checkbox(Translator.TrId("Settings_Web_Hide_Left"), ref enableWebStopwatchTimeout))
        {
            configuration.WebServer.EnableStopwatchTimeout = enableWebStopwatchTimeout;
            configuration.Save();
        }

        var webStopwatchTimeout = configuration.WebServer.StopwatchTimeout;
        ImGui.SameLine();
        if (ImGui.DragFloat(Translator.TrId("Settings_Web_Hide_Right"), ref webStopwatchTimeout))
        {
            configuration.WebServer.StopwatchTimeout = webStopwatchTimeout;
            configuration.Save();
        }
    }

    private void CountdownNumberStyle()
    {
        var numberTextures = Plugin.NumberTextures;
        var texture = numberTextures.GetTexture(_exampleNumber);
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
        if (ImGui.Combo("###DropDown_" + Translator.Tr("Settings_CountdownTab_Texture"), ref currentTexture, choiceString))
        {
            configuration.Countdown.TexturePreset = currentTexture < choices.Count() ? choices[currentTexture] : "";
            configuration.Save();
            numberTextures.Load();
        }

        ImGui.PopItemWidth();

        ImGui.SameLine();
        var monospaced = configuration.Countdown.Monospaced;
        if (ImGui.Checkbox(Translator.TrId("Settings_CountdownTab_Monospaced"), ref monospaced))
        {
            configuration.Countdown.Monospaced = monospaced;
            configuration.Save();
        }

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
                numberTextures.Load();
            }
        }

        if (ImGui.CollapsingHeader(Translator.TrId("Settings_CountdownTab_NumberStyleTitle"))) CountdownNumberColor();

        if (ImGui.CollapsingHeader(Translator.TrId("Settings_CountdownTab_NumberStyle_Advanced")))
        {
            var leading0 = configuration.Countdown.LeadingZero;
            if (ImGui.Checkbox(Translator.TrId("Settings_CountdownTab_NumberStyle_LeadingZero"), ref leading0))
            {
                configuration.Countdown.LeadingZero = leading0;
                configuration.Save();
            }

            var enableCustomNegativeMargin = configuration.Countdown.CustomNegativeMargin != null;
            if (ImGui.Checkbox(Translator.TrId("Settings_CountdownTab_NumberStyle_EnableCustomNegativeMargin"),
                    ref enableCustomNegativeMargin))
            {
                configuration.Countdown.CustomNegativeMargin = enableCustomNegativeMargin ? 20f : null;
                configuration.Save();
            }

            if (enableCustomNegativeMargin)
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
        // --- Luminance ---
        ImGui.PushItemWidth(250f);
        var configuration = Plugin.Config;
        var numberTextures = Plugin.NumberTextures;
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Undo.ToIconString() + "###reset_lum"))
        {
            configuration.Countdown.Luminance = 0f;
            numberTextures.CreateTextures();
            configuration.Save();
        }

        ImGui.SameLine();
        var b = configuration.Countdown.Luminance;
        if (ImGui.SliderFloat("± " + Translator.TrId("Settings_CountdownTab_NumberLuminance"), ref b, -1f, 1f))
        {
            configuration.Countdown.Luminance = Math.Clamp(b, -1f, 1f);
            numberTextures.CreateTextures();
            configuration.Save();
        }

        // --- Saturation ---
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Undo.ToIconString() + "###reset_sat"))
        {
            configuration.Countdown.Saturation = 0f;
            numberTextures.CreateTextures();
            configuration.Save();
        }

        ImGui.SameLine();
        var s = configuration.Countdown.Saturation;
        if (ImGui.SliderFloat("± " + Translator.TrId("Settings_CountdownTab_NumberSaturation"), ref s, -1f, 1f))
        {
            configuration.Countdown.Saturation = Math.Clamp(s, -1f, 1f);
            numberTextures.CreateTextures();
            configuration.Save();
        }

        // --- Hue ---
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Undo.ToIconString() + "###reset_hue"))
        {
            configuration.Countdown.Hue = 0;
            numberTextures.CreateTextures();
            configuration.Save();
        }

        var h = configuration.Countdown.Hue;
        ImGui.SameLine();
        if (configuration.Countdown.NumberRecolorMode)
        {
            ImGui.PushStyleColor(ImGuiCol.FrameBg, HslConv.HslToVector4Rgb(h, 0.3f, 0.3f));
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, HslConv.HslToVector4Rgb(h, 0.5f, 0.3f));
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, HslConv.HslToVector4Rgb(h, 0.7f, 0.3f));
        }

        if (ImGui.DragInt((configuration.Countdown.NumberRecolorMode ? "" : "± ") +
                          Translator.TrId("Settings_CountdownTab_NumberHue"), ref h, 1))
        {
            if (h > 360) h = 0;
            if (h < 0) h = 360;
            configuration.Countdown.Hue = h;
            numberTextures.CreateTextures();
            configuration.Save();
        }

        if (configuration.Countdown.NumberRecolorMode) ImGui.PopStyleColor(3);

        ImGui.PopItemWidth();

        var tint = configuration.Countdown.NumberRecolorMode;
        if (ImGui.Checkbox(Translator.TrId("Settings_CountdownTab_NumberRecolor"), ref tint))
        {
            configuration.Countdown.NumberRecolorMode = !configuration.Countdown.NumberRecolorMode;
            configuration.Save();
            numberTextures.CreateTextures();
        }
    }
}