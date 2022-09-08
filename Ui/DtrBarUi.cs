using System;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using EngageTimer.Status;
using XwContainer;

namespace EngageTimer.Ui;

public sealed class DtrBarUi : IDisposable
{
    private readonly Configuration _configuration;
    private readonly DtrBar _dtrBar;
    private readonly State _state;
    private DtrBarEntry _entry;

    public DtrBarUi(Container container)
    {
        _configuration = container.Resolve<Configuration>();
        _state = container.Resolve<State>();
        _dtrBar = container.Resolve<DtrBar>();
        GetOrReset(_configuration.DtrCombatTimeEnabled);
        _configuration.DtrBarCombatTimerEnableChange +=
            (_, _) => GetOrReset(_configuration.DtrCombatTimeEnabled);
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
                PluginLog.LogError(e, $"Failed to acquire DtrBarEntry {dtrBarTitle}, trying {attempt}");
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
        if (!_configuration.DtrCombatTimeEnabled) return false;
        if (_configuration.DtrCombatTimeEnableHideAfter && (DateTime.Now - _state.CombatEnd).TotalSeconds >
            _configuration.DtrCombatTimeHideAfter) return false;
        return !_configuration.DtrCombatTimeAlwaysDisableOutsideDuty || _state.InInstance;
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

        var seString = (SeString)(_configuration.DtrCombatTimePrefix +
                                  (_configuration.DtrCombatTimeDecimalPrecision > 0
                                      ? _state.CombatDuration.ToString(@"mm\:ss\." + new string('f',
                                          _configuration.DtrCombatTimeDecimalPrecision))
                                      : _state.CombatDuration.ToString(@"mm\:ss"))
                                  + _configuration.DtrCombatTimeSuffix);
        if (_entry.Text == null || !_entry.Text.Equals(seString)) _entry.Text = seString;
    }
}