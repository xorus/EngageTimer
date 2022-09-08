using System;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
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
        _container.Resolve<CommandManager>().AddHandler(Command, new CommandInfo(OnCommand)
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
        _container.Resolve<CommandManager>().RemoveHandler(Command);
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
                    chat.Print(_tr.Trans("MainCommand_Help_Countdown_Success", StatusStr(config.DisplayCountdown)));
                    break;
                case "sw":
                case "fw":
                    config.DisplayFloatingWindow = ToStatus(argument, config.DisplayFloatingWindow);
                    config.Save();
                    chat.Print(_tr.Trans("MainCommand_Help_FW_Success", StatusStr(config.DisplayFloatingWindow)));
                    break;
                case "dtr":
                    config.DtrCombatTimeEnabled = ToStatus(argument, config.DtrCombatTimeEnabled);
                    config.Save();
                    chat.Print(_tr.Trans("MainCommand_Help_Dtr_Success", StatusStr(config.DtrCombatTimeEnabled)));
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