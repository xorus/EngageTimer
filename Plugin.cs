using System;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Plugin;
using EngageTimer.Commands;
using JetBrains.Annotations;

namespace EngageTimer;

[PublicAPI]
public sealed class Plugin : IDalamudPlugin
{
    private readonly Configuration _configuration;
    private readonly DalamudPluginInterface _pluginInterface;

    public string PluginPath { get; }

    private Container Container { get; }

    public string Name => "Engage Timer";

    public Plugin(
        DalamudPluginInterface pluginInterface,
        GameGui gameGui,
        CommandManager commands,
        Condition condition,
        DtrBar dtrBar,
        PartyList partyList,
        Framework framework,
        ChatGui chatGui
    )
    {
        PluginPath = pluginInterface.AssemblyLocation.DirectoryName;
        Container = new Container();
        Container.Register(this);
        _pluginInterface = Container.Register(pluginInterface);
        Container.Register(pluginInterface.UiBuilder);
        Container.Register(gameGui);
        Container.Register(commands);
        Container.Register(condition);
        Container.Register(dtrBar);
        Container.Register(partyList);
        Container.Register(framework);
        Container.Register(chatGui);
        // new Localization(_pluginInterface.GetPluginLocDirectory());

        _configuration = Container.Register((Configuration)_pluginInterface.GetPluginConfig() ?? new Configuration());
        _configuration.Initialize(_pluginInterface);
        _configuration.Migrate();

        Container.Register(new State());
        Container.RegisterDisposable<Translator>();
        Container.RegisterDisposable<PluginUi>();
        Container.RegisterDisposable<FwThings>();
        Container.RegisterDisposable<MainCommand>();
        Container.RegisterDisposable<SettingsCommand>();
    }

    void IDisposable.Dispose()
    {
        _pluginInterface.SavePluginConfig(_configuration);
        Container.DoDispose();
    }
}