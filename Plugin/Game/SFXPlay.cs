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
using System.Diagnostics;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace EngageTimer.Game;

public class SfxPlay
{
    public const uint FirstSeSfx = 37 - 1;
    public const uint SmallTick = 29;
    public const uint CdTick = 48;

    public SfxPlay()
    {
        /* Force a sound to play on load as a workaround for the CLR taking some time to init the pointy method call,
         * we don't want a freeze midway through a countdown (or midway in combat for alarms)
         * https://discord.com/channels/581875019861328007/653504487352303619/988123102116450335
         * https://i.imgur.com/BrLUr2p.png
         */
        SoundEffect(0); // should be cursor sound
    }

    public void SoundEffect(uint id)
    {
        // var s = new Stopwatch();
        // s.Start();
        UIGlobals.PlaySoundEffect(id, IntPtr.Zero, IntPtr.Zero, 0);
        // s.Stop();
        // Plugin.Logger.Debug("Sound play took " + s.ElapsedMilliseconds + "ms");
    }
}