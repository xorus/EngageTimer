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
using Dalamud.Interface.Components;
using ImGuiNET;

namespace EngageTimer.Ui.SettingsTab;

public static class WebServerTab
{
    public static void Draw()
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
}