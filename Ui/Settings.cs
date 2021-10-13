using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using EngageTimer.Properties;
using ImGuiNET;

namespace EngageTimer.UI
{
    public class Settings
    {
        private readonly Configuration _configuration;
        private readonly UiBuilder _uiBuilder;
        private readonly NumberTextures _numberTextures;
        private readonly State _state;

        private bool _visible;


        public Settings(Configuration configuration, State state, UiBuilder uiBuilder, NumberTextures _numberTextures)
        {
            _configuration = configuration;
            _uiBuilder = uiBuilder;
            this._numberTextures = _numberTextures;
            _state = state;
        }

        public bool Visible
        {
            get => _visible;
            set => _visible = value;
        }

        private string TransId(string id)
        {
            return $"{Resources.ResourceManager.GetString(id, Resources.Culture) ?? id}###EngageTimer_{id}";
        }

        private string Trans(string id)
        {
            return Resources.ResourceManager.GetString(id, Resources.Culture);
        }

        public void Draw()
        {
            if (!Visible) return;
            // _state.Mocked = true;
            // _state.InCombat = false;
            // _state.CountDownValue = 12.23f;
            // _state.CountingDown = true;

            if (ImGui.Begin(Resources.Settings_Title, ref _visible, ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (ImGui.BeginTabBar("EngageTimerSettingsTabBar", ImGuiTabBarFlags.None))
                {
                    var countdownAccurateCountdown = _configuration.CountdownAccurateCountdown;
                    var floatingWindowAccurateCountdown = _configuration.FloatingWindowAccurateCountdown;

                    ImGui.PushItemWidth(100f);
                    if (ImGui.BeginTabItem(TransId("Settings_CountdownTab_Title")))
                    {
                        ImGui.PushTextWrapPos();
                        ImGui.Text(Trans("Settings_CountdownTab_Info1"));
                        ImGui.Text(Trans("Settings_CountdownTab_Info2"));
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

                        var enableTickingSound = _configuration.EnableTickingSound;
                        if (ImGui.Checkbox(TransId("Settings_CountdownTab_PlaySound"), ref enableTickingSound))
                        {
                            _configuration.EnableTickingSound = enableTickingSound;
                            _configuration.Save();
                        }

                        if (enableTickingSound)
                        {
                            ImGui.Indent();
                            var volume = _configuration.TickingSoundVolume * 100f;
                            if (ImGui.DragFloat(TransId("Settings_CountdownTab_PlaySound_Volume"), ref volume, .1f, 0f,
                                100f, "%.1f%%"))
                            {
                                _configuration.TickingSoundVolume = Math.Max(0f, Math.Min(1f, volume / 100f));
                                _configuration.Save();
                            }

                            ImGui.Unindent();
                        }

                        CountdownTextures();

                        ImGui.Separator();

                        var countdownAccurateCountdownDisabled = !_configuration.HideOriginalCountdown;
                        if (countdownAccurateCountdownDisabled)
                        {
                            countdownAccurateCountdownDisabled = true;
                            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                        }

                        if (ImGui.Checkbox(TransId("Settings_CountdownTab_AccurateMode"),
                            ref countdownAccurateCountdown))
                        {
                            _configuration.CountdownAccurateCountdown = countdownAccurateCountdown;
                            _configuration.Save();
                        }

                        if (countdownAccurateCountdownDisabled)
                        {
                            ImGui.PopStyleVar();
                        }

                        ImGui.Indent();
                        ImGui.TextDisabled(Trans("Settings_CountdownTab_AccurateMode_Help"));
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem(TransId("Settings_FWTab_Title")))
                    {
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

                        ImGui.Text(Trans("Settings_FWTab_Styling"));
                        ImGui.Indent();

                        var textAlign = (int)_configuration.StopwatchTextAlign;
                        if (ImGui.Combo(TransId("Settings_FWTab_TextAlign"), ref textAlign,
                            Trans("Settings_FWTab_TextAlign_Left") + "###Left\0" +
                            Trans("Settings_FWTab_TextAlign_Center") + "###Center\0" +
                            Trans("Settings_FWTab_TextAlign_Right") + "###Right"))
                        {
                            _configuration.StopwatchTextAlign = (Configuration.TextAlign)textAlign;
                        }

                        var fontSize = _configuration.FontSize;
                        if (ImGui.InputInt(TransId("Settings_FWTab_FontSize"), ref fontSize, 4))
                        {
                            _configuration.FontSize = Math.Max(0, fontSize);
                            _configuration.Save();

                            if (_configuration.FontSize >= 8)
                            {
                                _uiBuilder.RebuildFonts();
                            }
                        }

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
                        ImGui.Unindent();
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

                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem(TransId("Settings_Web_Title")))
                    {
                        var enableWebServer = _configuration.EnableWebServer;

                        ImGui.PushTextWrapPos();
                        ImGui.Text(Trans("Settings_Web_Help"));
                        ImGui.Text(Trans("Settings_Web_HelpAdd"));

                        ImGui.Text($"http://localhost:{_configuration.WebServerPort}/");
                        ImGui.SameLine();
                        if (ImGuiComponents.IconButton(FontAwesomeIcon.Copy))
                        {
                            ImGui.SetClipboardText($"http://localhost:{_configuration.WebServerPort}/");
                        }

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

                    ImGui.PopItemWidth();
                    ImGui.EndTabBar();
                }

                ImGui.NewLine();
                ImGui.Separator();
                if (ImGui.Button(TransId("Settings_Close"))) Visible = false;
            }

            ImGui.End();
        }

        private string _tempTexturePath = null;
        private int _exampleNumber = 9;

        private void CountdownTextures()
        {
            ImGui.Separator();

            ImGui.Text(Trans("Settings_CountdownTab_Texture"));

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
                choiceString += (TransId("Settings_CountdownTab_Texture_" + choices[i])) + "\0";
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

            if (_configuration.CountdownTexturePreset == "")
            {
                if (_tempTexturePath == null) _tempTexturePath = _configuration.CountdownTextureDirectory ?? "";

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

            ImGui.EndGroup();
            ImGui.EndGroup();
        }
    }
}