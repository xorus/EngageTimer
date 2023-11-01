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
using Dalamud.Interface.Windowing;
using XwContainer;

namespace EngageTimer.Ui;

public sealed class PluginUi : IDisposable
{
    private readonly Container _container;
    private readonly CountDown _countDown;
    private readonly FloatingWindow _floatingWindow;
    private readonly Settings _settings;
    private readonly WindowSystem _windowSystem;

    public PluginUi(Container container)
    {
        _container = container;
        var numbers = new NumberTextures(container);
        container.Register(numbers);

        _windowSystem = new WindowSystem("Engage Timer");
        _countDown = container.Register<CountDown>();
        _floatingWindow = container.RegisterDisposable<FloatingWindow>();
        _settings = container.Register<Settings>();

        _windowSystem.AddWindow(_settings);

        Bag.PluginInterface.UiBuilder.Draw += Draw;
        Bag.PluginInterface.UiBuilder.OpenConfigUi += OpenSettings;
    }

    public void Dispose()
    {
        Bag.PluginInterface.UiBuilder.Draw -= Draw;
        Bag.PluginInterface.UiBuilder.OpenConfigUi -= OpenSettings;
    }

    private void Draw()
    {
        _windowSystem.Draw();
        _countDown.Draw();
        _floatingWindow.Draw();
    }

    public void OpenSettings()
    {
        _settings.IsOpen = true;
    }
}