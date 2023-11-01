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

namespace EngageTimer.Configuration;

[Serializable]
public class CountdownConfiguration
{
    public static readonly string[] BundledTextures =
        { "default", "yellow", "wow", "awk", "tall", "misaligned", "pixel", "moire", "mspaint" };

    // Countdown
    public bool Display { get; set; } = true;
    public bool HideOriginalAddon { get; set; } = false;
    public bool EnableTickingSound { get; set; } = false;
    public bool UseAlternativeSound { get; set; } = false;
    public int StartTickingFrom { get; set; } = 30;
    public bool EnableDecimals { get; set; } = false;
    public int DecimalPrecision { get; set; } = 1;
    public bool EnableDisplayThreshold { get; set; } = false;
    public int DisplayThreshold { get; set; } = 5;

    // Countdown style
    public string TexturePreset { get; set; } = "default";
    public string TextureDirectory { get; set; } = null;
    public float Scale { get; set; } = 1f;
    public bool Monospaced { get; set; }
    public float? CustomNegativeMargin { get; set; } = null;
    public bool LeadingZero { get; set; }

    // Countdown color
    public int Hue { get; set; }
    public float Saturation { get; set; }
    public float Luminance { get; set; }
    public bool NumberRecolorMode { get; set; }
    public bool Animate { get; set; }
    public bool AnimateScale { get; set; } = true;
    public bool AnimateOpacity { get; set; } = true;
    public Vector2 WindowOffset { get; set; } = Vector2.Zero;

    public bool AccurateMode { get; set; } = false;
    public ConfigurationFile.TextAlign Align { get; set; } = ConfigurationFile.TextAlign.Center;
}