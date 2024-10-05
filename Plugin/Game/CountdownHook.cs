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

/*
 * Based on the work (for finding the pointer) of https://github.com/Haplo064/Europe
 * 7.0 function changes taken from https://github.com/DelvUI/DelvUI/commit/492211c8f43b813d10b2220e6fe768ac508dcede and
 * https://discord.com/channels/581875019861328007/653504487352303619/1257920418388639774. Thanks Tischel for that work!
 */
namespace EngageTimer.Game;

public sealed class CountdownHook : IDisposable
{
    [Signature("40 53 48 83 EC 40 80 79 38 00", DetourName = nameof(CountdownTimerFunc))]
    private readonly Hook<CountdownTimerDelegate>? _countdownTimerHook = null;

    private readonly State _state;

    private ulong _paramValue;

    public CountdownHook()
    {
        _state = Plugin.State;
        _paramValue = 0;
        Plugin.GameInterop.InitializeFromAttributes(this);
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
        _paramValue = value;
        return _countdownTimerHook!.Original(value);
    }

    public void Update()
    {
        if (_state.Mocked) return;
        UpdateCountDown();
        _state.InInstance = Plugin.Condition[ConditionFlag.BoundByDuty];
        _state.InCutscene = Plugin.Condition[ConditionFlag.OccupiedInCutSceneEvent];
    }

    private void UpdateCountDown()
    {
        if (_paramValue == 0) return;
        var countDownActive = Marshal.PtrToStructure<byte>((IntPtr)_paramValue + 0x38) == 1;
        var countDownPointerValue = Marshal.PtrToStructure<float>((IntPtr)_paramValue + 0x2c);
        _state.CountingDown = countDownActive && countDownPointerValue > 0f;
        _state.CountDownValue = countDownPointerValue;
    }

    [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    private delegate IntPtr CountdownTimerDelegate(ulong p1);
}