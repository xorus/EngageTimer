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

namespace EngageTimer.Status;

public class State
{
    private bool _countingDown;
    private bool _inCombat;
    public TimeSpan CombatDuration { get; set; }
    public DateTime CombatEnd { get; set; }
    public DateTime CombatStart { get; set; }
    public bool Mocked { get; set; }

    public bool InCombat
    {
        get => _inCombat;
        set
        {
            if (_inCombat == value) return;
            _inCombat = value;
            InCombatChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool CountingDown
    {
        get => _countingDown;
        set
        {
            if (_countingDown == value) return;
            _countingDown = value;
            CountingDownChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool InInstance { get; set; }
    public float CountDownValue { get; set; } = 0f;
    public bool PrePulling { get; set; } = false;
    public event EventHandler? InCombatChanged;
    public event EventHandler? CountingDownChanged;
}