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
using EngageTimer.Status;
using EngageTimer.Ui.Color;
using ImGuiNET;
using XwContainer;

namespace EngageTimer.Ui;

public class Settings : Window
{
    private readonly ConfigurationFile _configuration;
    private readonly NumberTextures _numberTextures;
    private readonly State _state;
    private readonly Translator _tr;
    private readonly UiBuilder _uiBuilder;
    private int _exampleNumber = 9;

    private bool _mocking;
    private double _mockStart;
    private double _mockTarget;

    private string _tempTexturePath;

    public Settings(Container container) : base("Settings", ImGuiWindowFlags.AlwaysAutoResize)
    {
        _configuration = container.Resolve<ConfigurationFile>();
        _uiBuilder = Bag.PluginInterface.UiBuilder;
        _numberTextures = container.Resolve<NumberTextures>();
        _state = container.Resolve<State>();
        _tr = container.Resolve<Translator>();
        _tr.LocaleChanged += (_, _) => UpdateWindowName();
        UpdateWindowName();
#if DEBUG
        IsOpen = true;
#endif
    }

    private void UpdateWindowName()
    {
        WindowName = TransId("Settings_Title");
    }

    private string TransId(string id)
    {
        return _tr.TransId(id);
    }

    private string Trans(string id)
    {
        return _tr.Trans(id);
    }

    private void ToggleMock()
    {
        _mocking = !_mocking;
        if (_mocking)
        {
            _state.Mocked = true;
            _state.InCombat = false;
            _state.CountDownValue = 12.23f;
            _state.CountingDown = true;
            _mockStart = ImGui.GetTime();
        }
        else
        {
            _state.Mocked = false;
        }
    }

    private void UpdateMock()
    {
        if (!_mocking) return;
        if (_mockTarget == 0 || _mockTarget < ImGui.GetTime()) _mockTarget = ImGui.GetTime() + 30d;

        _state.CountingDown = true;
        _state.CountDownValue = (float)(_mockTarget - ImGui.GetTime());
    }

