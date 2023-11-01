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

namespace EngageTimer.Web;

internal class WebServer : IDisposable
{
    private readonly string _staticDirectory = Path.Combine(Plugin.PluginPath, "Data", "html");

    private bool _enableWebServer;

    private EmbedIO.WebServer _server;
    private CancellationTokenSource _serverCancellationToken;

    private Websocket _websocket;

    public void Dispose()
    {
        _serverCancellationToken?.Cancel();
        _server?.Dispose();
        _websocket?.Dispose();
        _serverCancellationToken?.Dispose();
    }

    private void Enable()
    {
        Plugin.Logger.Info($"WebServer enabled - serving files from {_staticDirectory}");
        var configuration = Plugin.Config;
        _websocket = new Websocket("/ws", Plugin.State, configuration);
        _server = new EmbedIO.WebServer(o => o
                    .WithUrlPrefix($"http://+:{configuration.WebServer.WebServer}/")
                    .WithMode(HttpListenerMode.EmbedIO)
                )
                .WithModule(_websocket)
                .WithStaticFolder("/", _staticDirectory, false)
            ;
        _server.StateChanged += (s, e) => { Plugin.Logger.Info($"WebServer is {e.NewState}"); };
        _serverCancellationToken = new CancellationTokenSource();
        _server.RunAsync(_serverCancellationToken.Token);
    }

    private void Disable()
    {
        Plugin.Logger.Info("Disabling WebServer");
        if (_serverCancellationToken != null && !_serverCancellationToken.IsCancellationRequested)
            _serverCancellationToken?.Cancel();

        _server?.Dispose();
        _websocket?.Dispose();
    }

    public void Update()
    {
        // Check if the webserver enable setting has been toggled
        var configuration = Plugin.Config;
        if (_enableWebServer != configuration.WebServer.Enable)
        {
            _enableWebServer = configuration.WebServer.Enable;
            if (_enableWebServer)
                Enable();
            else
                Disable();
        }

        if (_enableWebServer)
            _websocket?.UpdateInfo();
    }
}