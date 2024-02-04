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
using EngageTimer.Localization;
using EngageTimer.Ui.Color;
using EngageTimer.Ui.SettingsTab;
using ImGuiNET;
using JetBrains.Annotations;

namespace EngageTimer.Ui;

public class Settings : Window
{
    public Settings() : base("Settings", ImGuiWindowFlags.AlwaysAutoResize)
    {
        Translator.LocaleChanged += (_, _) => UpdateWindowName();
        UpdateWindowName();
#if DEBUG
        IsOpen = true;
#endif
    }

    public override void OnClose()
    {
        CountdownTab.OnClose();
    }

    private void UpdateWindowName()
    {
        WindowName = Translator.TrId("Settings_Title");
    }

    public override void Draw()
    {
        CountdownTab.DebounceTextureCreation();
        CountdownTab.UpdateMock();

        if (ImGui.BeginTabBar("EngageTimerSettingsTabBar", ImGuiTabBarFlags.None))
        {
            ImGui.PushItemWidth(100f);
            if (ImGui.BeginTabItem(Translator.TrId("Settings_CountdownTab_Title")))
            {
                CountdownTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(Translator.TrId("Settings_FWTab_Title")))
            {
                FloatingWindowTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(Translator.TrId("Settings_DtrTab_Title")))
            {
                DtrTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(Translator.TrId("Settings_Web_Title")))
            {
                WebServerTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(Translator.TrId("Settings_AlarmsTab_Title")))
            {
                AlarmsTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("About"))
            {
                AboutTab.Draw();
                ImGui.EndTabItem();
            }

            ImGui.PopItemWidth();
            ImGui.EndTabBar();
        }

        ImGui.NewLine();
        ImGui.Separator();
        if (ImGui.Button(Translator.TrId("Settings_Close"))) IsOpen = false;
    }
}