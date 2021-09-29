using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace EngageTimer
{
    public class StopWatchHook : IDisposable
    {
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly State _state;
        private readonly SigScanner _sig;
        private readonly Condition _condition;

        private DateTime _combatTimeEnd;

        private DateTime _combatTimeStart;

        private ulong _countDown;
        private IntPtr _countdownPtr;
        private bool _countDownRunning;

        /// <summary>
        ///     Ticks since the timer stalled
        /// </summary>
        private int _countDownStallTicks;

        private readonly CountdownTimer _countdownTimer;
        private Hook<CountdownTimer> _countdownTimerHook;
        private float _lastCountDownValue;
        private bool _shouldRestartCombatTimer = true;

        public StopWatchHook(
            DalamudPluginInterface pluginInterface,
            State state,
            SigScanner sig,
            Condition condition
        )
        {
            _pluginInterface = pluginInterface;
            _state = state;
            _sig = sig;
            _condition = condition;
            _countDown = 0;
            _countdownTimer = CountdownTimerFunc;
            HookCountdownPointer();
        }

        public void Dispose()
        {
            if (_countdownTimerHook == null) return;
            _countdownTimerHook.Disable();
            _countdownTimerHook.Dispose();
        }

        private IntPtr CountdownTimerFunc(ulong value)
        {
            _countDown = value;
            return _countdownTimerHook.Original(value);
        }

        public void Update()
        {
            if (_state.Mocked) return;
            UpdateCountDown();
            UpdateEncounterTimer();
            _state.InInstance = _condition[ConditionFlag.BoundByDuty];
        }

        private void HookCountdownPointer()
        {
            _countdownPtr = _sig.ScanText("48 89 5C 24 ?? 57 48 83 EC 40 8B 41");
            try
            {
                _countdownTimerHook = new Hook<CountdownTimer>(_countdownPtr, _countdownTimer);
                _countdownTimerHook.Enable();
            }
            catch (Exception e)
            {
                PluginLog.Error("Could not hook to timer\n" + e);
            }
        }

        private void UpdateEncounterTimer()
        {
            if (_condition[ConditionFlag.InCombat])
            {
                _state.InCombat = true;
                if (_shouldRestartCombatTimer)
                {
                    _shouldRestartCombatTimer = false;
                    _combatTimeStart = DateTime.Now;
                }

                _combatTimeEnd = DateTime.Now;
            }
            else
            {
                _state.InCombat = false;
                _shouldRestartCombatTimer = true;
            }

            _state.CombatStart = _combatTimeStart;
            _state.CombatDuration = _combatTimeEnd - _combatTimeStart;
            _state.CombatEnd = _combatTimeEnd;
        }

        private void UpdateCountDown()
        {
            _state.CountingDown = false;
            if (_countDown != 0)
            {
                var countDownPointerValue = Marshal.PtrToStructure<float>((IntPtr)_countDown + 0x2c);

                // is last value close enough (workaround for floating point approx)
                if (Math.Abs(countDownPointerValue - _lastCountDownValue) < 0.001f)
                {
                    _countDownStallTicks++;
                }
                else
                {
                    _countDownStallTicks = 0;
                    _countDownRunning = true;
                }

                if (_countDownStallTicks > 50) _countDownRunning = false;

                if (countDownPointerValue > 0 && _countDownRunning)
                {
                    _state.CountDownValue = Marshal.PtrToStructure<float>((IntPtr)_countDown + 0x2c);
                    _state.CountingDown = true;
                }

                _lastCountDownValue = countDownPointerValue;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr CountdownTimer(ulong p1);
    }
}