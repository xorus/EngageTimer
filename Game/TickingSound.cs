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
using EngageTimer.Configuration;
using EngageTimer.Status;
using XwContainer;

namespace EngageTimer.Game;

public class TickingSound
{
    private readonly ConfigurationFile _configuration;
    private readonly SfxPlay _sfx = new();
    private readonly State _state;

    private int? _lastNumberPlayed;

    // This is a workaround for CLR taking some time to init the pointy method call. 
    private bool _soundLoaded;

    public TickingSound(Container container)
    {
        _configuration = container.Resolve<ConfigurationFile>();
        _state = container.Resolve<State>();
    }

    public void Update()
    {
        // if (!_configuration.DisplayCountdown) return;
        if (!_configuration.Countdown.EnableTickingSound || _state.Mocked) return;
        if (!_soundLoaded)
        {
            _sfx.SoundEffect(0); // should be cursor sound
            _soundLoaded = true;
            return;
        }

        if (_state.CountingDown &&
            _state.CountDownValue > 5 && _state.CountDownValue <= _configuration.Countdown.StartTickingFrom)
            TickSound((int)Math.Ceiling(_state.CountDownValue));
    }

    private void TickSound(int n)
    {
        if (!_configuration.Countdown.EnableTickingSound || _lastNumberPlayed == n)
            return;
        _lastNumberPlayed = n;
        _sfx.SoundEffect(_configuration.Countdown.UseAlternativeSound ? SfxPlay.SmallTick : SfxPlay.CdTick);
    }
}