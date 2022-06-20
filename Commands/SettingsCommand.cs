﻿using System;
using Dalamud.Game.Command;

namespace EngageTimer.Commands;

public class SettingsCommand : IDisposable
{
    private readonly Container _container;

    public SettingsCommand(Container container)
    {
        _container = container;
        _container.Resolve<CommandManager>().AddHandler("/egsettings", new CommandInfo(OpenSettingsCommand)
        {
            HelpMessage = container.Resolve<Translator>().Trans("MainCommand_Help_Settings")
        });
    }

    private void OpenSettingsCommand(string command, string args) => _container.Resolve<PluginUi>().OpenSettings();

    public void Dispose()
    {
        _container.Resolve<CommandManager>().RemoveHandler("/egsettings");
    }
}