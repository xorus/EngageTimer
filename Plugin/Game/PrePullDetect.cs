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

using EngageTimer.Configuration;
using EngageTimer.Status;
using FFXIVClientStructs.FFXIV.Client.Game;
using XwContainer;

namespace EngageTimer.Game;

/**
 * Thanks Aireil for this feature
 */
public class PrePullDetect
{
    private readonly ConfigurationFile _configuration;
    private readonly State _state;

    public PrePullDetect(Container container)
    {
        _configuration = container.Resolve<ConfigurationFile>();
        _state = container.Resolve<State>();
    }

    public unsafe void Update()
    {
        if (_state.PrePulling) _state.PrePulling = false;
        if (!_configuration.FloatingWindow.EnableCountdown || !_configuration.FloatingWindow.ShowPrePulling) return;
        var actionManager = (TrimmedDownActionManager*)ActionManager.Instance();
        if (!_state.CountingDown || !actionManager->isCasting) return;
        _state.PrePulling =
            actionManager->castTime - actionManager->elapsedCastTime + _configuration.FloatingWindow.PrePullOffset <
            _state.CountDownValue;
    }
}