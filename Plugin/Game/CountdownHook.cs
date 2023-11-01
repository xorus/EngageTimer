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
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using EngageTimer.Status;
using XwContainer;

/*
 * Based on the work (for finding the pointer) of https://github.com/Haplo064/Europe
 */
namespace EngageTimer.Game;

public sealed class CountdownHook : IDisposable
{
    [Signature("48 89 5C 24 ?? 57 48 83 EC 40 8B 41", DetourName = nameof(CountdownTimerFunc))]
    private readonly Hook<CountdownTimerDelegate>? _countdownTimerHook = null;

    private readonly State _state;

    private ulong _countDown;
    private bool _countDownRunning;

    /// <summary>
    ///     Ticks since the timer stalled
    /// </summary>
    private int _countDownStallTicks;

    private float _lastCountDownValue;


    public CountdownHook(Container container)
    {
        _state = container.Resolve<State>();
        _countDown = 0;
        Bag.GameInterop.InitializeFromAttributes(this);
        _countdownTimerHook?.Enable();
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
        return _countdownTimerHook!.Original(value);
    }

    public void Update()
    {
        if (_state.Mocked) return;
        UpdateCountDown();
        _state.InInstance = Bag.Condition[ConditionFlag.BoundByDuty];
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
            var newValue = Marshal.PtrToStructure<float>((IntPtr)_countDown + 0x2c);
            if (newValue > _state.CountDownValue) _state.FireStartCountingDown();
            _state.CountDownValue = newValue;
            _state.CountingDown = true;
        }

        _lastCountDownValue = countDownPointerValue;
    }

    [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    private delegate IntPtr CountdownTimerDelegate(ulong p1);
}