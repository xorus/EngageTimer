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
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.ImGuiFileDialog;
using EngageTimer.Configuration;
using EngageTimer.Status;
using ImGuiNET;

namespace EngageTimer.Ui.SettingsTab;

public static class AlarmsTab
{
    private static readonly FileDialogManager Fdm = new();
    private static readonly Modal Modal = new();

    private static void Header(bool tooltip = false)
    {
        ImGui.TableNextColumn();
        var str = ImGui.TableGetColumnName(ImGui.TableGetColumnIndex());
        ImGui.TableHeader(Translator.Tr(ImGui.TableGetColumnName(ImGui.TableGetColumnIndex())));
        if (tooltip) Components.TooltipOnItemHovered(Translator.Tr(str + "_Tooltip"));
    }

    /**
     * Can't open a popup from within a table apparently
     */
    private static bool _openConfirmClear = false;

    private static readonly List<int> EditingTexts = new();

    public static void Draw()
    {
        ImGui.Text(Translator.Tr("Settings_AlarmsTab_Line1"));
        ImGui.Text(Translator.Tr("Settings_AlarmsTab_Line2"));
        ImGui.Separator();

        if (ImGui.BeginTable("alarms", 8, ImGuiTableFlags.Borders))
        {
            ImGui.TableSetupColumn(" ");
            ImGui.TableSetupColumn("AlarmEdit_Active");
            ImGui.TableSetupColumn("AlarmEdit_StartTime");
            ImGui.TableSetupColumn("AlarmEdit_Color");
            ImGui.TableSetupColumn("AlarmEdit_Blink");
            ImGui.TableSetupColumn("AlarmEdit_Duration");
            ImGui.TableSetupColumn("AlarmEdit_Sound");
            ImGui.TableSetupColumn("AlarmEdit_Text", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
            // trash can
            {
                ImGui.TableNextColumn();
                ImGui.Text("");
            }
            Header(true); // active
            Header(true); // start time
            Header(true); // color
            Header(true); // blink
            Header(true); // duration
            Header(true); // sound 
            Header(true); // text notification

            for (var index = 0; index < Plugin.Config.CombatAlarms.Alarms.Count; index++)
            {
                ImGui.PushID("alarm" + index);
                AlarmElement(index, Plugin.Config.CombatAlarms.Alarms[index]);
                ImGui.PopID();
            }

            ImGui.EndTable();
        }

        Components.LeftRight("buttons", () =>
        {
            if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.FileImport, Translator.Tr("AlarmEdit_Import")))
            {
                Fdm.OpenFileDialog(
                    Translator.Tr("AlarmEdit_Import_File"),
                    ".json",
                    (ok, path) =>
                    {
                        if (!ok) return;
                        foreach (var p in path)
                        {
                            var error = CombatAlarm.Import(p);
                            if (error != null) Modal.Show(error);
                        }
                    },
                    0,
                    null,
                    true
                );
            }


            ImGui.SameLine();
            if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.FileExport, Translator.Tr("AlarmEdit_Export")))
            {
                Fdm.SaveFileDialog(
                    Translator.Tr("AlarmEdit_Export_File"),
                    ".json",
                    "EngageTimerAlarms.json",
                    "json",
                    (ok, path) =>
                    {
                        if (ok)
                        {
                            var error = CombatAlarm.Export(path);
                            Plugin.Logger.Info("got error" + error);
                            if (error != null) Modal.Show(error);
                        }

                        Plugin.Logger.Info($"got {ok} file {path}");
                    },
                    null,
                    true
                );
            }

            Components.TooltipOnItemHovered("AlarmEdit_Export_Tooltip");

