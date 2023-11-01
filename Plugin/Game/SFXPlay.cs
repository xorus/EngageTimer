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
using Dalamud.Utility.Signatures;

namespace EngageTimer.Game;

/**
 * thanks aers
 * sig taken from https://github.com/philpax/plogonscript/blob/main/PlogonScript/Script/Bindings/Sound.cs
 * ---
 * https://discord.com/channels/581875019861328007/653504487352303619/988123102116450335
 */
internal unsafe class GameSound
{
    [Signature("E8 ?? ?? ?? ?? 4D 39 BE ?? ?? ?? ??")]
    public readonly delegate* unmanaged<uint, IntPtr, IntPtr, byte, void> PlaySoundEffect = null;

    public GameSound()
    {
        Plugin.GameInterop.InitializeFromAttributes(this);
    }
}

public class SfxPlay
{
    public const uint SmallTick = 29;
    public const uint CdTick = 48;
    private readonly GameSound _gameSound = new();

    public void SoundEffect(uint id)
    {
        // var s = new Stopwatch();
        // s.Start();
        unsafe
        {
            _gameSound.PlaySoundEffect(id, IntPtr.Zero, IntPtr.Zero, 0);
        }
        // s.Stop();
        // PluginIoC.Logger.Debug("Sound play took " + s.ElapsedMilliseconds + "ms");
    }
}