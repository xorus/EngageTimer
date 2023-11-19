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

using System.Diagnostics;
using ImGuiNET;

namespace EngageTimer.Ui.SettingsTab;

public static class AboutTab
{
    public static void Draw()
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
    }
}