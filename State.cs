using System;

namespace EngageTimer
{
    public class State
    {
        public TimeSpan CombatDuration { get; set; }
        public DateTime CombatEnd { get; set; }
        public DateTime CombatStart { get; set; }
        public bool InCombat { get; set; }
        public bool CountingDown { get; set; }
        public float CountDownValue { get; set; } = 0f;
    }
}