﻿// This file is part of EngageTimer
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
using Dalamud.Interface.Components;
using EngageTimer.Configuration;
using ImGuiNET;

namespace EngageTimer.Ui.SettingsTab;

public static class FloatingWindowTab
{
    public static void Draw()
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

    private static void FwStyling()
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

            if (configuration.FloatingWindow.FontSize >= 8) Plugin.PluginInterface.UiBuilder.RebuildFonts();
        }

        ImGui.EndGroup();
        ImGui.SameLine();
        ImGui.BeginGroup();
        Components.AutoField(Plugin.Config.FloatingWindow, "TextColor");
        Components.AutoField(Plugin.Config.FloatingWindow, "BackgroundColor");
        ImGui.EndGroup();

        ImGui.Unindent();
    }
}