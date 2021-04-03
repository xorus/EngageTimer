using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using Swan.Logging;

namespace EngageTimer
{
    class WebServer : IDisposable
    {
        public bool InCombat { get; set; } = false;
        public TimeSpan CombatDuration { get; set; } = new();

        public bool CountingDown { get; set; } = false;
        public float CountDown { get; set; } = 0f;

        private bool _enableWebServer;

        private readonly Configuration _configuration;
        private readonly PluginUI _pluginUi;
        private readonly string _staticDirectory;

        private EmbedIO.WebServer _server;
        private Websocket _websocket;

        public WebServer(Configuration configuration, string dir, PluginUI pluginUi)
        {
            this._configuration = configuration;
            _pluginUi = pluginUi;
            this._staticDirectory = Path.Combine(dir, "Data", "html");
        }

        private Task _serverTask;
        private Thread _thread;
        private CancellationTokenSource _serverCancelationToken;

        public void Enable()
        {
            PluginLog.Log($"WebServer enabled - serving files from ${_staticDirectory}");
            _websocket = new Websocket("/ws", _pluginUi);
            _server = new EmbedIO.WebServer(o => o
                        .WithUrlPrefix($"http://+:{_configuration.WebServerPort}/")
                        .WithMode(HttpListenerMode.EmbedIO)
                    )
                    .WithModule(_websocket)
                    .WithStaticFolder("/", _staticDirectory, false)
                ;
            _server.StateChanged += (s, e) => { PluginLog.Log($"WebServer New State - {e.NewState}"); };
            _serverCancelationToken = new CancellationTokenSource();
            _server.RunAsync(_serverCancelationToken.Token);
        }

        public void Disable()
        {
            PluginLog.Log("Disabling WebServer");
            if (_serverCancelationToken != null && !_serverCancelationToken.IsCancellationRequested)
            {
                _serverCancelationToken?.Cancel();
            }

            _server?.Dispose();
            _websocket?.Dispose();
        }

        public void Update()
        {
            // Check if the webserver enable setting has been toggled
            if (_enableWebServer != _configuration.EnableWebServer)
            {
                _enableWebServer = _configuration.EnableWebServer;
                if (_enableWebServer)
                    Enable();
                else
                    Disable();
            }

            if (_enableWebServer && _websocket != null)
                _websocket.UpdateInfo();
        }

        public void Dispose()
        {
            _serverCancelationToken?.Cancel();
            _server?.Dispose();
            _websocket?.Dispose();
            _serverTask?.Dispose();
            _serverCancelationToken?.Dispose();
        }
    }
}