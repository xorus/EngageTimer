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
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;

namespace EngageTimer.Ui;

public sealed class DtrBarUi : IDisposable
{
    private IDtrBarEntry? _entry;

    public DtrBarUi()
    {
        GetOrReset(Plugin.Config.Dtr.CombatTimeEnabled);
        Plugin.Config.Dtr.BarCombatTimerEnableChange += (_, _) => GetOrReset(Plugin.Config.Dtr.CombatTimeEnabled);
    }

    public void Dispose()
    {
        _entry?.Remove();
    }

    private IDtrBarEntry? GetEntry()
    {
        const string dtrBarTitle = "EngageTimer stopwatch";
        var dtrBar = Plugin.DtrBar;
        try
        {
            _entry = dtrBar.Get(dtrBarTitle);
        }
        catch (ArgumentException e)
        {
            var random = new Random();
            // this can happen when Dalamud did not have the time to update it's internal dictionary
            // https://github.com/goatcorp/Dalamud/issues/759
            for (var i = 0; i < 5; i++)
            {
                var attempt = $"{dtrBarTitle} ({random.Next().ToString()})";
                Plugin.Logger.Error(e, $"Failed to acquire DtrBarEntry {dtrBarTitle}, trying {attempt}");
                try
                {
                    _entry = dtrBar.Get(attempt);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                break;
            }
        }

        return _entry;
    }

    private void GetOrReset(bool enabled)
    {
        if (enabled) _entry = GetEntry();
        else _entry?.Remove();
    }

    private static bool CombatTimerActive()
    {
        if (!Plugin.Config.Dtr.CombatTimeEnabled) return false;
        if (Plugin.Config.Dtr.CombatTimeEnableHideAfter && (DateTime.Now - Plugin.State.CombatEnd).TotalSeconds >
            Plugin.Config.Dtr.CombatTimeHideAfter) return false;
        return !Plugin.Config.Dtr.CombatTimeAlwaysDisableOutsideDuty || Plugin.State.InInstance;
    }

    public void Update()
    {
        if (_entry == null) return;
        if (!CombatTimerActive())
        {
            if (_entry is { Shown: true }) _entry.Shown = false;
            return;
        }

        if (!_entry.Shown) _entry.Shown = true;

        var seString = (SeString)(Plugin.Config.Dtr.CombatTimePrefix +
                                  (Plugin.Config.Dtr.CombatTimeDecimalPrecision > 0
                                      ? Plugin.State.CombatDuration.ToString(@"mm\:ss\." + new string('f',
                                          Plugin.Config.Dtr.CombatTimeDecimalPrecision))
                                      : Plugin.State.CombatDuration.ToString(@"mm\:ss"))
                                  + Plugin.Config.Dtr.CombatTimeSuffix);
        if (_entry.Text == null || !_entry.Text.Equals(seString)) _entry.Text = seString;
    }
}