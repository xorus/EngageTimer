using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Game.Command;
using EngageTimer.Attributes;
using static Dalamud.Game.Command.CommandInfo;

// ReSharper disable ForCanBeConvertedToForeach

namespace EngageTimer;

public class PluginCommandManager<THost> : IDisposable
{
    private readonly CommandManager _commandManager;
    private readonly THost _host;
    private readonly (string, CommandInfo)[] _pluginCommands;

    public PluginCommandManager(THost host, CommandManager commandManager)
    {
        _commandManager = commandManager;
        _host = host;

        _pluginCommands = host.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public |
                                                    BindingFlags.Static | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<CommandAttribute>() != null)
            .SelectMany(GetCommandInfoTuple)
            .ToArray();

        AddCommandHandlers();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        RemoveCommandHandlers();
    }

    private void AddCommandHandlers()
    {
        for (var i = 0; i < _pluginCommands.Length; i++)
        {
            var (command, commandInfo) = _pluginCommands[i];
            _commandManager.AddHandler(command, commandInfo);
        }
    }

    private void RemoveCommandHandlers()
    {
        for (var i = 0; i < _pluginCommands.Length; i++)
        {
            var (command, _) = _pluginCommands[i];
            _commandManager.RemoveHandler(command);
        }
    }

    private IEnumerable<(string, CommandInfo)> GetCommandInfoTuple(MethodInfo method)
    {
        var handlerDelegate = (HandlerDelegate)Delegate.CreateDelegate(typeof(HandlerDelegate), _host, method);

        var command = handlerDelegate.Method.GetCustomAttribute<CommandAttribute>();
        var helpMessage = handlerDelegate.Method.GetCustomAttribute<HelpMessageAttribute>();

        var commandInfo = new CommandInfo(handlerDelegate)
        {
            HelpMessage = helpMessage?.HelpMessage ?? string.Empty,
            ShowInHelp = true
        };

        // Create list of tuples that will be filled with one tuple per alias, in addition to the base command tuple.
        var commandInfoTuples = new List<(string, CommandInfo)> { (command.Command, commandInfo) };
        return commandInfoTuples;
    }
}