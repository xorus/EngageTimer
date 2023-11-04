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
using EngageTimer.Attributes;
using EngageTimer.Ui;

namespace EngageTimer.Configuration;

[Serializable]
public class DtrConfiguration
{
    public const string DefaultCombatTimePrefix = "【 ";
    public const string DefaultCombatTimeSuffix = "】";

    // Dtr bar
    [NonSerialized] private bool _dtrCombatTimeEnabled;

    public bool CombatTimeEnabled
    {
        get => _dtrCombatTimeEnabled;
        set
        {
            _dtrCombatTimeEnabled = value;
            BarCombatTimerEnableChange?.Invoke(this, EventArgs.Empty);
        }
    }

    [AutoField("Settings_DtrCombatTimer_Prefix"), MinMax(0, 50)]
    public string CombatTimePrefix { get; set; } = DefaultCombatTimePrefix;

    [AutoField("Settings_DtrCombatTimer_Suffix"), MinMax(0, 50)]
    public string CombatTimeSuffix { get; set; } = DefaultCombatTimeSuffix;

    [AutoField("Settings_DtrCombatTimer_DecimalPrecision"), MinMax(0, 3)]
    public int CombatTimeDecimalPrecision { get; set; } = 0;

    [AutoField("Settings_DtrCombatTimer_AlwaysDisableOutsideDuty")]
    public bool CombatTimeAlwaysDisableOutsideDuty { get; set; }

    [AutoField("Settings_DtrCombatTimer_HideAfter")]
    public bool CombatTimeEnableHideAfter { get; set; } = false;

    [AutoField("Settings_DtrCombatTimer_HideAfterRight", Components.FieldType.InputFloat, 0.1f, 1f, "%.1f%")]
    public float CombatTimeHideAfter { get; set; } = 20f;
    public event EventHandler? BarCombatTimerEnableChange;
}