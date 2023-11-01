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
using System.Numerics;
using Dalamud.Interface.Colors;

namespace EngageTimer.Configuration;

[Serializable]
public class FloatingWindowConfiguration
{
    public bool AccurateMode { get; set; } = true;
    public bool Display { get; set; } = true;
    public bool EnableCountdown { get; set; } = false;
    public bool EnableStopwatch { get; set; } = true;
    public bool Lock { get; set; }
    public int DecimalCountdownPrecision { get; set; }
    public int DecimalStopwatchPrecision { get; set; }
    public bool StopwatchOnlyInDuty { get; set; } = false;
    public bool StopwatchAsSeconds { get; set; } = false;
    public bool CountdownNegativeSign { get; set; } = true;
    public float Scale { get; set; } = 1f;
    public bool ShowPrePulling { get; set; } = false;
    public float PrePullOffset { get; set; } = .0f;
    public Vector4 PrePullColor { get; set; } = ImGuiColors.DalamudRed;

    // Stopwatch cosmetics
    public Vector4 TextColor { get; set; } = new(255, 255, 255, 1);
    public Vector4 BackgroundColor { get; set; } = new(0, 0, 0, 0);

    public ConfigurationFile.TextAlign Align { get; set; } = ConfigurationFile.TextAlign.Left;
    public int FontSize { get; set; } = 16;
    public bool AutoHide { get; set; } = true;
    public float AutoHideTimeout { get; set; } = 20f;
}