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
    private readonly double _p0;
    private readonly double _p1;
    private readonly double _p2;
    private readonly double _p3;

    public OpacityEasing(TimeSpan duration, double p0, double p1, double p2, double p3) : base(duration)
    {
        _p0 = p0;
        _p1 = p1;
        _p2 = p2;
        _p3 = p3;
    }

    // https://www.desmos.com/calculator/6btgm8tjk0
    public override void Update()
    {
        Value = Math.Clamp(
            0.08 - 0.9 * Math.Sin(3 - 7.5 * Progress)
            , 0d, 1d);
    }
}