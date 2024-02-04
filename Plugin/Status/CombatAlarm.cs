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
using System.IO;
using System.Linq;
using Dalamud.Game.Text;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Plugin.Services;
using EngageTimer.Configuration;
using EngageTimer.Game;
using EngageTimer.Localization;
using EngageTimer.Ui;
using Newtonsoft.Json;

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

    private int? _lastCheck = null;

    public CombatAlarm()
    {
        Plugin.Config.OnSave += ConfigurationChanged;
        Plugin.Framework.Update += FrameworkUpdate;
        ConfigurationChanged(null, EventArgs.Empty);
        Plugin.State.InCombatChanged += InCombatChanged;
    }

    public static string? Import(string fileName)
    {
        try
        {
            var text = File.ReadAllText(fileName);
            var data = JsonConvert.DeserializeObject<List<CombatAlarmsConfiguration.Alarm>>(text,
                new JsonSerializerSettings
                {
                    // using "TypeNameHandling.Objects" causes a "resolving to a collectible assembly is not supported"
                    TypeNameHandling = TypeNameHandling.None
                });
            if (data == null || data.Count == 0) return Translator.Tr("CombatAlarm_ImportedEmpty");
            Plugin.Config.CombatAlarms.Alarms.AddRange(data);
        }
        catch (JsonSerializationException e)
        {
            Plugin.Logger.Error(e, $"Could not parse file {fileName}");
            return Translator.Tr("CombatAlarm_IncorrectFormat");
        }
        catch (Exception e)
        {
            Plugin.Logger.Error(e, $"Could not read file {fileName}");
            return Translator.Tr("CombatAlarm_ReadGeneric", fileName, e.Message);
        }

        return null;
    }

    public static string? Export(string fileName)
    {
        try
        {
            File.WriteAllText(fileName,
                JsonConvert.SerializeObject(Plugin.Config.CombatAlarms.Alarms.Where(alarm => alarm.Enabled).ToList(),
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.None
                    }));
        }
        catch (UnauthorizedAccessException)
        {
            return Translator.Tr("CombatAlarm_AccessDenied");
        }
        catch (Exception e)
        {
            Plugin.Logger.Error(e, $"Could not save file {fileName}");
            return Translator.Tr("CombatAlarm_SaveGeneric", fileName, e.Message);
        }

        return null;
    }

    private void ConfigurationChanged(object? sender, EventArgs e)
    {
        ClearAlarms();
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

    private static void InCombatChanged(object? sender, EventArgs e)
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
        var time = (int) Math.Floor(Plugin.State.CombatDuration.TotalSeconds);
        if (_lastCheck == time) return;
        _lastCheck = time;

        if (!_alarms.TryGetValue(time, out var alarm)) return;

        if (alarm.Type == AlarmActionType.Start)
        {
            RunAlarm(alarm.Config);
            return;
        }

        if (alarm.Type == AlarmActionType.Stop) ClearAlarms();
    }

    public static void AlarmSfx(CombatAlarmsConfiguration.Alarm alarm)
    {
        if (alarm.Sfx != null)
        {
            Plugin.SfxPlay.SoundEffect((uint) (SfxPlay.FirstSeSfx + alarm.Sfx));
        }
    }

    public static void AlarmText(CombatAlarmsConfiguration.Alarm alarm)
    {
        var trimText = alarm.Text?.Trim();
        if (trimText is not {Length: > 0}) return;
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

    private static void RunAlarm(CombatAlarmsConfiguration.Alarm alarm)
    {
        AlarmSfx(alarm);
        AlarmText(alarm);
        if (alarm.Color == null) return;
        Plugin.State.OverrideFwColor = alarm.Color;
        Plugin.State.BlinkStopwatch = alarm.Blink;
    }

    private static void ClearAlarms()
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