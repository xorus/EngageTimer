using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Hooking;
using Dalamud.Logging;

/*
 * Based on the work (for finding the pointer) of https://github.com/Haplo064/Europe
 */
namespace EngageTimer
{
    public class StopWatchHook : IDisposable
    {
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
        private readonly PartyList _partyList;

        public StopWatchHook(State state,
            SigScanner sig,
            Condition condition, PartyList partyList)
        {
            _state = state;
            _sig = sig;
            _condition = condition;
            _countDown = 0;
            _countdownTimer = CountdownTimerFunc;
            _partyList = partyList;
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
            // if not in party but in combat (or self is in combat)
            // from my testing, condition flag is always identical to reading the client state status flag (but way faster)
            // var player = _clientState.LocalPlayer as Character;
            // var inCombat = player != null && (player.StatusFlags & StatusFlags.InCombat) != 0; 
            var inCombat = _condition[ConditionFlag.InCombat];
            if (!inCombat)
            {
                // if anyone in the party is in combat
                foreach (var actor in _partyList)
                {
                    if (actor.GameObject is not Character character ||
                        (character.StatusFlags & StatusFlags.InCombat) == 0) continue;
                    inCombat = true;
                    break;
                }
            }

            if (inCombat)
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
            if (_countDown == 0) return;
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

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr CountdownTimer(ulong p1);
    }
}