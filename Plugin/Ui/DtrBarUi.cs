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
using Dalamud.Plugin.Services;
using EngageTimer.Configuration;
using EngageTimer.Status;
using XwContainer;

namespace EngageTimer.Ui;

public sealed class DtrBarUi : IDisposable
{
    private readonly ConfigurationFile _configuration;
    private readonly IDtrBar _dtrBar;
    private readonly State _state;
    private DtrBarEntry _entry;

    public DtrBarUi(Container container)
    {
        _configuration = container.Resolve<ConfigurationFile>();
        _state = container.Resolve<State>();
        _dtrBar = Bag.DtrBar;
        GetOrReset(_configuration.Dtr.CombatTimeEnabled);
        _configuration.Dtr.BarCombatTimerEnableChange +=
            (_, _) => GetOrReset(_configuration.Dtr.CombatTimeEnabled);
    }

    public void Dispose()
    {
        _entry?.Dispose();
    }

    private DtrBarEntry GetEntry()
    {
        var dtrBarTitle = "EngageTimer stopwatch";
        try
        {
            _entry = _dtrBar.Get(dtrBarTitle);
        }
        catch (ArgumentException e)
        {
            var random = new Random();
            // this can happen when Dalamud did not have the time to update it's internal dictionary
            // https://github.com/goatcorp/Dalamud/issues/759
            for (var i = 0; i < 5; i++)
            {
                var attempt = $"{dtrBarTitle} ({random.Next().ToString()})";
                Bag.Logger.Error(e, $"Failed to acquire DtrBarEntry {dtrBarTitle}, trying {attempt}");
                try
                {
                    _entry = _dtrBar.Get(attempt);
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

    private bool CombatTimerActive()
    {
        if (!_configuration.Dtr.CombatTimeEnabled) return false;
        if (_configuration.Dtr.CombatTimeEnableHideAfter && (DateTime.Now - _state.CombatEnd).TotalSeconds >
            _configuration.Dtr.CombatTimeHideAfter) return false;
        return !_configuration.Dtr.CombatTimeAlwaysDisableOutsideDuty || _state.InInstance;
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

        var seString = (SeString)(_configuration.Dtr.CombatTimePrefix +
                                  (_configuration.Dtr.CombatTimeDecimalPrecision > 0
                                      ? _state.CombatDuration.ToString(@"mm\:ss\." + new string('f',
                                          _configuration.Dtr.CombatTimeDecimalPrecision))
                                      : _state.CombatDuration.ToString(@"mm\:ss"))
                                  + _configuration.Dtr.CombatTimeSuffix);
        if (_entry.Text == null || !_entry.Text.Equals(seString)) _entry.Text = seString;
    }
}