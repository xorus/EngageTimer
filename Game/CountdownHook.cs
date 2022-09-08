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
    private readonly Condition _condition;

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
        _condition = container.Resolve<Condition>();
        _countDown = 0;
        SignatureHelper.Initialise(this);
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
        _state.InInstance = _condition[ConditionFlag.BoundByDuty];
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