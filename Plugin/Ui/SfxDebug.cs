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
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace EngageTimer.Ui;

public class SfxDebug : Window
{
    public SfxDebug() : base("Sound effect finder", ImGuiWindowFlags.AlwaysAutoResize)
    {
        IsOpen = true;
    }

    private int _id = 0;

    public override void Draw()
    {
        if (ImGui.InputInt("ID", ref _id))
        {
            Plugin.SfxPlay.SoundEffect((uint)_id);
        }

        ImGui.SameLine();
        Components.IconButton(FontAwesomeIcon.Play, "Play", () => Plugin.SfxPlay.SoundEffect((uint)_id));
    }
}