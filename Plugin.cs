using System;
using System.IO;
using System.Reflection;
using Dalamud.Plugin;
using EngageTimer.Attributes;
using EngageTimer.Web;

/**
 * Based on the work (for finding the pointer) of https://github.com/Haplo064/Europe
 **/
namespace EngageTimer
{
    public class Plugin : IDalamudPlugin
    {
        private PluginCommandManager<Plugin> _commandManager;
        private Configuration _configuration;
        private DalamudPluginInterface _pluginInterface;
        private WebServer _server;
        private State _state;
        private StopWatchHook _stopWatchHook;
        private PluginUi _ui;

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string AssemblyLocation { get; set; }

        public string Name => "Engage Timer";

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            _pluginInterface = pluginInterface;
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

            _configuration = (Configuration) _pluginInterface.GetPluginConfig() ?? new Configuration();
            _configuration.Initialize(_pluginInterface);

            _state = new State();
            _ui = new PluginUi(_pluginInterface, _configuration, localPath, _state);

            _commandManager = new PluginCommandManager<Plugin>(this, _pluginInterface);

            _stopWatchHook = new StopWatchHook(_pluginInterface, _state);
            
            _pluginInterface.UiBuilder.OnBuildUi += DrawUi;
            _pluginInterface.UiBuilder.OnOpenConfigUi += OpenConfigUi;

            _server = new WebServer(_configuration, localPath, _state);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void DrawUi()
        {
            // disable plugin operation when not logged in
            if (_pluginInterface.ClientState.LocalPlayer == null)
                return;

            _stopWatchHook.Update();
            _server.Update();
            _ui.Draw();
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

            _server.Dispose();
            _commandManager.Dispose();
            _pluginInterface.SavePluginConfig(_configuration);
            _pluginInterface.UiBuilder.OnBuildUi -= _ui.Draw;
            _pluginInterface.Dispose();
            _stopWatchHook.Dispose();
        }
    }
}