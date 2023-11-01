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
using Dalamud.Plugin.Services;
using EngageTimer.Game;
using EngageTimer.Ui;
using EngageTimer.Web;

namespace EngageTimer.Status;

public sealed class FrameworkThings : IDisposable
{
    private readonly CombatStopwatch _combatStopwatch = new();
    private readonly CountdownHook _countdownHook = new();
    private readonly DtrBarUi _dtrBarUi = new();
    private readonly PrePullDetect _prePull = new();
    private readonly WebServer _server = new();
    private readonly TickingSound _sound = new();

    public FrameworkThings()
    {
        Plugin.Framework.Update += OnUpdate;
    }

    public void Dispose()
    {
        _countdownHook.Dispose();
        _server.Dispose();
        _dtrBarUi.Dispose();
        Plugin.Framework.Update -= OnUpdate;
    }

    private void OnUpdate(IFramework framework)
    {
        _server.Update();
        _combatStopwatch.UpdateEncounterTimer();
        _countdownHook.Update();
        _dtrBarUi.Update();
        _sound.Update();
        _prePull.Update();
    }
}