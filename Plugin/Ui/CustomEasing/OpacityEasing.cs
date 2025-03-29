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

public class OpacityEasing : Easing
{
    public OpacityEasing() : base(new TimeSpan(0, 0, 0, 0, 1000))
    {
    }

    // https://www.desmos.com/calculator/6btgm8tjk0
    public override void Update()
    {
        ValueUnclamped = Math.Clamp(
            0.08 - 0.9 * Math.Sin(3 - 7.5 * Progress)
            , 0d, 1d);
    }
}