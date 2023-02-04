using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using EngageTimer.Status;
using EngageTimer.Ui.Color;
using ImGuiNET;
using XwContainer;

namespace EngageTimer.Ui;

public class Settings : Window
{
    private readonly Configuration _configuration;
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
        _configuration = container.Resolve<Configuration>();
        _uiBuilder = container.Resolve<UiBuilder>();
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

        var enabled = _configuration.DtrCombatTimeEnabled;
        if (ImGui.Checkbox(TransId("Settings_DtrCombatTimer_Enable"), ref enabled))
        {
            _configuration.DtrCombatTimeEnabled = enabled;
            _configuration.Save();
        }

        var prefix = _configuration.DtrCombatTimePrefix;
        if (ImGui.InputText(TransId("Settings_DtrCombatTimer_Prefix"), ref prefix, 50))
        {
            _configuration.DtrCombatTimePrefix = prefix;
            _configuration.Save();
        }

        ImGui.SameLine();
        var suffix = _configuration.DtrCombatTimeSuffix;
        if (ImGui.InputText(TransId("Settings_DtrCombatTimer_Suffix"), ref suffix, 50))
        {
            _configuration.DtrCombatTimeSuffix = suffix;
            _configuration.Save();
        }

        ImGui.SameLine();
        if (ImGui.Button(TransId("Settings_DtrCombatTimer_Defaults")))
        {
            _configuration.DtrCombatTimePrefix = Configuration.DefaultCombatTimePrefix;
            _configuration.DtrCombatTimeSuffix = Configuration.DefaultCombatTimeSuffix;
            _configuration.Save();
        }


        var outside = _configuration.DtrCombatTimeAlwaysDisableOutsideDuty;
        if (ImGui.Checkbox(TransId("Settings_DtrCombatTimer_AlwaysDisableOutsideDuty"), ref outside))
        {
            _configuration.DtrCombatTimeAlwaysDisableOutsideDuty = outside;
            _configuration.Save();
        }

        var decimals = _configuration.DtrCombatTimeDecimalPrecision;
        if (ImGui.InputInt(TransId("Settings_DtrCombatTimer_DecimalPrecision"), ref decimals, 1, 0))
        {
            _configuration.DtrCombatTimeDecimalPrecision = Math.Max(0, Math.Min(3, decimals));
            _configuration.Save();
        }

        var enableHideAfter = _configuration.DtrCombatTimeEnableHideAfter;
        if (ImGui.Checkbox(TransId("Settings_DtrCombatTimer_HideAfter"), ref enableHideAfter))
        {
            _configuration.DtrCombatTimeEnableHideAfter = enableHideAfter;
            _configuration.Save();
        }

