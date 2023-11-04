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
using System.Numerics;
using System.Reflection;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using EngageTimer.Attributes;
using ImGuiNET;

namespace EngageTimer.Ui;

public static class Components
{
    public static void Checkbox(bool original, string title, ApplyCallback<bool> save)
    {
        var state = original;
        if (!ImGui.Checkbox(title, ref state)) return;
        save(state);
        Plugin.Config.DebouncedSave();
    }

    public static void InputFloat(
        string label,
        float original,
        ApplyCallback<float> apply,
        float? step = null,
        float? min = null,
        float? max = null,
        float width = 100f
    )
    {
        ImGui.PushItemWidth(width);

        var value = original;
        if (ImGui.InputFloat(label, ref value, step ?? 1f))
        {
            if (value < min) value = (float)min;
            if (value > max) value = (float)max;
            apply(value);
            Plugin.Config.DebouncedSave();
        }

        ImGui.PopItemWidth();
    }

    public static void IconButton(FontAwesomeIcon icon, string id, ApplyCallback save)
    {
        if (!ImGuiComponents.IconButton($"{icon.ToIconString()}###{id}")) return;
        save();
        Plugin.Config.DebouncedSave();
    }

    public static void Text(string label, bool sameLine = false)
    {
        if (sameLine) ImGui.SameLine();
        ImGui.Text(Translator.Tr("Settings_FWTab_TextColor"));
    }

    public static void ResettableDraggable(string id, string label, int original, int defaultValue,
        int min, int max, ApplyCallback<int> apply)
    {
        IconButton(FontAwesomeIcon.Undo, $"reset_{id}", () =>
        {
            apply(defaultValue);
            Plugin.Config.DebouncedSave();
        });
        ImGui.SameLine();
        var value = original;
        if (!ImGui.DragInt(label, ref value, 1)) return;

        // loop around min/max
        if (value > max) value = min;
        if (value < min) value = max;
        apply(value);
        Plugin.Config.DebouncedSave();
    }

    public static void ResettableSlider(string id, string label, float original, float defaultValue,
        float min, float max, ApplyCallback<float> apply)
    {
        IconButton(FontAwesomeIcon.Undo, $"reset_{id}", () =>
        {
            apply(defaultValue);
            Plugin.Config.DebouncedSave();
        });
        ImGui.SameLine();
        var value = original;
        if (!ImGui.SliderFloat(label, ref value, -1f, 1f)) return;
        value = Math.Clamp(value, min, max);
        apply(value);
        Plugin.Config.DebouncedSave();
    }

    public delegate void ApplyCallback();

    public delegate void ApplyCallback<in T>(T value);

    public static void AutoField<T>(T instance, string propertyName, ApplyCallback? customApply = null,
        bool sameLine = false)
        where T : class
    {
        var prop = instance.GetType().GetProperty(propertyName);
        if (prop == null)
        {
            ImGui.Text("!!property is null!!");
            return;
        }

        var attr = (AutoField?)prop.GetCustomAttribute(typeof(AutoField));
        if (attr == null)
        {
            ImGui.Text($"!!cannot find attribute on {prop}!!");
            return;
        }

        var minMaxAttr = (MinMax?)prop.GetCustomAttribute(typeof(MinMax));
        var min = minMaxAttr?.Min ?? attr.Min;
        var max = minMaxAttr?.Max ?? attr.Max;

        var itemWidth = ((ItemWidth?)prop.GetCustomAttribute(typeof(ItemWidth)))?.Width;

        var label = "!!no string!!";
        if (attr.Id != null) label = $"###{attr.Id}";
        else if (attr.String != null) label = Translator.TrId(attr.String);

        var foundType = FindType(attr, prop.PropertyType);

        if (sameLine == true) ImGui.SameLine();
        if (itemWidth != null) ImGui.PushItemWidth((float)itemWidth);
        ShowField(instance, foundType, prop, label, min, max, attr, customApply);
        if (itemWidth != null) ImGui.PopItemWidth();

        var helpAttr = (Help?)prop.GetCustomAttribute(typeof(Help));
        if (helpAttr != null)
            ImGuiComponents.HelpMarker(Translator.Tr(helpAttr.Str ?? (attr.String ?? "???") + "_Help"));
    }

