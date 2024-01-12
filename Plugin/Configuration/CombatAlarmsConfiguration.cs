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

using System.Collections.Generic;
using System.Numerics;

namespace EngageTimer.Configuration;

public class CombatAlarmsConfiguration
{
    public enum TextType
    {
        ChatLogMessage = 0,
        DalamudNotification = 1,
        GameToast = 2
    }

    public class Alarm
    {
        public bool Enabled = true;
        public int StartTime = 270;
        public int Duration = 10;
        public string? Text;
        public Vector4? Color = new Vector4(.8f, .24f, .24f, 1);
        public int? Sfx = 9;
        public TextType TextType = TextType.DalamudNotification;
        public bool Blink = false;
    }

    public readonly List<Alarm> Alarms = new()
    {
        // new Alarm()
        // {
        //     StartTime = 5,
        //     Duration = 30,
        //     TextType = TextType.DalamudNotification,
        //     Color = new Vector4(255, 0, 0, 1),
        //     Text = "Potion Window!",
        //     Sfx = 16
        // }
    };
}