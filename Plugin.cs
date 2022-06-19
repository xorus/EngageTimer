using System;
using System.Globalization;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Plugin;
using EngageTimer.Attributes;
using EngageTimer.Properties;
using EngageTimer.Web;

namespace EngageTimer;

public sealed class Plugin : IDalamudPlugin
{
    private readonly CombatStopwatch _combatStopwatch;
    private readonly Configuration _configuration;
    private readonly CountdownHook _countdownHook;
    private readonly Framework _framework;
    private readonly PluginCommandManager<Plugin> _pluginCommandManager;
    private readonly DalamudPluginInterface _pluginInterface;
    private readonly WebServer _server;
    private readonly PluginUi _ui;

    public Plugin(
        DalamudPluginInterface pluginInterface,
        GameGui gameGui,
        CommandManager commands,
        Condition condition,
        DtrBar dtrBar,
        PartyList partyList,
        Framework framework
    )
    {
        _pluginInterface = pluginInterface;
        _framework = framework;

        var localPath = pluginInterface.AssemblyLocation.DirectoryName;
        // new Localization(_pluginInterface.GetPluginLocDirectory());

        _configuration = (Configuration)_pluginInterface.GetPluginConfig() ?? new Configuration();
        _configuration.Initialize(_pluginInterface);
        _configuration.Migrate();

        var state = new State();
        _ui = new PluginUi(_pluginInterface, _configuration, gameGui, localPath, state, dtrBar);

        _pluginCommandManager = new PluginCommandManager<Plugin>(this, commands);
        _countdownHook = new CountdownHook(state, condition);

        _pluginInterface.UiBuilder.Draw += DrawUi;
        _pluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;
        _framework.Update += FrameworkOnUpdate;
        _combatStopwatch = new CombatStopwatch(state, condition, partyList, _configuration);

        _server = new WebServer(_configuration, localPath, state);
        _server = new WebServer(_configuration, localPath, state);
        _pluginInterface.LanguageChanged += ConfigureLanguage;
        ConfigureLanguage();
    }

    public string Name => "Engage Timer";

    void IDisposable.Dispose()
    {
        _server?.Dispose();
        _pluginCommandManager?.Dispose();
        _pluginInterface.SavePluginConfig(_configuration);
        _pluginInterface.UiBuilder.Draw -= _ui.Draw;
        _ui?.Dispose();
        _framework.Update -= FrameworkOnUpdate;
        _countdownHook?.Dispose();

        _pluginInterface.LanguageChanged -= ConfigureLanguage;
    }

    private void FrameworkOnUpdate(Framework framework)
    {
        _combatStopwatch.UpdateEncounterTimer();
        _countdownHook?.Update();
    }

    private void ConfigureLanguage(string langCode = null)
    {
        var lang = (langCode ?? _pluginInterface.UiLanguage) switch
        {
            "fr" => "fr",
            "de" => "de",
            // "ja" => "ja",
            _ => "en"
        };
        Resources.Culture = new CultureInfo(lang ?? "en");
    }

    private void DrawUi()
    {
        // disable plugin operation when not logged in
        // if (!_clientState.IsLoggedIn)
        // return;
        _server?.Update();
        _ui?.Draw();
    }

    private void OpenConfigUi()
    {
        _ui.OpenSettings();
    }

    [Command("/egsettings")]
    [HelpMessage("Opens up the settings")]
    public void OpenSettingsCommand(string command, string args)
    {
        _ui.OpenSettings();
    }
}