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

#nullable enable
using System;
using System.Numerics;
using System.Text.Json.Serialization;
using Dalamud.Interface.Colors;
using Dalamud.Interface.FontIdentifier;
using EngageTimer.Attributes;
using EngageTimer.Ui;

namespace EngageTimer.Configuration;

[Serializable]
public class FloatingWindowConfiguration
{
    [AutoField("Settings_FWTab_AccurateCountdown")]
    public bool AccurateMode { get; set; } = true;

    [AutoField("Settings_FWTab_Display")] public bool Display { get; set; } = true;

    [AutoField("Settings_FWTab_CountdownPrecision_With_Left")]
    public bool EnableCountdown { get; set; } = false;

    [AutoField("Settings_FWTab_StopwatchPrecision_With_Left")]
    public bool EnableStopwatch { get; set; } = true;

    [AutoField("Settings_FWTab_Lock")] public bool Lock { get; set; }

    [AutoField("Settings_FWTab_CountdownPrecision_Right", 0, 3), ItemWidth(70f),
     Help("Settings_FWTab_CountdownPrecision_Help")]
    public int DecimalCountdownPrecision { get; set; }

    [AutoField("Settings_FWTab_StopwatchPrecision_Right", 0, 3), ItemWidth(70f),
     Help("Settings_FWTab_StopwatchPrecision_Help")]
    public int DecimalStopwatchPrecision { get; set; }

    [AutoField("Settings_FWTab_DisplayStopwatchOnlyInDuty")]
    public bool StopwatchOnlyInDuty { get; set; } = false;
    
    [AutoField("Settings_FWTab_HideInCutscenes")]
    public bool HideInCutscenes { get; set; } = true;

    [AutoField("Settings_FWTab_HideWhileOccupiedInCombat")]
    public bool HideWhileOccupiedInCombat { get; set; } = false;

    [AutoField("Settings_FWTab_StopwatchAsSeconds")]
    public bool StopwatchAsSeconds { get; set; } = false;

    [AutoField("Settings_FWTab_CountdownNegativeSign")]
    public bool CountdownNegativeSign { get; set; } = true;
    
    [AutoField("Settings_FWTab_ForceHideWindowBorder")]
    public bool ForceHideWindowBorder { get; set; } = true;

    [AutoField("Settings_CountdownTab_FloatingWindowScale", Components.FieldType.DragFloat, .01f, .05f, 15f),
     ItemWidth(100f)]
    public float Scale { get; set; } = 1f;

    [AutoField("Settings_FWTab_ShowPrePulling")]
    public bool ShowPrePulling { get; set; } = false;

    [AutoField("Settings_FWTab_PrePullOffset", Components.FieldType.InputFloat, 0.1f, 1f, "%.3fs")]
    public float PrePullOffset { get; set; } = .0f;

    [AutoField("Settings_FWTab_TextColor"), ColorPicker(1)]
    public Vector4 PrePullColor { get; set; } = ImGuiColors.DalamudRed;

    // Stopwatch cosmetics
    [AutoField("Settings_FWTab_TextColor"), ColorPicker(2)]
    public Vector4 TextColor { get; set; } = new(255, 255, 255, 1);

    [AutoField("Settings_FWTab_BackgroundColor"), ColorPicker(3)]
    public Vector4 BackgroundColor { get; set; } = new(0, 0, 0, 0);

    public ConfigurationFile.TextAlign Align { get; set; } = ConfigurationFile.TextAlign.Left;

    public int FontSize { get; set; } = 16;
    
    [JsonIgnore] public IFontSpec? FontSpec { get; set; } = null;
    public IFontSpec? Font { get; set; } = null;

    [AutoField("Settings_FWTab_AutoHide_Left")]
    public bool AutoHide { get; set; } = true;

    [AutoField(
        "Settings_FWTab_AutoHide_Right",
        Components.FieldType.InputFloat,
        .1f,
        1f,
        "%.1f%"
    )]
    public float AutoHideTimeout { get; set; } = 20f;
}