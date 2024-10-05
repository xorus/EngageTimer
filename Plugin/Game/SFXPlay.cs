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
 * https://github.com/0ceal0t/JobBars/blob/2c9bef8dd4f0bf9ebc91c07e03da6c841ac2bd35/JobBars/Helper/UiHelper.GameFunctions.cs#L61
 * ---
 * https://discord.com/channels/581875019861328007/653504487352303619/988123102116450335
 */
internal unsafe class GameSound
{
    [Signature("E8 ?? ?? ?? ?? 48 63 45 80")]
    public readonly delegate* unmanaged<uint, IntPtr, IntPtr, byte, void> PlaySoundEffect = null;

    public GameSound()
    {
        Plugin.GameInterop.InitializeFromAttributes(this);
    }
}

public class SfxPlay
{
    public const uint FirstSeSfx = 37 - 1;
    public const uint SmallTick = 29;
    public const uint CdTick = 48;
    private readonly GameSound _gameSound = new();

    public SfxPlay()
    {
        /* Force a sound to play on load as a workaround for the CLR taking some time to init the pointy method call,
         * we don't want a freeze midway through a countdown (or midway in combat for alarms)
         * https://discord.com/channels/581875019861328007/653504487352303619/988123102116450335
         * https://i.imgur.com/BrLUr2p.png
         */
        SoundEffect(0); // should be cursor sound
    }

    public unsafe void SoundEffect(uint id)
    {
        // var s = new Stopwatch();
        // s.Start();
        _gameSound.PlaySoundEffect(id, IntPtr.Zero, IntPtr.Zero, 0);
        // s.Stop();
        // Plugin.Logger.Debug("Sound play took " + s.ElapsedMilliseconds + "ms");
    }
}