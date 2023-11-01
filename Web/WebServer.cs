// This file is part of EngageTimer
// Copyright (C) 2023 Xorus <xorus@posteo.net>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Threading;
using EmbedIO;
using EngageTimer.Configuration;
using EngageTimer.Status;
using XwContainer;

namespace EngageTimer.Web;

internal class WebServer : IDisposable
{
    private readonly ConfigurationFile _configuration;
    private readonly State _state;
    private readonly string _staticDirectory;

    private bool _enableWebServer;

    private EmbedIO.WebServer _server;
    private CancellationTokenSource _serverCancellationToken;

    private Websocket _websocket;

    public WebServer(Container container)
    {
        _configuration = container.Resolve<ConfigurationFile>();
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
        Bag.Logger.Info($"WebServer enabled - serving files from {_staticDirectory}");
        _websocket = new Websocket("/ws", _state, _configuration);
        _server = new EmbedIO.WebServer(o => o
                    .WithUrlPrefix($"http://+:{_configuration.WebServer.WebServer}/")
                    .WithMode(HttpListenerMode.EmbedIO)
                )
                .WithModule(_websocket)
                .WithStaticFolder("/", _staticDirectory, false)
            ;
        _server.StateChanged += (s, e) => { Bag.Logger.Info($"WebServer is {e.NewState}"); };
        _serverCancellationToken = new CancellationTokenSource();
        _server.RunAsync(_serverCancellationToken.Token);
    }

    public void Disable()
    {
        Bag.Logger.Info("Disabling WebServer");
        if (_serverCancellationToken != null && !_serverCancellationToken.IsCancellationRequested)
            _serverCancellationToken?.Cancel();

        _server?.Dispose();
        _websocket?.Dispose();
    }

    public void Update()
    {
        // Check if the webserver enable setting has been toggled
        if (_enableWebServer != _configuration.WebServer.Enable)
        {
            _enableWebServer = _configuration.WebServer.Enable;
            if (_enableWebServer)
                Enable();
            else
                Disable();
        }

        if (_enableWebServer)
            _websocket?.UpdateInfo();
    }
}