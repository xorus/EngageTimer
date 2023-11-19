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

using Dalamud.Interface;
using EngageTimer.Configuration.Legacy;
using ImGuiNET;

namespace EngageTimer.Ui.SettingsTab;

public static class AlarmsTab
{
    public static void Draw()
    {
        ImGui.Text("Some explanation text here");
        ImGui.Separator();

        ImGui.Text("Alarm list");
        if (Plugin.Config.CombatAlarms.Alarms.Count == 0)
        {
            ImGui.Text("No alarms set");
            return;
        }

        for (var index = 0; index < Plugin.Config.CombatAlarms.Alarms.Count; index++)
        {
            AlarmElement(index, Plugin.Config.CombatAlarms.Alarms[index]);
        }

        if (ImGui.Button("add"))
        {
        }
    }

    private static void AlarmElement(int index, CombatAlarmsConfiguration.Alarm alarm)
    {
        ImGui.PushID("alarm_" + index);
        ImGui.BeginGroup();
        Components.IconButton(FontAwesomeIcon.Trash, "delete_" + index,
            () => Plugin.Config.CombatAlarms.Alarms.RemoveAt(index));
        ImGui.SameLine();
        ImGui.Text("Alarm " + index);
        ImGui.EndGroup();

        ImGui.Indent();
        {
            var startTimeFormatted = "00:00";
            ImGui.InputText("start time", ref startTimeFormatted, 10);
        }
        ImGui.Unindent();
        ImGui.PopID();
    }
}