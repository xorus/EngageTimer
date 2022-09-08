using System;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using XwContainer;

namespace EngageTimer.Status;

public class CombatStopwatch
{
    private readonly Condition _condition;
    private readonly Configuration _configuration;
    private readonly PartyList _partyList;
    private readonly State _state;
    private DateTime _combatTimeEnd;
    private DateTime _combatTimeStart;
    private bool _shouldRestartCombatTimer = true;

    public CombatStopwatch(Container container)
    {
        _state = container.Resolve<State>();
        _condition = container.Resolve<Condition>();
        _partyList = container.Resolve<PartyList>();
        _configuration = container.Resolve<Configuration>();
    }

    public void UpdateEncounterTimer()
    {
        if (_state.Mocked) return;
        if (_configuration.FloatingWindowDisplayStopwatchOnlyInDuty && !_state.InInstance && !_state.InCombat) return;

        // if not in party but in combat (or self is in combat)
        // from my testing, condition flag is always identical to reading the client state status flag (but way faster)
        // (also client the party list does not exist when in solo)
        // var player = _clientState.LocalPlayer as Character;
        // var inCombat = player != null && (player.StatusFlags & StatusFlags.InCombat) != 0; 
        var inCombat = _condition[ConditionFlag.InCombat];
        if (!inCombat)
            // if anyone in the party is in combat
            foreach (var actor in _partyList)
            {
                if (actor.GameObject is not Character character ||
                    (character.StatusFlags & StatusFlags.InCombat) == 0) continue;
                inCombat = true;
                break;
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
}