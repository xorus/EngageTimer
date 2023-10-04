using System;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using EngageTimer.Commands;
using EngageTimer.Status;
using EngageTimer.Ui;
using JetBrains.Annotations;
using XwContainer;

namespace EngageTimer;

[PublicAPI]
public sealed class Plugin : IDalamudPlugin
{
    private readonly Configuration _configuration;
    private readonly DalamudPluginInterface _pluginInterface;

    public Plugin(
        DalamudPluginInterface pluginInterface,
        IGameGui gameGui,
        ICommandManager commands,
        ICondition condition,
        IDtrBar dtrBar,
        IPartyList partyList,
        IFramework framework,
        IChatGui chatGui,
        IGameInteropProvider gameInterop
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
        Container.Register(gameInterop);
        // new Localization(_pluginInterface.GetPluginLocDirectory());

        _configuration = Container.Register((Configuration)_pluginInterface.GetPluginConfig() ?? new Configuration());
        _configuration.Initialize(_pluginInterface);
        _configuration.Migrate();

        Container.Register(new State());
        Container.RegisterDisposable<Translator>();
        Container.RegisterDisposable<PluginUi>();
        Container.RegisterDisposable<FrameworkThings>();
        Container.RegisterDisposable<MainCommand>();
        Container.RegisterDisposable<SettingsCommand>();
    }

    public string PluginPath { get; }
    private Container Container { get; }
    public string Name => "Engage Timer";

    void IDisposable.Dispose()
    {
        _pluginInterface.SavePluginConfig(_configuration);
        Container.Dispose();
    }
}