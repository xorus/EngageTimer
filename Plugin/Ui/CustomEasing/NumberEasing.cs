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
using Dalamud.Interface.Animation;

namespace EngageTimer.Ui.CustomEasing;

public class NumberEasing : Easing
{
    public const float StartSize = .76f;

    public NumberEasing() : base(new TimeSpan(0, 0, 0, 0, 400))
    {
    }

    /**
     * I did not do a frame analysis yet, but it seems linear, I hate it but must match it.
     */
    public override void Update()
    {
        if (this.Progress > 1) return;
        this.ValueUnclamped = this.Progress;
    }
}