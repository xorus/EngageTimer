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

using EngageTimer.Configuration;
using EngageTimer.Localization;
using EngageTimer.Properties;
using ImGuiNET;

namespace EngageTimer.Ui.SettingsTab;

public static class DtrTab
{
    public static void Draw()
    {
        ImGui.PushTextWrapPos();
        ImGui.Text(Strings.Settings_DtrTab_Info);
        ImGui.PopTextWrapPos();
        ImGui.Separator();

        var enabled = Plugin.Config.Dtr.CombatTimeEnabled;
        if (ImGui.Checkbox(Translator.TrId("Settings_DtrCombatTimer_Enable"), ref enabled))
        {
            Plugin.Config.Dtr.CombatTimeEnabled = enabled;
            Plugin.Config.Save();
        }

        Components.AutoField(Plugin.Config.Dtr, "CombatTimePrefix");
        ImGui.SameLine();
        Components.AutoField(Plugin.Config.Dtr, "CombatTimeSuffix");
        ImGui.SameLine();

        if (ImGui.Button(Translator.TrId("Settings_DtrCombatTimer_Defaults")))
        {
            Plugin.Config.Dtr.CombatTimePrefix = DtrConfiguration.DefaultCombatTimePrefix;
            Plugin.Config.Dtr.CombatTimeSuffix = DtrConfiguration.DefaultCombatTimeSuffix;
            Plugin.Config.Save();
        }

        Components.AutoField(Plugin.Config.Dtr, "CombatTimeAlwaysDisableOutsideDuty");
        Components.AutoField(Plugin.Config.Dtr, "CombatTimeDecimalPrecision");
        Components.AutoField(Plugin.Config.Dtr, "CombatTimeEnableHideAfter");
        ImGui.SameLine();
        Components.AutoField(Plugin.Config.Dtr, "CombatTimeHideAfter");
    }
}