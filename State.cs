using System;

namespace EngageTimer;

public class State
{
    private bool _countingDown;
    private bool _inCombat;
    public TimeSpan CombatDuration { get; set; }
    public DateTime CombatEnd { get; set; }
    public DateTime CombatStart { get; set; }

    public bool Mocked { get; set; }

    public bool InCombat
    {
        get => _inCombat;
        set
        {
            if (_inCombat == value) return;
            _inCombat = value;
            InCombatChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool CountingDown
    {
        get => _countingDown;
        set
        {
            if (_countingDown == value) return;
            _countingDown = value;
            CountingDownChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool InInstance { get; set; }

    public float CountDownValue { get; set; } = 0f;
    public event EventHandler InCombatChanged;
    public event EventHandler CountingDownChanged;
}