            ImGui.SameLine();
            // clear all button
            if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Trash, Translator.Tr("AlarmEdit_Clear")))
            {
                if (Plugin.Config.CombatAlarms.Alarms.Count == 0) return;
                _openConfirmClear = true;
            }
        }, () =>
        {
            if (!ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Plus, Translator.Tr("AlarmEdit_Add"))) return;
            Plugin.Config.CombatAlarms.Alarms.Add(new CombatAlarmsConfiguration.Alarm());
            Plugin.Config.Save();
        });

        if (_openConfirmClear)
        {
            _openConfirmClear = false;
            Modal.Confirm(Translator.Tr("AlarmEdit_Clear_Confirm"), () =>
            {
                Plugin.Config.CombatAlarms.Alarms.Clear();
                Plugin.Config.Save();
            });
        }

        Fdm.Draw();
        Modal.Draw();
    }

    private static void AlarmElement(int index, CombatAlarmsConfiguration.Alarm alarm)
    {
        ImGui.TableNextRow();
        {
            ImGui.TableNextColumn();
            ImGui.PushID("alarm_" + index);
            Components.IconButton(FontAwesomeIcon.Trash, "delete" + index,
                () =>
                {
                    EditingTexts.Remove(index);
                    Plugin.Config.CombatAlarms.Alarms.RemoveAt(index);
                });
        }
        {
            ImGui.TableNextColumn();
            ImGui.Checkbox("###enabled" + index, ref alarm.Enabled);
        }
        {
            ImGui.TableNextColumn();
            var startTime = alarm.StartTime;

            if (Components.InputTime("start_time", ref startTime))
            {
                alarm.StartTime = startTime;
                Plugin.Config.Save();
            }
        }
        {
            ImGui.TableNextColumn();
            var color = alarm.Color ?? Plugin.Config.FloatingWindow.TextColor;
            var newValue =
                ImGuiComponents.ColorPickerWithPalette(3111, Translator.Tr("AlarmEdit_Color_Tooltip"), color);
            if (color != newValue)
            {
                alarm.Color = newValue;
                Plugin.Config.Save();
            }

            ImGui.SameLine();
            if (ImGuiComponents.IconButton($"{FontAwesomeIcon.Undo.ToIconString()}###clearColor"))
            {
                alarm.Color = null;
                Plugin.Config.Save();
            }
        }
        {
            ImGui.TableNextColumn();
            if (ImGui.Checkbox("###blink", ref alarm.Blink))
            {
                Plugin.Config.Save();
            }
        }
        {
            ImGui.TableNextColumn();
            ImGui.PushItemWidth(50f);
            var duration = alarm.Duration;
            if (ImGui.DragInt("###duration" + index, ref duration, 1, 0, 1000, "%ds"))
            {
                duration = Math.Max(duration, 0);
                alarm.Duration = duration;
                Plugin.Config.Save();
            }

            ImGui.PopItemWidth();
        }
        {
            ImGui.TableNextColumn();
            ImGui.PushItemWidth(80f);
            var choice = alarm.Sfx ?? 0;
            if (ImGui.Combo("###sfx", ref choice, Translator.Tr("AlarmEdit_Sound_None") +
                                                  "\0<se.1>\0<se.2>\0<se.3>\0<se.4>\0<se.5>"
                                                  + "\0<se.6>\0<se.7>\0<se.8>\0<se.9>\0<se.10>\0<se.11>\0<se.12>\0<se.13>"
                                                  + "\0<se.14>\0<se.15>\0<se.16>\0"))
            {
                alarm.Sfx = choice == 0 ? null : choice;
                CombatAlarm.AlarmSfx(alarm);
            }

            ImGui.PopItemWidth();
        }
        {
            ImGui.TableNextColumn();
            if (EditingTexts.Contains(index))
            {
                var type = (int) alarm.TextType;
                ImGui.PushItemWidth(150f);
                if (ImGui.Combo("Type", ref type, Translator.Tr("AlarmEdit_Type_ChatLog") + "\0"
                        + Translator.Tr("AlarmEdit_Type_DalamudNotification") + "\0"
                        + Translator.Tr("AlarmEdit_Type_GameToast") + "\0"))
                {
                    alarm.TextType = (CombatAlarmsConfiguration.TextType) type;
                    Plugin.Config.Save();
                }

                var text = alarm.Text ?? "";
                if (ImGui.InputText(Translator.Tr("AlarmEdit_Text_Text"), ref text, 100))
                {
                    text = text.Trim();
                    alarm.Text = text.Length == 0 ? null : text;
                    Plugin.Config.Save();
                }

                ImGui.PopItemWidth();

                if (ImGui.Button(Translator.Tr("AlarmEdit_Text_Test")))
                {
                    CombatAlarm.AlarmText(alarm);
                }

                ImGui.SameLine();
                if (ImGui.Button(Translator.Tr("AlarmEdit_Text_Clear")))
                {
                    alarm.Text = null;
                    Plugin.Config.Save();
                    EditingTexts.Remove(index);
                }

                ImGui.SameLine();
                if (ImGuiComponents.IconButton($"{FontAwesomeIcon.Check.ToIconString()}###doneEditing"))
                {
                    EditingTexts.Remove(index);
                    Plugin.Config.Save();
                }
            }
            else
            {
                if (alarm.Text == null)
                {
                    ImGui.Text("No text");
                }
                else
                {
                    ImGui.Text("Type: ");
                    ImGui.SameLine();
                    switch (alarm.TextType)
                    {
                        case CombatAlarmsConfiguration.TextType.DalamudNotification:
                            ImGui.Text("Dalamud notification");
                            break;
                        case CombatAlarmsConfiguration.TextType.GameToast:
                            ImGui.Text("Toast");
                            break;
                        case CombatAlarmsConfiguration.TextType.ChatLogMessage:
                            ImGui.Text("Log message");
                            break;
                    }

                    ImGui.Text(alarm.Text);
                }

                ImGui.SameLine();
                if (ImGuiComponents.IconButton($"{FontAwesomeIcon.Comment.ToIconString()}###editNotification"))
                {
                    // coggers!
                    EditingTexts.Add(index);
                }
            }
        }
        ImGui.PopID();
    }
}