    public override void Draw()
    {
        UpdateMock();
        if (ImGui.BeginTabBar("EngageTimerSettingsTabBar", ImGuiTabBarFlags.None))
        {
            ImGui.PushItemWidth(100f);
            if (ImGui.BeginTabItem(TransId("Settings_CountdownTab_Title")))
            {
                CountdownTabContent();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(TransId("Settings_FWTab_Title")))
            {
                FloatingWindowTabContent();
                ImGui.EndTabItem();
            }


            if (ImGui.BeginTabItem(TransId("Settings_DtrTab_Title")))
            {
                DtrTabContent();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(TransId("Settings_Web_Title")))
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
        if (ImGui.Button(TransId("Settings_Close"))) IsOpen = false;
    }

    private void DtrTabContent()
    {
        ImGui.PushTextWrapPos();
        ImGui.Text(Trans("Settings_DtrTab_Info"));
        ImGui.PopTextWrapPos();
        ImGui.Separator();

        var enabled = _configuration.Dtr.CombatTimeEnabled;
        if (ImGui.Checkbox(TransId("Settings_DtrCombatTimer_Enable"), ref enabled))
        {
            _configuration.Dtr.CombatTimeEnabled = enabled;
            _configuration.Save();
        }

        var prefix = _configuration.Dtr.CombatTimePrefix;
        if (ImGui.InputText(TransId("Settings_DtrCombatTimer_Prefix"), ref prefix, 50))
        {
            _configuration.Dtr.CombatTimePrefix = prefix;
            _configuration.Save();
        }

        ImGui.SameLine();
        var suffix = _configuration.Dtr.CombatTimeSuffix;
        if (ImGui.InputText(TransId("Settings_DtrCombatTimer_Suffix"), ref suffix, 50))
        {
            _configuration.Dtr.CombatTimeSuffix = suffix;
            _configuration.Save();
        }

        ImGui.SameLine();
        if (ImGui.Button(TransId("Settings_DtrCombatTimer_Defaults")))
        {
            _configuration.Dtr.CombatTimePrefix = DtrConfiguration.DefaultCombatTimePrefix;
            _configuration.Dtr.CombatTimeSuffix = DtrConfiguration.DefaultCombatTimeSuffix;
            _configuration.Save();
        }


        var outside = _configuration.Dtr.CombatTimeAlwaysDisableOutsideDuty;
        if (ImGui.Checkbox(TransId("Settings_DtrCombatTimer_AlwaysDisableOutsideDuty"), ref outside))
        {
            _configuration.Dtr.CombatTimeAlwaysDisableOutsideDuty = outside;
            _configuration.Save();
        }

        var decimals = _configuration.Dtr.CombatTimeDecimalPrecision;
        if (ImGui.InputInt(TransId("Settings_DtrCombatTimer_DecimalPrecision"), ref decimals, 1, 0))
        {
            _configuration.Dtr.CombatTimeDecimalPrecision = Math.Max(0, Math.Min(3, decimals));
            _configuration.Save();
        }

        var enableHideAfter = _configuration.Dtr.CombatTimeEnableHideAfter;
        if (ImGui.Checkbox(TransId("Settings_DtrCombatTimer_HideAfter"), ref enableHideAfter))
        {
            _configuration.Dtr.CombatTimeEnableHideAfter = enableHideAfter;
            _configuration.Save();
        }

        ImGui.SameLine();
        var hideAfter = _configuration.Dtr.CombatTimeHideAfter;
        if (ImGui.InputFloat(TransId("Settings_DtrCombatTimer_HideAfterRight"), ref hideAfter, 0.1f, 1f, "%.1f%"))
        {
            _configuration.Dtr.CombatTimeHideAfter = Math.Max(0, hideAfter);
            _configuration.Save();
        }
    }

    private void CountdownTabContent()
    {
        var countdownAccurateCountdown = _configuration.Countdown.AccurateMode;

        ImGui.PushTextWrapPos();
        ImGui.Text(Trans("Settings_CountdownTab_Info1"));
        if (ImGui.Button(
                (_mocking
                    ? Trans("Settings_CountdownTab_Test_Stop")
                    : Trans("Settings_CountdownTab_Test_Start"))
                + "###Settings_CountdownTab_Test"))
            ToggleMock();

        ImGui.PopTextWrapPos();
        ImGui.Separator();

        var displayCountdown = _configuration.Countdown.Display;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_Enable"),
                ref displayCountdown))
        {
            _configuration.Countdown.Display = displayCountdown;
            _configuration.Save();
        }

