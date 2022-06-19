using System;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Logging;
using EngageTimer.Attributes;

namespace EngageTimer.Commands;

public class MainCommand : IDisposable
{
    private readonly Container _container;
    private const string Command = "/eg";
    private const string Tab = "   ";

    public MainCommand(Container container)
    {
        _container = container;
        _container.Resolve<CommandManager>().AddHandler(Command, new CommandInfo(OnCommand)
        {
            HelpMessage = "\n" +
                          Tab + Command + " c|countdown [on|off] → " +
                          "enable or disable the big countdown (toggles by default)\n" +
                          Tab + Command + " fw [on|off] → " +
                          "enable or disable the floating window (toggles by default)\n" +
                          Tab + Command + " dtr [on|off] → " +
                          "enable or disable the server info bar (toggles by default)\n" +
                          Tab + Command + " s|settings → " +
                          "open the settings menu\n"
        });
    }

    private bool ToStatus(string input, bool current)
    {
        return input switch
        {
            "1" or "on" or "enable" or "show" => true,
            "" or "toggle" => !current,
            "0" or "off" or "disable" or "hide" => false,
            _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
        };
    }

    private string BoolStatus(bool value)
    {
        return value ? "enabled" : "disabled";
    }

    private void OnCommand(string command, string args)
    {
        var config = _container.Resolve<Configuration>();
        var chat = _container.Resolve<ChatGui>();

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
                    config.DisplayCountdown = ToStatus(argument, config.DisplayCountdown);
                    config.Save();
                    chat.Print("Countdown is now " + BoolStatus(config.DisplayCountdown));
                    break;
                case "sw":
                case "fw":
                    config.DisplayFloatingWindow = ToStatus(argument, config.DisplayFloatingWindow);
                    config.Save();
                    chat.Print("Floating window is now " + BoolStatus(config.DisplayFloatingWindow));
                    break;
                case "dtr":
                    config.DtrCombatTimeEnabled = ToStatus(argument, config.DtrCombatTimeEnabled);
                    config.Save();
                    chat.Print("server info bar is now " + BoolStatus(config.DtrCombatTimeEnabled));
                    break;
                default:
                    chat.PrintError("unrecognized subcommand: " + subcommand);
                    break;
            }
        }
        catch (ArgumentOutOfRangeException e)
        {
            chat.PrintError("unrecognized argument for " + subcommand + ": " + argument);
        }
    }

    public void Dispose()
    {
        _container.Resolve<CommandManager>().RemoveHandler(Command);
    }
}