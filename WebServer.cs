using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using Swan.Logging;

namespace EngageTimer
{
    class WebServer
    {
        public bool InCombat { get; set; } = false;
        public TimeSpan CombatDuration { get; set; } = new();

        public bool CountingDown { get; set; } = false;
        public float CountDown { get; set; } = 0f;

        private bool _enableWebServer = false;

        private readonly Configuration _configuration;
        private readonly PluginUI _pluginUi;

        private string _staticDirectory;

        private EmbedIO.WebServer _server;
        private Websocket _websocket;

        public WebServer(Configuration configuration, string dir, PluginUI pluginUi)
        {
            this._configuration = configuration;
            _pluginUi = pluginUi;
            this._staticDirectory = Path.Combine(dir, "Data", "html");
        }

        public void Enable()
        {
            _websocket = new Websocket("/ws", _pluginUi);
            _server = new EmbedIO.WebServer(o => o
                    .WithUrlPrefix($"http://{_configuration.WebServerHost}:{_configuration.WebServerPort}/")
                    .WithMode(HttpListenerMode.EmbedIO))
                .WithModule(_websocket)
                .WithStaticFolder("/", _staticDirectory, true);
            // server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();
        }

        public void Disable()
        {
            _server?.Dispose();
            _websocket?.Dispose();
            _server = null;
            _websocket = null;
        }

        public void Update()
        {
            if (_enableWebServer != _configuration.EnableWebServer)
            {
                _enableWebServer = _configuration.EnableWebServer;
                if (_enableWebServer)
                    Enable();
                else
                    Disable();
            }

            if (_websocket != null)
            {
                _websocket.UpdateInfo();
            }
        }
    }
}