        var hideOriginalCountdown = _configuration.Countdown.HideOriginalAddon;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_HideOriginalCountDown"),
                ref hideOriginalCountdown))
        {
            _configuration.Countdown.HideOriginalAddon = hideOriginalCountdown;
            _configuration.Save();
        }

        ImGuiComponents.HelpMarker(Trans("Settings_CountdownTab_HideOriginalCountDown_Help"));

        var enableCountdownDecimal = _configuration.Countdown.EnableDecimals;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_CountdownDecimals_Left"),
                ref enableCountdownDecimal))
        {
            _configuration.Countdown.EnableDecimals = enableCountdownDecimal;
            _configuration.Save();
        }

        ImGui.SameLine();
        ImGui.PushItemWidth(70f);
        var countdownDecimalPrecision = _configuration.Countdown.DecimalPrecision;
        if (ImGui.InputInt(TransId("Settings_CountdownTab_CountdownDecimals_Right"),
                ref countdownDecimalPrecision, 1, 0))
        {
            countdownDecimalPrecision = Math.Max(1, Math.Min(3, countdownDecimalPrecision));
            _configuration.Countdown.DecimalPrecision = countdownDecimalPrecision;
            _configuration.Save();
        }

        ImGui.PopItemWidth();

        var enableTickingSound = _configuration.Countdown.EnableTickingSound;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_Audio_Enable"), ref enableTickingSound))
        {
            _configuration.Countdown.EnableTickingSound = enableTickingSound;
            _configuration.Save();
        }

        if (enableTickingSound)
        {
            ImGui.Indent();
            var alternativeSound = _configuration.Countdown.UseAlternativeSound;
            if (ImGui.Checkbox(TransId("Settings_CountdownTab_Audio_UseAlternativeSound"),
                    ref alternativeSound))
            {
                _configuration.Countdown.UseAlternativeSound = alternativeSound;
                _configuration.Save();
            }

            var tickFrom = _configuration.Countdown.StartTickingFrom;
            // ImGui.Text(Trans("Settings_CountdownTab_TickFrom"));
            if (ImGui.InputInt(TransId("Settings_CountdownTab_TickFrom"), ref tickFrom, 1, 0))
            {
                _configuration.Countdown.StartTickingFrom = Math.Min(30, Math.Max(5, tickFrom));
                _configuration.Save();
            }

            ImGuiComponents.HelpMarker(Trans("Settings_CountdownTab_TickFrom_Help"));

            ImGui.Unindent();
        }

        var animate = _configuration.Countdown.Animate;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_Animate"), ref animate))
        {
            _configuration.Countdown.Animate = animate;
            _configuration.Save();
            _numberTextures.CreateTextures();
        }

        if (animate)
        {
            ImGui.SameLine();
            var animateScale = _configuration.Countdown.AnimateScale;
            if (ImGui.Checkbox(TransId("Settings_CountdownTab_AnimateScale"), ref animateScale))
            {
                _configuration.Countdown.AnimateScale = animateScale;
                _configuration.Save();
                _numberTextures.CreateTextures();
            }

            ImGui.SameLine();
            var animateOpacity = _configuration.Countdown.AnimateOpacity;
            if (ImGui.Checkbox(TransId("Settings_CountdownTab_AnimateOpacity"), ref animateOpacity))
            {
                _configuration.Countdown.AnimateOpacity = animateOpacity;
                _configuration.Save();
                _numberTextures.CreateTextures();
            }
        }

        var enableCountdownDisplayThreshold = _configuration.Countdown.EnableDisplayThreshold;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_CountdownDisplayThreshold"),
                ref enableCountdownDisplayThreshold))
        {
            _configuration.Countdown.EnableDisplayThreshold = enableCountdownDisplayThreshold;
            _configuration.Save();
        }

        ImGui.SameLine();

        var countdownDisplayThreshold = _configuration.Countdown.DisplayThreshold;
        if (ImGui.InputInt("###Settings_CountdownTab_CountdownDisplayThreshold_Value",
                ref countdownDisplayThreshold, 1))
        {
            countdownDisplayThreshold = Math.Clamp(countdownDisplayThreshold, 0, 30);
            _configuration.Countdown.DisplayThreshold = countdownDisplayThreshold;
            _configuration.Save();
        }

        ImGui.SameLine();
        ImGuiComponents.HelpMarker(Trans("Settings_CountdownTab_CountdownDisplayThreshold_Help"));

        ImGui.Separator();
        if (ImGui.CollapsingHeader(TransId("Settings_CountdownTab_PositioningTitle"))) CountdownPositionAndSize();
        if (ImGui.CollapsingHeader(TransId("Settings_CountdownTab_Texture"), ImGuiTreeNodeFlags.DefaultOpen))
            CountdownNumberStyle();
        ImGui.Separator();

        var countdownAccurateCountdownDisabled = !_configuration.Countdown.HideOriginalAddon;
        if (countdownAccurateCountdownDisabled) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);

        if (ImGui.Checkbox(TransId("Settings_CountdownTab_AccurateMode"),
                ref countdownAccurateCountdown))
        {
            _configuration.Countdown.AccurateMode = countdownAccurateCountdown;
            _configuration.Save();
        }

        if (countdownAccurateCountdownDisabled) ImGui.PopStyleVar();

        ImGui.Indent();
        ImGui.PushTextWrapPos(500f);
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
        ImGui.TextWrapped(Trans("Settings_CountdownTab_AccurateMode_Help"));
        ImGui.PopTextWrapPos();
        ImGui.PopStyleColor();
        ImGui.Unindent();
    }

    private void CountdownPositionAndSize()
    {
        CountDown.ShowBackground = true;
        ImGui.Indent();
        if (!_configuration.Countdown.HideOriginalAddon)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            ImGui.TextWrapped(Trans("Settings_CountdownTab_PositionWarning"));
            ImGui.PopStyleColor();
        }

        ImGui.TextWrapped(Trans("Settings_CountdownTab_MultiMonitorWarning"));

        var countdownOffsetX = _configuration.Countdown.WindowOffset.X * 100;
        if (ImGui.DragFloat(TransId("Settings_CountdownTab_OffsetX"), ref countdownOffsetX, .1f))
        {
            _configuration.Countdown.WindowOffset =
                new Vector2(countdownOffsetX / 100, _configuration.Countdown.WindowOffset.Y);
            _configuration.Save();
        }

        ImGui.SameLine();

        var countdownOffsetY = _configuration.Countdown.WindowOffset.Y * 100;
        if (ImGui.DragFloat(TransId("Settings_CountdownTab_OffsetY"), ref countdownOffsetY, .1f))
        {
            _configuration.Countdown.WindowOffset =
                new Vector2(_configuration.Countdown.WindowOffset.X, countdownOffsetY / 100);
            _configuration.Save();
        }

        ImGui.SameLine();
        ImGui.Text(Trans("Settings_CountdownTab_OffsetText"));
        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Undo.ToIconString() + "###reset_cd_offset"))
        {
            _configuration.Countdown.WindowOffset = Vector2.Zero;
            _configuration.Save();
        }

        var countdownScale = _configuration.Countdown.Scale;
        ImGui.PushItemWidth(100f);
        if (ImGui.InputFloat(TransId("Settings_CountdownTab_CountdownScale"), ref countdownScale, .01f))
        {
            _configuration.Countdown.Scale = Math.Clamp(countdownScale, 0.05f, 15f);
            _configuration.Save();
        }

        ImGui.PopItemWidth();

        var align = (int)_configuration.Countdown.Align;
        if (ImGui.Combo(TransId("Settings_CountdownTab_CountdownAlign"), ref align,
                Trans("Settings_FWTab_TextAlign_Left") + "###Left\0" +
                Trans("Settings_FWTab_TextAlign_Center") + "###Center\0" +
                Trans("Settings_FWTab_TextAlign_Right") + "###Right"))
        {
            _configuration.Countdown.Align = (ConfigurationFile.TextAlign)align;
            _configuration.Save();
        }


        ImGui.Unindent();
    }

    private void FloatingWindowTabContent()
    {
        var floatingWindowAccurateCountdown = _configuration.FloatingWindow.AccurateMode;

        ImGui.PushTextWrapPos();
        ImGui.Text(Trans("Settings_FWTab_Help"));
        ImGui.PopTextWrapPos();
        ImGui.Separator();

        var displayFloatingWindow = _configuration.FloatingWindow.Display;
        if (ImGui.Checkbox(TransId("Settings_FWTab_Display"), ref displayFloatingWindow))
        {
            _configuration.FloatingWindow.Display = displayFloatingWindow;
            _configuration.Save();
        }

        var floatingWindowLock = _configuration.FloatingWindow.Lock;
        if (ImGui.Checkbox(TransId("Settings_FWTab_Lock"), ref floatingWindowLock))
        {
            _configuration.FloatingWindow.Lock = floatingWindowLock;
            _configuration.Save();
        }

        ImGuiComponents.HelpMarker(Trans("Settings_FWTab_Lock_Help"));

        var autoHideStopwatch = _configuration.FloatingWindow.AutoHide;
        if (ImGui.Checkbox(TransId("Settings_FWTab_AutoHide_Left"), ref autoHideStopwatch))
        {
            _configuration.FloatingWindow.AutoHide = autoHideStopwatch;
            _configuration.Save();
        }

        var autoHideTimeout = _configuration.FloatingWindow.AutoHideTimeout;
        ImGui.SameLine();
        if (ImGui.InputFloat(TransId("Settings_FWTab_AutoHide_Right"), ref autoHideTimeout, .1f, 1f,
                "%.1f%"))
        {
            _configuration.FloatingWindow.AutoHideTimeout = Math.Max(0, autoHideTimeout);
            _configuration.Save();
        }

        ImGui.Separator();

        var floatingWindowCountdown = _configuration.FloatingWindow.EnableCountdown;
        if (ImGui.Checkbox(
                TransId("Settings_FWTab_CountdownPrecision" +
                        (floatingWindowCountdown ? "_With" : "") + "_Left"),
                ref floatingWindowCountdown))
        {
            _configuration.FloatingWindow.EnableCountdown = floatingWindowCountdown;
            _configuration.Save();
        }

        if (floatingWindowCountdown)
        {
            ImGui.SameLine();
            ImGui.PushItemWidth(70f);
            var fwDecimalCountdownPrecision = _configuration.FloatingWindow.DecimalCountdownPrecision;
            // the little space is necessary because imgui id's the fields by label
            if (ImGui.InputInt(
                    TransId("Settings_FWTab_CountdownPrecision_Right"),
                    ref fwDecimalCountdownPrecision, 1, 0))
            {
                fwDecimalCountdownPrecision = Math.Max(0, Math.Min(3, fwDecimalCountdownPrecision));
                _configuration.FloatingWindow.DecimalCountdownPrecision = fwDecimalCountdownPrecision;
                _configuration.Save();
            }

            ImGui.PopItemWidth();
        }

        ImGuiComponents.HelpMarker(Trans("Settings_FWTab_CountdownPrecision_Help"));

        var floatingWindowStopwatch = _configuration.FloatingWindow.EnableStopwatch;
        if (ImGui.Checkbox(
                TransId("Settings_FWTab_StopwatchPrecision" +
                        (floatingWindowStopwatch ? "_With" : "") + "_Left"),
                ref floatingWindowStopwatch))
        {
            _configuration.FloatingWindow.EnableStopwatch = floatingWindowStopwatch;
            _configuration.Save();
        }

        if (floatingWindowStopwatch)
        {
            ImGui.SameLine();
            ImGui.PushItemWidth(70f);
            var fwDecimalStopwatchPrecision = _configuration.FloatingWindow.DecimalStopwatchPrecision;
            if (ImGui.InputInt(TransId("Settings_FWTab_StopwatchPrecision_Right"),
                    ref fwDecimalStopwatchPrecision, 1, 0))
            {
                fwDecimalStopwatchPrecision = Math.Max(0, Math.Min(3, fwDecimalStopwatchPrecision));
                _configuration.FloatingWindow.DecimalStopwatchPrecision = fwDecimalStopwatchPrecision;
                _configuration.Save();
            }

            ImGui.PopItemWidth();
        }

        ImGuiComponents.HelpMarker(Trans("Settings_FWTab_StopwatchPrecision_Help"));

        ImGui.Separator();
        if (ImGui.CollapsingHeader(TransId("Settings_FWTab_Styling"))) FwStyling();
        ImGui.Separator();

        if (ImGui.Checkbox(TransId("Settings_FWTab_AccurateCountdown"),
                ref floatingWindowAccurateCountdown))
        {
            _configuration.FloatingWindow.AccurateMode = floatingWindowAccurateCountdown;
            _configuration.Save();
        }

        ImGuiComponents.HelpMarker(Trans("Settings_FWTab_AccurateCountdown_Help"));

        var fWDisplayStopwatchOnlyInDuty = _configuration.FloatingWindow.StopwatchOnlyInDuty;
        if (ImGui.Checkbox(TransId("Settings_FWTab_DisplayStopwatchOnlyInDuty"),
                ref fWDisplayStopwatchOnlyInDuty))
        {
            _configuration.FloatingWindow.StopwatchOnlyInDuty = fWDisplayStopwatchOnlyInDuty;
            _configuration.Save();
        }

        ImGuiComponents.HelpMarker(Trans("Settings_FWTab_DisplayStopwatchOnlyInDuty_Help"));

        var negativeSign = _configuration.FloatingWindow.CountdownNegativeSign;
        if (ImGui.Checkbox(TransId("Settings_FWTab_CountdownNegativeSign"), ref negativeSign))
        {
            _configuration.FloatingWindow.CountdownNegativeSign = negativeSign;
            _configuration.Save();
        }

        var displaySeconds = _configuration.FloatingWindow.StopwatchAsSeconds;
        if (ImGui.Checkbox(TransId("Settings_FWTab_StopwatchAsSeconds"), ref displaySeconds))
        {
            _configuration.FloatingWindow.StopwatchAsSeconds = displaySeconds;
            _configuration.Save();
        }

        var prePullWarning = _configuration.FloatingWindow.ShowPrePulling;
        if (ImGui.Checkbox(TransId("Settings_FWTab_ShowPrePulling"), ref prePullWarning))
        {
            _configuration.FloatingWindow.ShowPrePulling = prePullWarning;
            _configuration.Save();
        }

        ImGuiComponents.HelpMarker(Trans("Settings_FWTab_ShowPrePulling_Help"));

        if (prePullWarning)
        {
            ImGui.Indent();
            var offset = _configuration.FloatingWindow.PrePullOffset;
            ImGui.PushItemWidth(110f);
            if (ImGui.InputFloat(Trans("Settings_FWTab_PrePullOffset"), ref offset, 0.1f, 1f, "%.3fs"))
            {
                _configuration.FloatingWindow.PrePullOffset = offset;
                _configuration.Save();
            }

            ImGui.PopItemWidth();
            ImGuiComponents.HelpMarker(Trans("Settings_FWTab_PrePullOffset_Help"));

            ImGui.SameLine();
            var prePullColor = ImGuiComponents.ColorPickerWithPalette(10,
                TransId("Settings_FWTab_TextColor"),
                _configuration.FloatingWindow.PrePullColor);
            if (prePullColor != _configuration.FloatingWindow.PrePullColor)
            {
                _configuration.FloatingWindow.PrePullColor = prePullColor;
                _configuration.Save();
            }

            ImGui.SameLine();
            ImGui.Text(Trans("Settings_FWTab_TextColor"));

            ImGui.Unindent();
        }
    }

    private void FwStyling()
    {
        ImGui.Indent();

        ImGui.BeginGroup();
        var fwScale = _configuration.FloatingWindow.Scale;
        ImGui.PushItemWidth(100f);
        if (ImGui.DragFloat(TransId("Settings_CountdownTab_FloatingWindowScale"), ref fwScale, .01f))
        {
            _configuration.FloatingWindow.Scale = Math.Clamp(fwScale, 0.05f, 15f);
            _configuration.Save();
        }

        var textAlign = (int)_configuration.FloatingWindow.Align;
        if (ImGui.Combo(TransId("Settings_FWTab_TextAlign"), ref textAlign,
                Trans("Settings_FWTab_TextAlign_Left") + "###Left\0" +
                Trans("Settings_FWTab_TextAlign_Center") + "###Center\0" +
                Trans("Settings_FWTab_TextAlign_Right") + "###Right"))
        {
            _configuration.FloatingWindow.Align = (ConfigurationFile.TextAlign)textAlign;
            _configuration.Save();
        }

        var fontSize = _configuration.FloatingWindow.FontSize;
        if (ImGui.InputInt(TransId("Settings_FWTab_FontSize"), ref fontSize, 4))
        {
            _configuration.FloatingWindow.FontSize = Math.Max(0, fontSize);
            _configuration.Save();

            if (_configuration.FloatingWindow.FontSize >= 8) _uiBuilder.RebuildFonts();
        }

        ImGui.EndGroup();
        ImGui.SameLine();
        ImGui.BeginGroup();
        var floatingWindowTextColor = ImGuiComponents.ColorPickerWithPalette(1,
            TransId("Settings_FWTab_TextColor"),
            _configuration.FloatingWindow.TextColor);
        if (floatingWindowTextColor != _configuration.FloatingWindow.TextColor)
        {
            _configuration.FloatingWindow.TextColor = floatingWindowTextColor;
            _configuration.Save();
        }

        ImGui.SameLine();
        ImGui.Text(Trans("Settings_FWTab_TextColor"));

        var floatingWindowBackgroundColor = ImGuiComponents.ColorPickerWithPalette(2,
            TransId("Settings_FWTab_BackgroundColor"),
            _configuration.FloatingWindow.BackgroundColor);
        if (floatingWindowBackgroundColor != _configuration.FloatingWindow.BackgroundColor)
        {
            _configuration.FloatingWindow.BackgroundColor = floatingWindowBackgroundColor;
            _configuration.Save();
        }

        ImGui.SameLine();
        ImGui.Text(Trans("Settings_FWTab_BackgroundColor"));
        ImGui.EndGroup();

        ImGui.Unindent();
    }

    private void WebServerTabContent()
    {
        var enableWebServer = _configuration.WebServer.Enable;

        ImGui.PushTextWrapPos();
        ImGui.Text(Trans("Settings_Web_Help"));
        ImGui.Text(Trans("Settings_Web_HelpAdd"));

        ImGui.Text($"http://localhost:{_configuration.WebServer.WebServer}/");
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Copy))
            ImGui.SetClipboardText($"http://localhost:{_configuration.WebServer.WebServer}/");

        ImGui.Text(Trans("Settings_Web_HelpSize"));
        ImGui.PopTextWrapPos();
        ImGui.Separator();

        if (ImGui.Checkbox(TransId("Settings_Web_EnablePort"), ref enableWebServer))
        {
            _configuration.WebServer.Enable = enableWebServer;
            _configuration.Save();
        }

        ImGui.SameLine();
        var webServerPort = _configuration.WebServer.WebServer;
        if (ImGui.InputInt("###EngageTimer_WebPort", ref webServerPort))
        {
            _configuration.WebServer.WebServer = webServerPort;
            _configuration.Save();
        }

        var enableWebStopwatchTimeout = _configuration.WebServer.EnableStopwatchTimeout;
        if (ImGui.Checkbox(TransId("Settings_Web_Hide_Left"), ref enableWebStopwatchTimeout))
        {
            _configuration.WebServer.EnableStopwatchTimeout = enableWebStopwatchTimeout;
            _configuration.Save();
        }

        var webStopwatchTimeout = _configuration.WebServer.StopwatchTimeout;
        ImGui.SameLine();
        if (ImGui.DragFloat(TransId("Settings_Web_Hide_Right"), ref webStopwatchTimeout))
        {
            _configuration.WebServer.StopwatchTimeout = webStopwatchTimeout;
            _configuration.Save();
        }
    }

    private void CountdownNumberStyle()
    {
        var texture = _numberTextures.GetTexture(_exampleNumber);
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
        for (var i = 0; i < choices.Count(); i++)
        {
            choiceString += _tr.TransId("Settings_CountdownTab_Texture_" + choices[i], choices[i]) + "\0";
            if (_configuration.Countdown.TexturePreset == choices[i]) currentTexture = i;
        }

        ImGui.BeginGroup();
        ImGui.PushItemWidth(200f);
        choiceString += TransId("Settings_CountdownTab_Texture_custom");
        if (ImGui.Combo("###DropDown_" + Trans("Settings_CountdownTab_Texture"), ref currentTexture, choiceString))
        {
            _configuration.Countdown.TexturePreset = currentTexture < choices.Count() ? choices[currentTexture] : "";
            _configuration.Save();
            _numberTextures.Load();
        }

        ImGui.PopItemWidth();

        ImGui.SameLine();
        var monospaced = _configuration.Countdown.Monospaced;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_Monospaced"), ref monospaced))
        {
            _configuration.Countdown.Monospaced = monospaced;
            _configuration.Save();
        }

        if (_configuration.Countdown.TexturePreset == "")
        {
            _tempTexturePath ??= _configuration.Countdown.TextureDirectory ?? "";

            ImGui.PushItemWidth(400f);
            ImGui.InputText(TransId("Settings_CountdownTab_Texture_Custom_Path"), ref _tempTexturePath, 1024);
            ImGui.PopItemWidth();
            if (ImGui.Button(TransId("Settings_CountdownTab_Texture_Custom_Load")))
            {
                _configuration.Countdown.TextureDirectory = _tempTexturePath;
                _configuration.Save();
                _numberTextures.Load();
            }
        }

        if (ImGui.CollapsingHeader(TransId("Settings_CountdownTab_NumberStyleTitle"))) CountdownNumberColor();

        if (ImGui.CollapsingHeader(TransId("Settings_CountdownTab_NumberStyle_Advanced")))
        {
            var leading0 = _configuration.Countdown.LeadingZero;
            if (ImGui.Checkbox(TransId("Settings_CountdownTab_NumberStyle_LeadingZero"), ref leading0))
            {
                _configuration.Countdown.LeadingZero = leading0;
                _configuration.Save();
            }

            var enableCustomNegativeMargin = _configuration.Countdown.CustomNegativeMargin != null;
            if (ImGui.Checkbox(TransId("Settings_CountdownTab_NumberStyle_EnableCustomNegativeMargin"),
                    ref enableCustomNegativeMargin))
            {
                _configuration.Countdown.CustomNegativeMargin = enableCustomNegativeMargin ? 20f : null;
                _configuration.Save();
            }

            if (enableCustomNegativeMargin)
            {
                ImGui.Indent();
                ImGui.PushItemWidth(100f);
                var nm = _configuration.Countdown.CustomNegativeMargin ?? 20f;
                if (ImGui.InputFloat(TransId("Settings_CountdownTab_NumberStyle_CustomNegativeMargin"), ref nm, 1f))
                {
                    _configuration.Countdown.CustomNegativeMargin = nm;
                    _configuration.Save();
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
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Undo.ToIconString() + "###reset_lum"))
        {
            _configuration.Countdown.Luminance = 0f;
            _numberTextures.CreateTextures();
            _configuration.Save();
        }

        ImGui.SameLine();
        var b = _configuration.Countdown.Luminance;
        if (ImGui.SliderFloat("± " + TransId("Settings_CountdownTab_NumberLuminance"), ref b, -1f, 1f))
        {
            _configuration.Countdown.Luminance = Math.Clamp(b, -1f, 1f);
            _numberTextures.CreateTextures();
            _configuration.Save();
        }

        // --- Saturation ---
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Undo.ToIconString() + "###reset_sat"))
        {
            _configuration.Countdown.Saturation = 0f;
            _numberTextures.CreateTextures();
            _configuration.Save();
        }

        ImGui.SameLine();
        var s = _configuration.Countdown.Saturation;
        if (ImGui.SliderFloat("± " + TransId("Settings_CountdownTab_NumberSaturation"), ref s, -1f, 1f))
        {
            _configuration.Countdown.Saturation = Math.Clamp(s, -1f, 1f);
            _numberTextures.CreateTextures();
            _configuration.Save();
        }

        // --- Hue ---
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Undo.ToIconString() + "###reset_hue"))
        {
            _configuration.Countdown.Hue = 0;
            _numberTextures.CreateTextures();
            _configuration.Save();
        }

        var h = _configuration.Countdown.Hue;
        ImGui.SameLine();
        if (_configuration.Countdown.NumberRecolorMode)
        {
            ImGui.PushStyleColor(ImGuiCol.FrameBg, HslConv.HslToVector4Rgb(h, 0.3f, 0.3f));
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, HslConv.HslToVector4Rgb(h, 0.5f, 0.3f));
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, HslConv.HslToVector4Rgb(h, 0.7f, 0.3f));
        }

        if (ImGui.DragInt((_configuration.Countdown.NumberRecolorMode ? "" : "± ") +
                          TransId("Settings_CountdownTab_NumberHue"), ref h, 1))
        {
            if (h > 360) h = 0;
            if (h < 0) h = 360;
            _configuration.Countdown.Hue = h;
            _numberTextures.CreateTextures();
            _configuration.Save();
        }

        if (_configuration.Countdown.NumberRecolorMode) ImGui.PopStyleColor(3);

        ImGui.PopItemWidth();

        var tint = _configuration.Countdown.NumberRecolorMode;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_NumberRecolor"), ref tint))
        {
            _configuration.Countdown.NumberRecolorMode = !_configuration.Countdown.NumberRecolorMode;
            _configuration.Save();
            _numberTextures.CreateTextures();
        }
    }
}