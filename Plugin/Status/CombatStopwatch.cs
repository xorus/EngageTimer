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

using System;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using EngageTimer.Configuration;
using XwContainer;

namespace EngageTimer.Status;

public class CombatStopwatch
{
    private readonly ConfigurationFile _configuration;
    private readonly IPartyList _partyList;
    private readonly State _state;
    private DateTime _combatTimeEnd;
    private DateTime _combatTimeStart;
    private bool _shouldRestartCombatTimer = true;

    public CombatStopwatch(Container container)
    {
        _state = container.Resolve<State>();
        _partyList = Bag.PartyList;
        _configuration = container.Resolve<ConfigurationFile>();
    }

    public void UpdateEncounterTimer()
    {
        if (_state.Mocked) return;
        if (_configuration.FloatingWindow.StopwatchOnlyInDuty && !_state.InInstance && !_state.InCombat) return;

        // if not in party but in combat (or self is in combat)
        // from my testing, condition flag is always identical to reading the client state status flag (but way faster)
        // (also client the party list does not exist when in solo)
        // var player = _clientState.LocalPlayer as Character;
        // var inCombat = player != null && (player.StatusFlags & StatusFlags.InCombat) != 0; 
        var inCombat = Bag.Condition[ConditionFlag.InCombat];
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