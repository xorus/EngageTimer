using System;
using System.IO;
using System.Threading;
using Dalamud.Logging;
using EmbedIO;
using EngageTimer.Status;
using XwContainer;

namespace EngageTimer.Web;

internal class WebServer : IDisposable
{
    private readonly Configuration _configuration;
    private readonly State _state;
    private readonly string _staticDirectory;

    private bool _enableWebServer;

    private EmbedIO.WebServer _server;
    private CancellationTokenSource _serverCancellationToken;

    private Websocket _websocket;

    public WebServer(Container container)
    {
        _configuration = container.Resolve<Configuration>();
        _state = container.Resolve<State>();
        _staticDirectory = Path.Combine(container.Resolve<Plugin>().PluginPath, "Data", "html");
    }

    public void Dispose()
    {
        _serverCancellationToken?.Cancel();
        _server?.Dispose();
        _websocket?.Dispose();
        _serverCancellationToken?.Dispose();
    }

    public void Enable()
    {
        PluginLog.Log($"WebServer enabled - serving files from {_staticDirectory}");
        _websocket = new Websocket("/ws", _state, _configuration);
        _server = new EmbedIO.WebServer(o => o
                    .WithUrlPrefix($"http://+:{_configuration.WebServerPort}/")
                    .WithMode(HttpListenerMode.EmbedIO)
                )
                .WithModule(_websocket)
                .WithStaticFolder("/", _staticDirectory, false)
            ;
        _server.StateChanged += (s, e) => { PluginLog.Log($"WebServer is {e.NewState}"); };
        _serverCancellationToken = new CancellationTokenSource();
        _server.RunAsync(_serverCancellationToken.Token);
    }

    public void Disable()
    {
        PluginLog.Log("Disabling WebServer");
        if (_serverCancellationToken != null && !_serverCancellationToken.IsCancellationRequested)
            _serverCancellationToken?.Cancel();

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

        if (_enableWebServer)
            _websocket?.UpdateInfo();
    }
}