    private static void ShowField<T>(T instance, FieldType? foundType, PropertyInfo prop, string label, float? min,
        float? max, AutoField attr, ApplyCallback? customApply = null) where T : class
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (foundType == FieldType.Checkbox)
        {
            var value = (bool)(prop.GetValue(instance) ?? false);
            if (!ImGui.Checkbox(label, ref value)) return;
            prop.SetValue(instance, value);
            Plugin.Config.DebouncedSave();
        }
        else if (foundType == FieldType.InputInt)
        {
            var value = (int)(prop.GetValue(instance) ?? 0);
            if (!ImGui.InputInt(label, ref value)) return;
            if (min != null) value = Math.Max((int)min, value);
            if (max != null) value = Math.Min((int)max, value);
            prop.SetValue(instance, value);
            Plugin.Config.DebouncedSave();
            customApply?.Invoke();
        }
        else if (foundType == FieldType.InputFloat)
        {
            var value = (float)(prop.GetValue(instance) ?? 0f);

            if (attr is { Step: not null, StepFast: not null, Format: not null })
            {
                if (!ImGui.InputFloat(label, ref value, (float)attr.Step, (float)attr.StepFast, attr.Format)) return;
            }
            else
            {
                if (!ImGui.InputFloat(label, ref value)) return;
            }

            if (min != null) value = Math.Max((float)min, value);
            if (max != null) value = Math.Min((float)max, value);

            prop.SetValue(instance, value);
            Plugin.Config.DebouncedSave();
            customApply?.Invoke();
        }
        else if (foundType == FieldType.DragFloat)
        {
            var value = (float)(prop.GetValue(instance) ?? 0f);
            if (attr is { Step: not null })
            {
                if (!ImGui.DragFloat(label, ref value, (float)attr.Step)) return;
            }
            else
            {
                if (!ImGui.DragFloat(label, ref value)) return;
            }

            if (min != null) value = Math.Max((float)min, value);
            if (max != null) value = Math.Min((float)max, value);

            prop.SetValue(instance, value);
            Plugin.Config.DebouncedSave();
            customApply?.Invoke();
        }
        else if (foundType == FieldType.ColorPicker)
        {
            var value = (Vector4)(prop.GetValue(instance) ?? new Vector4());
            var newValue = ImGuiComponents.ColorPickerWithPalette(10, label, value);
            if (value == newValue)
            {
                prop.SetValue(instance, value);
                Plugin.Config.DebouncedSave();
                customApply?.Invoke();
            }

            ImGui.SameLine();
            ImGui.Text(Translator.Tr(attr.String ?? ""));
        }
        else if (foundType == FieldType.InputText)
        {
            var value = (string)(prop.GetValue(instance) ?? "");
            if (!ImGui.InputText(label, ref value, (uint)(max ?? 100))) return;
            prop.SetValue(instance, value);
            Plugin.Config.DebouncedSave();
            customApply?.Invoke();
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            ImGui.Text($"!!unknown field type {prop.PropertyType} [{foundType}]!!");
            ImGui.PopStyleColor();
        }
    }

    private static FieldType? FindType(AutoField attr, Type propertyType)
    {
        if (attr.Mode != FieldType.Auto) return attr.Mode;
        if (propertyType.IsEquivalentTo(typeof(bool))) return FieldType.Checkbox;
        if (propertyType.IsEquivalentTo(typeof(int))) return FieldType.InputInt;
        if (propertyType.IsEquivalentTo(typeof(float))) return FieldType.DragFloat;
        if (propertyType.IsEquivalentTo(typeof(Vector4))) return FieldType.ColorPicker;
        if (propertyType.IsEquivalentTo(typeof(string))) return FieldType.InputText;
        return null;
    }

    public enum FieldType
    {
        Auto,
        Checkbox,
        InputInt,
        InputFloat,
        DragFloat,
        ColorPicker,
        InputText
    }
}