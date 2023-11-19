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
using System.Collections.Generic;
using System.Net.Mime;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Plugin.Services;
using EngageTimer.Configuration.Legacy;
using EngageTimer.Game;

namespace EngageTimer.Status;

public sealed class CombatAlarm : IDisposable
{
    private enum AlarmActionType
    {
        Start,
        Stop
    }

    private class AlarmAction
    {
        public AlarmActionType Type { get; init; }
        public int Id { get; init; }
        public CombatAlarmsConfiguration.Alarm Config { get; init; } = null!;
    }

    private readonly Dictionary<int, AlarmAction> _alarms = new();

    // private readonly List<int> _rangAlarms = new();
    private int? _lastCheck = null;

    public CombatAlarm()
    {
        Plugin.Config.OnSave += ConfigurationChanged;
        Plugin.Framework.Update += FrameworkUpdate;
        ConfigurationChanged(null, EventArgs.Empty);
        Plugin.State.InCombatChanged += InCombatChanged;
    }


    private void ConfigurationChanged(object? sender, EventArgs e)
    {
        _alarms.Clear();
        for (var index = 0; index < Plugin.Config.CombatAlarms.Alarms.Count; index++)
        {
            var alarm = Plugin.Config.CombatAlarms.Alarms[index];
            _alarms[alarm.StartTime] = new AlarmAction()
            {
                Type = AlarmActionType.Start,
                Id = index,
                Config = alarm
            };
            _alarms[alarm.StartTime + alarm.Duration] = new AlarmAction()
            {
                Type = AlarmActionType.Stop,
                Id = index,
                Config = alarm
            };
        }
    }

    private void InCombatChanged(object? sender, EventArgs e)
    {
        if (!Plugin.State.InCombat)
        {
            // + clear whatever alarms are up
            // _rangAlarms.Clear();
            ClearAlarms();
        }
    }

    private void FrameworkUpdate(IFramework framework)
    {
        if (!Plugin.State.InCombat) return;

        // only run once a second
        var time = (int)Math.Floor(Plugin.State.CombatDuration.TotalSeconds);
        if (_lastCheck == time) return;
        _lastCheck = time;

        if (!_alarms.TryGetValue(time, out var alarm)) return;

        if (alarm.Type == AlarmActionType.Start)
        {
            RunAlarm(alarm.Config);
            // _rangAlarms.Add(time);
        }

        if (alarm.Type == AlarmActionType.Stop)
        {
            ClearAlarms();
        }
    }

    private void RunAlarm(CombatAlarmsConfiguration.Alarm alarm)
    {
        if (alarm.Sfx != null)
        {
            // Plugin.SfxPlay.SoundEffect();
            Plugin.SfxPlay.SoundEffect((uint)(SfxPlay.FirstSeSfx + alarm.Sfx));
        }

        var trimText = alarm.Text?.Trim();
        if (trimText is { Length: > 0 })
        {
            switch (alarm.TextType)
            {
                case CombatAlarmsConfiguration.TextType.DalamudNotification:
                    Plugin.PluginInterface.UiBuilder.AddNotification(
                        trimText,
                        "EngageTimer",
                        NotificationType.Info,
                        8000
                    );
                    break;
                case CombatAlarmsConfiguration.TextType.GameToast:
                    Plugin.ToastGui.ShowNormal(trimText);
                    break;
                case CombatAlarmsConfiguration.TextType.ChatLogMessage:
                    // Plugin.ChatGui.Print(trimText, "EngageTimer");
                    Plugin.ChatGui.Print(new XivChatEntry()
                    {
                        Type = XivChatType.Echo,
                        Name = "EngageTimer",
                        Message = trimText
                    });
                    break;
            }
        }

        if (alarm.Color != null)
        {
            Plugin.State.OverrideFwColor = alarm.Color;
            Plugin.State.BlinkStopwatch = alarm.Blink;
        }
    }

    private void ClearAlarms()
    {
        Plugin.State.OverrideFwColor = null;
        Plugin.State.BlinkStopwatch = false;
    }

    public void Dispose()
    {
        Plugin.Config.OnSave -= ConfigurationChanged;
        Plugin.Framework.Update -= FrameworkUpdate;
    }
}