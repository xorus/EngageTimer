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
using Dalamud.Plugin;
using EngageTimer.Commands;
using EngageTimer.Configuration;
using EngageTimer.Status;
using EngageTimer.Ui;
using JetBrains.Annotations;
using XwContainer;

namespace EngageTimer;

[PublicAPI]
public sealed class Plugin : IDalamudPlugin
{
    private readonly ConfigurationFile _configuration;
    private readonly DalamudPluginInterface _pluginInterface;

    public Plugin(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Bag>();
        Bag.Init();
        
        PluginPath = pluginInterface.AssemblyLocation.DirectoryName;
        Container = new Container();
        Container.Register(this);
        _pluginInterface = Container.Register(pluginInterface);
        Container.Register(pluginInterface.UiBuilder);
        // new Localization(_pluginInterface.GetPluginLocDirectory());

        _configuration = Container.Register(Bag.Config);

        // _configuration =
        // Container.Register((ConfigurationFile)_pluginInterface.GetPluginConfig() ?? new ConfigurationFile());

        Container.Register(new State());
        Container.RegisterDisposable<Translator>();
        Container.RegisterDisposable<PluginUi>();
        Container.RegisterDisposable<FrameworkThings>();
        Container.RegisterDisposable<MainCommand>();
        Container.RegisterDisposable<SettingsCommand>();
    }

    public string PluginPath { get; }
    private Container Container { get; }

    void IDisposable.Dispose()
    {
        _pluginInterface.SavePluginConfig(_configuration);
        Container.Dispose();
    }
}