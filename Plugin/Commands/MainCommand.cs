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
using EngageTimer.Configuration;
using EngageTimer.Ui;
using XwContainer;

namespace EngageTimer.Commands;

public sealed class MainCommand : IDisposable
{
    private const string Command = "/eg";
    private const string Tab = "   ";
    private readonly Container _container;
    private readonly Translator _tr;

    public MainCommand(Container container)
    {
        _container = container;
        _tr = _container.Resolve<Translator>();
        Register();
        _tr.LocaleChanged += OnLocaleChanged;
    }

    public void Dispose()
    {
        _tr.LocaleChanged -= OnLocaleChanged;
        Unregister();
    }

    private void Register()
    {
        Bag.Commands.AddHandler(Command, new CommandInfo(OnCommand)
        {
            HelpMessage = "\n" +
                          Tab + Command + " c|countdown [on|off] → " +
                          $"{_tr.Trans("MainCommand_Help_Countdown")}\n" +
                          Tab + Command + " fw [on|off] → " +
                          $"{_tr.Trans("MainCommand_Help_FW")}\n" +
                          Tab + Command + " dtr [on|off] → " +
                          $"{_tr.Trans("MainCommand_Help_Dtr")}\n" +
                          Tab + Command + " s|settings → " +
                          $"{_tr.Trans("MainCommand_Help_Settings")}\n"
        });
    }

    private void Unregister()
    {
        Bag.Commands.RemoveHandler(Command);
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
        return _tr.Trans(value ? "MainCommand_Status_On" : "MainCommand_Status_Off");
    }

    private void OnCommand(string command, string args)
    {
        var config = _container.Resolve<ConfigurationFile>();
        var chat = Bag.ChatGui;

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
                    _container.Resolve<PluginUi>().OpenSettings();
                    break;
                case "c":
                case "countdown":
                    config.Countdown.Display = ToStatus(argument, config.Countdown.Display);
                    config.Save();
                    chat.Print(_tr.Trans("MainCommand_Help_Countdown_Success", StatusStr(config.Countdown.Display)));
                    break;
                case "sw":
                case "fw":
                    config.FloatingWindow.Display = ToStatus(argument, config.FloatingWindow.Display);
                    config.Save();
                    chat.Print(_tr.Trans("MainCommand_Help_FW_Success", StatusStr(config.FloatingWindow.Display)));
                    break;
                case "dtr":
                    config.Dtr.CombatTimeEnabled = ToStatus(argument, config.Dtr.CombatTimeEnabled);
                    config.Save();
                    chat.Print(_tr.Trans("MainCommand_Help_Dtr_Success", StatusStr(config.Dtr.CombatTimeEnabled)));
                    break;
                default:
                    chat.PrintError(_tr.Trans("MainCommand_Error_InvalidSubcommand", subcommand));
                    break;
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            chat.PrintError(_tr.Trans("MainCommand_Error_InvalidArgument", subcommand, argument));
        }
    }

    private void OnLocaleChanged(object sender, EventArgs e)
    {
        Unregister();
        Register();
    }
}