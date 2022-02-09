using System;
using Dalamud.Game.Gui.Dtr;

namespace EngageTimer.UI
{
    public class DtrBarUi : IDisposable
    {
        private readonly Configuration _configuration;
        private readonly State _state;
        private DtrBarEntry? _entry = null;

        public DtrBarUi(Configuration configuration, State state, DtrBar dtrBar)
        {
            _configuration = configuration;
            _state = state;
            GetOrReset(_configuration.DtrCombatTimeEnabled, dtrBar);
            _configuration.DtrBarCombatTimerEnableChange +=
                (_, _) => GetOrReset(_configuration.DtrCombatTimeEnabled, dtrBar);
        }

        private void GetOrReset(bool enabled, DtrBar dtrBar)
        {
            if (enabled) _entry = dtrBar.Get("EngageTimer stopwatch");
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
            _entry.Text = _configuration.DtrCombatTimePrefix +
                          (_configuration.DtrCombatTimeDecimalPrecision > 0
                              ? _state.CombatDuration.ToString(@"mm\:ss\." + new string('f',
                                  _configuration.DtrCombatTimeDecimalPrecision))
                              : _state.CombatDuration.ToString(@"mm\:ss"))
                          + _configuration.DtrCombatTimeSuffix;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _entry?.Dispose();
        }
    }
}