using System;
using System.IO;
using System.Reflection;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Plugin;
using EngageTimer.Attributes;
using EngageTimer.Web;

/*
 * Based on the work (for finding the pointer) of https://github.com/Haplo064/Europe
 */
namespace EngageTimer
{
    public class Plugin : IDalamudPlugin
    {
        private readonly PluginCommandManager<Plugin> _pluginCommandManager;
        private readonly Configuration _configuration;
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly WebServer _server;
        private readonly StopWatchHook _stopWatchHook;
        private readonly PluginUi _ui;
        private readonly ClientState _clientState;

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string AssemblyLocation { get; set; }

        public string Name => "Engage Timer";

        public Plugin(
            DalamudPluginInterface pluginInterface,
            ClientState clientState,
            GameGui gameGui,
            CommandManager commands,
            SigScanner sigScanner,
            Condition condition)
        {
            _pluginInterface = pluginInterface;
            _clientState = clientState;

            string localPath;
            try
            {
                // For loading with Dalamud
                localPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            catch
            {
                // For loading with LPL
                localPath = Path.GetDirectoryName(AssemblyLocation);
            }

            _configuration = (Configuration)_pluginInterface.GetPluginConfig() ?? new Configuration();
            _configuration.Initialize(_pluginInterface);

            var state = new State();
            _ui = new PluginUi(_pluginInterface, _configuration, gameGui, localPath, state);

            _pluginCommandManager = new PluginCommandManager<Plugin>(this, _pluginInterface, commands);
            _stopWatchHook = new StopWatchHook(_pluginInterface, state, sigScanner, condition);

            _pluginInterface.UiBuilder.Draw += DrawUi;
            _pluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;

            _server = new WebServer(_configuration, localPath, state);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void DrawUi()
        {
            // disable plugin operation when not logged in
            if (!_clientState.IsLoggedIn)
                return;

            _stopWatchHook?.Update();
            _server?.Update();
            _ui?.Draw();
        }

        private void OpenConfigUi(object sender, EventArgs args)
        {
            _ui.OpenSettings();
        }

        [Command("/egsettings")]
        [HelpMessage("Opens up the settings")]
        public void OpenSettingsCommand(string command, string args)
        {
            _ui.OpenSettings();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            _server?.Dispose();
            _pluginCommandManager?.Dispose();
            _pluginInterface.SavePluginConfig(_configuration);
            _pluginInterface.UiBuilder.Draw -= _ui.Draw;
            _ui?.Dispose();
            _pluginInterface.Dispose();
            _stopWatchHook?.Dispose();
        }
    }
}