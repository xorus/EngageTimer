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
using Dalamud.Game.Command;
using EngageTimer.Ui;

namespace EngageTimer.Commands;

public sealed class MainCommand : IDisposable
{
    private const string Command = "/eg";
    private const string Tab = "   ";

    public MainCommand()
    {
        Register();
        Plugin.Translator.LocaleChanged += OnLocaleChanged;
    }

    public void Dispose()
    {
        Plugin.Translator.LocaleChanged -= OnLocaleChanged;
        Unregister();
    }

    private void Register()
    {
        Plugin.Commands.AddHandler(Command, new CommandInfo(OnCommand)
        {
            HelpMessage = "\n" +
                          Tab + Command + " c|countdown [on|off] → " +
                          $"{Translator.Tr("MainCommand_Help_Countdown")}\n" +
                          Tab + Command + " fw [on|off] → " +
                          $"{Translator.Tr("MainCommand_Help_FW")}\n" +
                          Tab + Command + " dtr [on|off] → " +
                          $"{Translator.Tr("MainCommand_Help_Dtr")}\n" +
                          Tab + Command + " s|settings → " +
                          $"{Translator.Tr("MainCommand_Help_Settings")}\n"
        });
    }

    private void Unregister()
    {
        Plugin.Commands.RemoveHandler(Command);
    }

    private static bool ToStatus(string input, bool current)
    {
        return input switch
        {
            "1" or "on" or "enable" or "show" => true,
            "" or "toggle" => !current,
            "0" or "off" or "disable" or "hide" => false,
            _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
        };
    }

    private string StatusStr(bool value)
    {
        return Translator.Tr(value ? "MainCommand_Status_On" : "MainCommand_Status_Off");
    }

    private void OnCommand(string command, string args)
    {
        var argsArray = args.Split(' ');
        var subcommand = "";
        if (argsArray.Length > 0) subcommand = argsArray[0];
        var argument = "";
        if (argsArray.Length > 1) argument = argsArray[1];

        try
        {
            switch (subcommand)
            {
                case "":
                case "s":
                case "settings":
                    Plugin.PluginUi.OpenSettings();
                    break;
                case "c":
                case "countdown":
                    Plugin.Config.Countdown.Display = ToStatus(argument, Plugin.Config.Countdown.Display);
                    Plugin.Config.Save();
                    Plugin.ChatGui.Print(Translator.Tr("MainCommand_Help_Countdown_Success",
                        StatusStr(Plugin.Config.Countdown.Display)));
                    break;
                case "sw":
                case "fw":
                    Plugin.Config.FloatingWindow.Display = ToStatus(argument, Plugin.Config.FloatingWindow.Display);
                    Plugin.Config.Save();
                    Plugin.ChatGui.Print(Translator.Tr("MainCommand_Help_FW_Success",
                        StatusStr(Plugin.Config.FloatingWindow.Display)));
                    break;
                case "dtr":
                    Plugin.Config.Dtr.CombatTimeEnabled = ToStatus(argument, Plugin.Config.Dtr.CombatTimeEnabled);
                    Plugin.Config.Save();
                    Plugin.ChatGui.Print(Translator.Tr("MainCommand_Help_Dtr_Success",
                        StatusStr(Plugin.Config.Dtr.CombatTimeEnabled)));
                    break;
                default:
                    Plugin.ChatGui.PrintError(Translator.Tr("MainCommand_Error_InvalidSubcommand", subcommand));
                    break;
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            Plugin.ChatGui.PrintError(Translator.Tr("MainCommand_Error_InvalidArgument", subcommand, argument));
        }
    }

    private void OnLocaleChanged(object? sender, EventArgs e)
    {
        Unregister();
        Register();
    }
}