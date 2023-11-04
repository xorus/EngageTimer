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
using EngageTimer.Attributes;
using EngageTimer.Ui;

namespace EngageTimer.Configuration;

[Serializable]
public class CountdownConfiguration
{
    public static readonly string[] BundledTextures =
        { "default", "yellow", "wow", "awk", "tall", "misaligned", "pixel", "moire", "mspaint" };

    // Countdown
    [AutoField("Settings_CountdownTab_Enable")]
    public bool Display { get; set; } = true;

    [AutoField("Settings_CountdownTab_HideOriginalCountDown"), Help("Settings_CountdownTab_HideOriginalCountDown_Help")]
    public bool HideOriginalAddon { get; set; } = false;

    [AutoField("Settings_CountdownTab_Audio_Enable")]
    public bool EnableTickingSound { get; set; } = false;

    [AutoField("Settings_CountdownTab_Audio_UseAlternativeSound")]
    public bool UseAlternativeSound { get; set; } = false;

    [AutoField("Settings_CountdownTab_TickFrom"), MinMax(5, 30),
     Help("Settings_CountdownTab_TickFrom_Help")]
    public int StartTickingFrom { get; set; } = 30;

    [AutoField("Settings_CountdownTab_CountdownDecimals_Left")]
    public bool EnableDecimals { get; set; } = false;

    [AutoField("Settings_CountdownTab_CountdownDecimals_Right"), ItemWidth(70f), MinMax(1, 3)]
    public int DecimalPrecision { get; set; } = 1;

    [AutoField("Settings_CountdownTab_CountdownDisplayThreshold")]
    public bool EnableDisplayThreshold { get; set; } = false;

    [AutoField(Id = "Settings_CountdownTab_CountdownDisplayThreshold_Value"), MinMax(0, 30),
     Help("Settings_CountdownTab_CountdownDisplayThreshold_Help")]
    public int DisplayThreshold { get; set; } = 5;

    // Countdown style
    public string TexturePreset { get; set; } = "default";
    public string? TextureDirectory { get; set; } = null;

    [AutoField("Settings_CountdownTab_CountdownScale"), MinMax(.05f, 15f), ItemWidth(100f)]
    public float Scale { get; set; } = 1f;

    [AutoField("Settings_CountdownTab_Monospaced")]
    public bool Monospaced { get; set; }

    public float? CustomNegativeMargin { get; set; } = null;

    [AutoField("Settings_CountdownTab_NumberStyle_LeadingZero")]
    public bool LeadingZero { get; set; }

    // Countdown color
    public int Hue { get; set; }
    public float Saturation { get; set; }
    public float Luminance { get; set; }
    public bool NumberRecolorMode { get; set; }

    [AutoField("Settings_CountdownTab_Animate")]
    public bool Animate { get; set; }

    [AutoField("Settings_CountdownTab_AnimateScale")]
    public bool AnimateScale { get; set; } = true;

    [AutoField("Settings_CountdownTab_AnimateOpacity")]
    public bool AnimateOpacity { get; set; } = true;

    public Vector2 WindowOffset { get; set; } = Vector2.Zero;

    [AutoField("Settings_CountdownTab_AccurateMode")]
    public bool AccurateMode { get; set; } = false;

    public ConfigurationFile.TextAlign Align { get; set; } = ConfigurationFile.TextAlign.Center;
}