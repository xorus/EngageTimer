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

using FFXIVClientStructs.FFXIV.Client.Game;

namespace EngageTimer.Game;

/**
 * Thanks Aireil for this feature
 */
public class PrePullDetect
{
    public unsafe void Update()
    {
        var state = Plugin.State;
        if (state.PrePulling) state.PrePulling = false;
        var configuration = Plugin.Config;
        if (!configuration.FloatingWindow.EnableCountdown || !configuration.FloatingWindow.ShowPrePulling) return;
        var actionManager = (TrimmedDownActionManager*)ActionManager.Instance();
        if (!state.CountingDown || !actionManager->isCasting) return;
        state.PrePulling =
            actionManager->castTime - actionManager->elapsedCastTime + configuration.FloatingWindow.PrePullOffset <
            state.CountDownValue;
    }
}