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
using Dalamud.Game.Command;
using EngageTimer.Properties;

namespace EngageTimer.Commands;

public sealed class SettingsCommand : IDisposable
{
    public SettingsCommand()
    {
        Plugin.Commands.AddHandler("/egsettings", new CommandInfo(OpenSettingsCommand)
        {
            HelpMessage = Strings.MainCommand_Help_Settings
        });
    }

    public void Dispose()
    {
        Plugin.Commands.RemoveHandler("/egsettings");
    }

    private static void OpenSettingsCommand(string command, string args)
    {
        Plugin.PluginUi.OpenSettings();
    }
}