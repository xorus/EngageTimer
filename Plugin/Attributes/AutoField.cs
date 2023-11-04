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
using EngageTimer.Ui;

namespace EngageTimer.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class AutoField : Attribute
{
    public string? String { get; set; }
    public string? Id { get; set; }
    public Components.FieldType Mode { get; set; } = Components.FieldType.Auto;
    public float? Step { get; set; }
    public float? StepFast { get; set; }
    public string? Format { get; set; }
    public float? Min { get; set; }
    public float? Max { get; set; }

    public AutoField()
    {
    }

    public AutoField(string str)
    {
        String = str;
    }

    public AutoField(string str, Components.FieldType mode)
    {
        String = str;
        Mode = mode;
    }

    public AutoField(string str, float min, float max)
    {
        String = str;
        Min = min;
        Max = max;
    }

    public AutoField(string str, Components.FieldType mode, float step, float min, float max)
    {
        String = str;
        Mode = mode;
        Step = step;
        Min = min;
        Max = max;
    }

    public AutoField(string str, Components.FieldType mode, float step, float stepFast, string format)
    {
        String = str;
        Mode = mode;
        Step = step;
        StepFast = stepFast;
        Format = format;
    }
}