// This file is part of EngageTimer
// Copyright (C) 2024 Xorus <xorus@posteo.net>
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
using System.Numerics;
using ImGuiNET;

namespace EngageTimer.Ui;

public class Modal
{
    private bool _visible = false;
    private const string Title = "EngageTimer####egmodal";
    private string _message = "";
    private Action? _validate = null;

    public void Draw()
    {
        if (_visible)
        {
            var center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new Vector2(300, 0));
        }

        if (!ImGui.BeginPopupModal(Title, ref _visible)) return;
        ImGui.TextWrapped(_message);

        if (_validate != null)
        {
            if (ImGui.Button(Translator.Tr("Modal_Cancel"), new Vector2(120, 0)))
                ImGui.CloseCurrentPopup();
            ImGui.SameLine();
            if (ImGui.Button(Translator.Tr("Modal_Confirm"), new Vector2(120, 0)))
            {
                ImGui.CloseCurrentPopup();
                _validate();
            }
        }
        else if (ImGui.Button(Translator.Tr("Modal_Ok"), new Vector2(120, 0)))
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }

    public void Show(string message)
    {
        _message = message;
        _visible = true;
        _validate = null;
        ImGui.OpenPopup(Title);
    }

    public void Confirm(string message, Action validate)
    {
        _message = message;
        _validate = validate;
        _visible = true;
        ImGui.OpenPopup(Title);
    }
}