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

using Dalamud.Interface.Windowing;
using EngageTimer.Localization;
using EngageTimer.Properties;
using EngageTimer.Ui.SettingsTab;
using Dalamud.Bindings.ImGui;

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
        WindowName = Strings.Settings_Title;
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

            if (ImGui.BeginTabItem(Translator.TrId("Settings_AboutTab_Title")))
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