        ImGui.SameLine();
        var hideAfter = _configuration.DtrCombatTimeHideAfter;
        if (ImGui.InputFloat(TransId("Settings_DtrCombatTimer_HideAfterRight"), ref hideAfter, 0.1f, 1f, "%.1f%"))
        {
            _configuration.DtrCombatTimeHideAfter = Math.Max(0, hideAfter);
            _configuration.Save();
        }
    }

    private void CountdownTabContent()
    {
        var countdownAccurateCountdown = _configuration.CountdownAccurateCountdown;

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

        var displayCountdown = _configuration.DisplayCountdown;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_Enable"),
                ref displayCountdown))
        {
            _configuration.DisplayCountdown = displayCountdown;
            _configuration.Save();
        }

        var hideOriginalCountdown = _configuration.HideOriginalCountdown;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_HideOriginalCountDown"),
                ref hideOriginalCountdown))
        {
            _configuration.HideOriginalCountdown = hideOriginalCountdown;
            _configuration.Save();
        }

        ImGuiComponents.HelpMarker(Trans("Settings_CountdownTab_HideOriginalCountDown_Help"));

        var enableCountdownDecimal = _configuration.EnableCountdownDecimal;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_CountdownDecimals_Left"),
                ref enableCountdownDecimal))
        {
            _configuration.EnableCountdownDecimal = enableCountdownDecimal;
            _configuration.Save();
        }

        ImGui.SameLine();
        ImGui.PushItemWidth(70f);
        var countdownDecimalPrecision = _configuration.CountdownDecimalPrecision;
        if (ImGui.InputInt(TransId("Settings_CountdownTab_CountdownDecimals_Right"),
                ref countdownDecimalPrecision, 1, 0))
        {
            countdownDecimalPrecision = Math.Max(1, Math.Min(3, countdownDecimalPrecision));
            _configuration.CountdownDecimalPrecision = countdownDecimalPrecision;
            _configuration.Save();
        }

        ImGui.PopItemWidth();

        var enableTickingSound = _configuration.EnableTickingSound;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_Audio_Enable"), ref enableTickingSound))
        {
            _configuration.EnableTickingSound = enableTickingSound;
            _configuration.Save();
        }

        if (enableTickingSound)
        {
            ImGui.Indent();
            var alternativeSound = _configuration.UseAlternativeSound;
            if (ImGui.Checkbox(TransId("Settings_CountdownTab_Audio_UseAlternativeSound"),
                    ref alternativeSound))
            {
                _configuration.UseAlternativeSound = alternativeSound;
                _configuration.Save();
            }

            ImGui.Unindent();
        }

        var animate = _configuration.CountdownAnimate;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_Animate"), ref animate))
        {
            _configuration.CountdownAnimate = animate;
            _configuration.Save();
            _numberTextures.CreateTextures();
        }

        if (animate)
        {
            ImGui.SameLine();
            var animateScale = _configuration.CountdownAnimateScale;
            if (ImGui.Checkbox(TransId("Settings_CountdownTab_AnimateScale"), ref animateScale))
            {
                _configuration.CountdownAnimateScale = animateScale;
                _configuration.Save();
                _numberTextures.CreateTextures();
            }

            ImGui.SameLine();
            var animateOpacity = _configuration.CountdownAnimateOpacity;
            if (ImGui.Checkbox(TransId("Settings_CountdownTab_AnimateOpacity"), ref animateOpacity))
            {
                _configuration.CountdownAnimateOpacity = animateOpacity;
                _configuration.Save();
                _numberTextures.CreateTextures();
            }
        }

        var enableCountdownDisplayThreshold = _configuration.EnableCountdownDisplayThreshold;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_CountdownDisplayThreshold"),
                ref enableCountdownDisplayThreshold))
        {
            _configuration.EnableCountdownDisplayThreshold = enableCountdownDisplayThreshold;
            _configuration.Save();
        }

        ImGui.SameLine();

        var countdownDisplayThreshold = _configuration.CountdownDisplayThreshold;
        if (ImGui.InputInt("###Settings_CountdownTab_CountdownDisplayThreshold_Value",
                ref countdownDisplayThreshold, 1))
        {
            countdownDisplayThreshold = Math.Clamp(countdownDisplayThreshold, 0, 30);
            _configuration.CountdownDisplayThreshold = countdownDisplayThreshold;
            _configuration.Save();
        }

        ImGui.SameLine();
        ImGuiComponents.HelpMarker(Trans("Settings_CountdownTab_CountdownDisplayThreshold_Help"));

        ImGui.Separator();
        if (ImGui.CollapsingHeader(TransId("Settings_CountdownTab_PositioningTitle"))) CountdownPositionAndSize();
        if (ImGui.CollapsingHeader(TransId("Settings_CountdownTab_Texture"), ImGuiTreeNodeFlags.DefaultOpen))
            CountdownNumberStyle();
        ImGui.Separator();

        var countdownAccurateCountdownDisabled = !_configuration.HideOriginalCountdown;
        if (countdownAccurateCountdownDisabled) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);

        if (ImGui.Checkbox(TransId("Settings_CountdownTab_AccurateMode"),
                ref countdownAccurateCountdown))
        {
            _configuration.CountdownAccurateCountdown = countdownAccurateCountdown;
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
        if (!_configuration.HideOriginalCountdown)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            ImGui.TextWrapped(Trans("Settings_CountdownTab_PositionWarning"));
            ImGui.PopStyleColor();
        }

        ImGui.TextWrapped(Trans("Settings_CountdownTab_MultiMonitorWarning"));

        var countdownOffsetX = _configuration.CountdownWindowOffset.X * 100;
        if (ImGui.DragFloat(TransId("Settings_CountdownTab_OffsetX"), ref countdownOffsetX, .1f))
        {
            _configuration.CountdownWindowOffset =
                new Vector2(countdownOffsetX / 100, _configuration.CountdownWindowOffset.Y);
            _configuration.Save();
        }

        ImGui.SameLine();

        var countdownOffsetY = _configuration.CountdownWindowOffset.Y * 100;
        if (ImGui.DragFloat(TransId("Settings_CountdownTab_OffsetY"), ref countdownOffsetY, .1f))
        {
            _configuration.CountdownWindowOffset =
                new Vector2(_configuration.CountdownWindowOffset.X, countdownOffsetY / 100);
            _configuration.Save();
        }

        ImGui.SameLine();
        ImGui.Text(Trans("Settings_CountdownTab_OffsetText"));
        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Undo.ToIconString() + "###reset_cd_offset"))
        {
            _configuration.CountdownWindowOffset = Vector2.Zero;
            _configuration.Save();
        }

        var countdownScale = _configuration.CountdownScale;
        ImGui.PushItemWidth(100f);
        if (ImGui.InputFloat(TransId("Settings_CountdownTab_CountdownScale"), ref countdownScale, .01f))
        {
            _configuration.CountdownScale = Math.Clamp(countdownScale, 0.05f, 15f);
            _configuration.Save();
        }

        ImGui.PopItemWidth();

        var align = (int)_configuration.CountdownAlign;
        if (ImGui.Combo(TransId("Settings_CountdownTab_CountdownAlign"), ref align,
                Trans("Settings_FWTab_TextAlign_Left") + "###Left\0" +
                Trans("Settings_FWTab_TextAlign_Center") + "###Center\0" +
                Trans("Settings_FWTab_TextAlign_Right") + "###Right"))
        {
            _configuration.CountdownAlign = (Configuration.TextAlign)align;
            _configuration.Save();
        }


        ImGui.Unindent();
    }

    private void FloatingWindowTabContent()
    {
        var floatingWindowAccurateCountdown = _configuration.FloatingWindowAccurateCountdown;

        ImGui.PushTextWrapPos();
        ImGui.Text(Trans("Settings_FWTab_Help"));
        ImGui.PopTextWrapPos();
        ImGui.Separator();

        var displayFloatingWindow = _configuration.DisplayFloatingWindow;
        if (ImGui.Checkbox(TransId("Settings_FWTab_Display"), ref displayFloatingWindow))
        {
            _configuration.DisplayFloatingWindow = displayFloatingWindow;
            _configuration.Save();
        }

        var floatingWindowLock = _configuration.FloatingWindowLock;
        if (ImGui.Checkbox(TransId("Settings_FWTab_Lock"), ref floatingWindowLock))
        {
            _configuration.FloatingWindowLock = floatingWindowLock;
            _configuration.Save();
        }

        ImGuiComponents.HelpMarker(Trans("Settings_FWTab_Lock_Help"));

        var autoHideStopwatch = _configuration.AutoHideStopwatch;
        if (ImGui.Checkbox(TransId("Settings_FWTab_AutoHide_Left"), ref autoHideStopwatch))
        {
            _configuration.AutoHideStopwatch = autoHideStopwatch;
            _configuration.Save();
        }

        var autoHideTimeout = _configuration.AutoHideTimeout;
        ImGui.SameLine();
        if (ImGui.InputFloat(TransId("Settings_FWTab_AutoHide_Right"), ref autoHideTimeout, .1f, 1f,
                "%.1f%"))
        {
            _configuration.AutoHideTimeout = Math.Max(0, autoHideTimeout);
            _configuration.Save();
        }

        ImGui.Separator();

        var floatingWindowCountdown = _configuration.FloatingWindowCountdown;
        if (ImGui.Checkbox(
                TransId("Settings_FWTab_CountdownPrecision" +
                        (floatingWindowCountdown ? "_With" : "") + "_Left"),
                ref floatingWindowCountdown))
        {
            _configuration.FloatingWindowCountdown = floatingWindowCountdown;
            _configuration.Save();
        }

        if (floatingWindowCountdown)
        {
            ImGui.SameLine();
            ImGui.PushItemWidth(70f);
            var fwDecimalCountdownPrecision = _configuration.FloatingWindowDecimalCountdownPrecision;
            // the little space is necessary because imgui id's the fields by label
            if (ImGui.InputInt(
                    TransId("Settings_FWTab_CountdownPrecision_Right"),
                    ref fwDecimalCountdownPrecision, 1, 0))
            {
                fwDecimalCountdownPrecision = Math.Max(0, Math.Min(3, fwDecimalCountdownPrecision));
                _configuration.FloatingWindowDecimalCountdownPrecision = fwDecimalCountdownPrecision;
                _configuration.Save();
            }

            ImGui.PopItemWidth();
        }

        ImGuiComponents.HelpMarker(Trans("Settings_FWTab_CountdownPrecision_Help"));

        var floatingWindowStopwatch = _configuration.FloatingWindowStopwatch;
        if (ImGui.Checkbox(
                TransId("Settings_FWTab_StopwatchPrecision" +
                        (floatingWindowStopwatch ? "_With" : "") + "_Left"),
                ref floatingWindowStopwatch))
        {
            _configuration.FloatingWindowStopwatch = floatingWindowStopwatch;
            _configuration.Save();
        }

        if (floatingWindowStopwatch)
        {
            ImGui.SameLine();
            ImGui.PushItemWidth(70f);
            var fwDecimalStopwatchPrecision = _configuration.FloatingWindowDecimalStopwatchPrecision;
            if (ImGui.InputInt(TransId("Settings_FWTab_StopwatchPrecision_Right"),
                    ref fwDecimalStopwatchPrecision, 1, 0))
            {
                fwDecimalStopwatchPrecision = Math.Max(0, Math.Min(3, fwDecimalStopwatchPrecision));
                _configuration.FloatingWindowDecimalStopwatchPrecision = fwDecimalStopwatchPrecision;
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
            _configuration.FloatingWindowAccurateCountdown = floatingWindowAccurateCountdown;
            _configuration.Save();
        }

        ImGuiComponents.HelpMarker(Trans("Settings_FWTab_AccurateCountdown_Help"));

        var fWDisplayStopwatchOnlyInDuty = _configuration.FloatingWindowDisplayStopwatchOnlyInDuty;
        if (ImGui.Checkbox(TransId("Settings_FWTab_DisplayStopwatchOnlyInDuty"),
                ref fWDisplayStopwatchOnlyInDuty))
        {
            _configuration.FloatingWindowDisplayStopwatchOnlyInDuty = fWDisplayStopwatchOnlyInDuty;
            _configuration.Save();
        }

        ImGuiComponents.HelpMarker(Trans("Settings_FWTab_DisplayStopwatchOnlyInDuty_Help"));

        var negativeSign = _configuration.FloatingWindowCountdownNegativeSign;
        if (ImGui.Checkbox(TransId("Settings_FWTab_CountdownNegativeSign"), ref negativeSign))
        {
            _configuration.FloatingWindowCountdownNegativeSign = negativeSign;
            _configuration.Save();
        }

        var displaySeconds = _configuration.FloatingWindowStopwatchAsSeconds;
        if (ImGui.Checkbox(TransId("Settings_FWTab_StopwatchAsSeconds"), ref displaySeconds))
        {
            _configuration.FloatingWindowStopwatchAsSeconds = displaySeconds;
            _configuration.Save();
        }

        var prePullWarning = _configuration.FloatingWindowShowPrePulling;
        if (ImGui.Checkbox(TransId("Settings_FWTab_ShowPrePulling"), ref prePullWarning))
        {
            _configuration.FloatingWindowShowPrePulling = prePullWarning;
            _configuration.Save();
        }

        ImGuiComponents.HelpMarker(Trans("Settings_FWTab_ShowPrePulling_Help"));

        if (prePullWarning)
        {
            ImGui.Indent();
            var offset = _configuration.FloatingWindowPrePullOffset;
            ImGui.PushItemWidth(110f);
            if (ImGui.InputFloat(Trans("Settings_FWTab_PrePullOffset"), ref offset, 0.1f, 1f, "%.3fs"))
            {
                _configuration.FloatingWindowPrePullOffset = offset;
                _configuration.Save();
            }

            ImGui.PopItemWidth();
            ImGuiComponents.HelpMarker(Trans("Settings_FWTab_PrePullOffset_Help"));

            ImGui.SameLine();
            var prePullColor = ImGuiComponents.ColorPickerWithPalette(10,
                TransId("Settings_FWTab_TextColor"),
                _configuration.FloatingWindowPrePullColor);
            if (prePullColor != _configuration.FloatingWindowPrePullColor)
            {
                _configuration.FloatingWindowPrePullColor = prePullColor;
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
        var fwScale = _configuration.FloatingWindowScale;
        ImGui.PushItemWidth(100f);
        if (ImGui.DragFloat(TransId("Settings_CountdownTab_FloatingWindowScale"), ref fwScale, .01f))
        {
            _configuration.FloatingWindowScale = Math.Clamp(fwScale, 0.05f, 15f);
            _configuration.Save();
        }

        var textAlign = (int)_configuration.StopwatchTextAlign;
        if (ImGui.Combo(TransId("Settings_FWTab_TextAlign"), ref textAlign,
                Trans("Settings_FWTab_TextAlign_Left") + "###Left\0" +
                Trans("Settings_FWTab_TextAlign_Center") + "###Center\0" +
                Trans("Settings_FWTab_TextAlign_Right") + "###Right"))
        {
            _configuration.StopwatchTextAlign = (Configuration.TextAlign)textAlign;
            _configuration.Save();
        }

        var fontSize = _configuration.FontSize;
        if (ImGui.InputInt(TransId("Settings_FWTab_FontSize"), ref fontSize, 4))
        {
            _configuration.FontSize = Math.Max(0, fontSize);
            _configuration.Save();

            if (_configuration.FontSize >= 8) _uiBuilder.RebuildFonts();
        }

        ImGui.EndGroup();
        ImGui.SameLine();
        ImGui.BeginGroup();
        var floatingWindowTextColor = ImGuiComponents.ColorPickerWithPalette(1,
            TransId("Settings_FWTab_TextColor"),
            _configuration.FloatingWindowTextColor);
        if (floatingWindowTextColor != _configuration.FloatingWindowTextColor)
        {
            _configuration.FloatingWindowTextColor = floatingWindowTextColor;
            _configuration.Save();
        }

        ImGui.SameLine();
        ImGui.Text(Trans("Settings_FWTab_TextColor"));

        var floatingWindowBackgroundColor = ImGuiComponents.ColorPickerWithPalette(2,
            TransId("Settings_FWTab_BackgroundColor"),
            _configuration.FloatingWindowBackgroundColor);
        if (floatingWindowBackgroundColor != _configuration.FloatingWindowBackgroundColor)
        {
            _configuration.FloatingWindowBackgroundColor = floatingWindowBackgroundColor;
            _configuration.Save();
        }

        ImGui.SameLine();
        ImGui.Text(Trans("Settings_FWTab_BackgroundColor"));
        ImGui.EndGroup();

        ImGui.Unindent();
    }

    private void WebServerTabContent()
    {
        var enableWebServer = _configuration.EnableWebServer;

        ImGui.PushTextWrapPos();
        ImGui.Text(Trans("Settings_Web_Help"));
        ImGui.Text(Trans("Settings_Web_HelpAdd"));

        ImGui.Text($"http://localhost:{_configuration.WebServerPort}/");
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Copy))
            ImGui.SetClipboardText($"http://localhost:{_configuration.WebServerPort}/");

        ImGui.Text(Trans("Settings_Web_HelpSize"));
        ImGui.PopTextWrapPos();
        ImGui.Separator();

        if (ImGui.Checkbox(TransId("Settings_Web_EnablePort"), ref enableWebServer))
        {
            _configuration.EnableWebServer = enableWebServer;
            _configuration.Save();
        }

        ImGui.SameLine();
        var webServerPort = _configuration.WebServerPort;
        if (ImGui.InputInt("###EngageTimer_WebPort", ref webServerPort))
        {
            _configuration.WebServerPort = webServerPort;
            _configuration.Save();
        }

        var enableWebStopwatchTimeout = _configuration.EnableWebStopwatchTimeout;
        if (ImGui.Checkbox(TransId("Settings_Web_Hide_Left"), ref enableWebStopwatchTimeout))
        {
            _configuration.EnableWebStopwatchTimeout = enableWebStopwatchTimeout;
            _configuration.Save();
        }

        var webStopwatchTimeout = _configuration.WebStopwatchTimeout;
        ImGui.SameLine();
        if (ImGui.DragFloat(TransId("Settings_Web_Hide_Right"), ref webStopwatchTimeout))
        {
            _configuration.WebStopwatchTimeout = webStopwatchTimeout;
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

        var choices = Configuration.BundledTextures;
        var choiceString = "";
        var currentTexture = choices.Count();
        for (var i = 0; i < choices.Count(); i++)
        {
            choiceString += _tr.TransId("Settings_CountdownTab_Texture_" + choices[i], choices[i]) + "\0";
            if (_configuration.CountdownTexturePreset == choices[i]) currentTexture = i;
        }

        ImGui.BeginGroup();
        ImGui.PushItemWidth(200f);
        choiceString += TransId("Settings_CountdownTab_Texture_custom");
        if (ImGui.Combo("###DropDown_" + Trans("Settings_CountdownTab_Texture"), ref currentTexture, choiceString))
        {
            _configuration.CountdownTexturePreset = currentTexture < choices.Count() ? choices[currentTexture] : "";
            _configuration.Save();
            _numberTextures.Load();
        }

        ImGui.PopItemWidth();

        ImGui.SameLine();
        var monospaced = _configuration.CountdownMonospaced;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_Monospaced"), ref monospaced))
        {
            _configuration.CountdownMonospaced = monospaced;
            _configuration.Save();
        }

        if (_configuration.CountdownTexturePreset == "")
        {
            _tempTexturePath ??= _configuration.CountdownTextureDirectory ?? "";

            ImGui.PushItemWidth(400f);
            ImGui.InputText(TransId("Settings_CountdownTab_Texture_Custom_Path"), ref _tempTexturePath, 1024);
            ImGui.PopItemWidth();
            if (ImGui.Button(TransId("Settings_CountdownTab_Texture_Custom_Load")))
            {
                _configuration.CountdownTextureDirectory = _tempTexturePath;
                _configuration.Save();
                _numberTextures.Load();
            }
        }

        if (ImGui.CollapsingHeader(TransId("Settings_CountdownTab_NumberStyleTitle"))) CountdownNumberColor();

        if (ImGui.CollapsingHeader(TransId("Settings_CountdownTab_NumberStyle_Advanced")))
        {
            var leading0 = _configuration.CountdownLeadingZero;
            if (ImGui.Checkbox(TransId("Settings_CountdownTab_NumberStyle_LeadingZero"), ref leading0))
            {
                _configuration.CountdownLeadingZero = leading0;
                _configuration.Save();
            }

            var enableCustomNegativeMargin = _configuration.CountdownCustomNegativeMargin != null;
            if (ImGui.Checkbox(TransId("Settings_CountdownTab_NumberStyle_EnableCustomNegativeMargin"),
                    ref enableCustomNegativeMargin))
            {
                _configuration.CountdownCustomNegativeMargin = enableCustomNegativeMargin ? 20f : null;
                _configuration.Save();
            }

            if (enableCustomNegativeMargin)
            {
                ImGui.Indent();
                ImGui.PushItemWidth(100f);
                var nm = _configuration.CountdownCustomNegativeMargin ?? 20f;
                if (ImGui.InputFloat(TransId("Settings_CountdownTab_NumberStyle_CustomNegativeMargin"), ref nm, 1f))
                {
                    _configuration.CountdownCustomNegativeMargin = nm;
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
            _configuration.CountdownNumberLuminance = 0f;
            _numberTextures.CreateTextures();
            _configuration.Save();
        }

        ImGui.SameLine();
        var b = _configuration.CountdownNumberLuminance;
        if (ImGui.SliderFloat("± " + TransId("Settings_CountdownTab_NumberLuminance"), ref b, -1f, 1f))
        {
            _configuration.CountdownNumberLuminance = Math.Clamp(b, -1f, 1f);
            _numberTextures.CreateTextures();
            _configuration.Save();
        }

        // --- Saturation ---
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Undo.ToIconString() + "###reset_sat"))
        {
            _configuration.CountdownNumberSaturation = 0f;
            _numberTextures.CreateTextures();
            _configuration.Save();
        }

        ImGui.SameLine();
        var s = _configuration.CountdownNumberSaturation;
        if (ImGui.SliderFloat("± " + TransId("Settings_CountdownTab_NumberSaturation"), ref s, -1f, 1f))
        {
            _configuration.CountdownNumberSaturation = Math.Clamp(s, -1f, 1f);
            _numberTextures.CreateTextures();
            _configuration.Save();
        }

        // --- Hue ---
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Undo.ToIconString() + "###reset_hue"))
        {
            _configuration.CountdownNumberHue = 0;
            _numberTextures.CreateTextures();
            _configuration.Save();
        }

        var h = _configuration.CountdownNumberHue;
        ImGui.SameLine();
        if (_configuration.CountdownNumberRecolorMode)
        {
            ImGui.PushStyleColor(ImGuiCol.FrameBg, HslConv.HslToVector4Rgb(h, 0.3f, 0.3f));
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, HslConv.HslToVector4Rgb(h, 0.5f, 0.3f));
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, HslConv.HslToVector4Rgb(h, 0.7f, 0.3f));
        }

        if (ImGui.DragInt((_configuration.CountdownNumberRecolorMode ? "" : "± ") +
                          TransId("Settings_CountdownTab_NumberHue"), ref h, 1))
        {
            if (h > 360) h = 0;
            if (h < 0) h = 360;
            _configuration.CountdownNumberHue = h;
            _numberTextures.CreateTextures();
            _configuration.Save();
        }

        if (_configuration.CountdownNumberRecolorMode) ImGui.PopStyleColor(3);

        ImGui.PopItemWidth();

        var tint = _configuration.CountdownNumberRecolorMode;
        if (ImGui.Checkbox(TransId("Settings_CountdownTab_NumberRecolor"), ref tint))
        {
            _configuration.CountdownNumberRecolorMode = !_configuration.CountdownNumberRecolorMode;
            _configuration.Save();
            _numberTextures.CreateTextures();
        }
    }
}