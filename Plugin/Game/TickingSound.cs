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

namespace EngageTimer.Game;

public class TickingSound
{
    private readonly SfxPlay _sfx = new();

    private int? _lastNumberPlayed;

    // This is a workaround for CLR taking some time to init the pointy method call. 
    private bool _soundLoaded;

    public void Update()
    {
        // if (!_configuration.DisplayCountdown) return;
        var configuration = Plugin.Config;
        var state = Plugin.State;
        if (!configuration.Countdown.EnableTickingSound || state.Mocked) return;
        if (!_soundLoaded)
        {
            _sfx.SoundEffect(0); // should be cursor sound
            _soundLoaded = true;
            return;
        }

        if (state.CountingDown &&
            state.CountDownValue > 5 && state.CountDownValue <= configuration.Countdown.StartTickingFrom)
            TickSound((int)Math.Ceiling(state.CountDownValue));
    }

    private void TickSound(int n)
    {
        var configuration = Plugin.Config;
        if (!configuration.Countdown.EnableTickingSound || _lastNumberPlayed == n)
            return;
        _lastNumberPlayed = n;
        _sfx.SoundEffect(configuration.Countdown.UseAlternativeSound ? SfxPlay.SmallTick : SfxPlay.CdTick);
    }
}