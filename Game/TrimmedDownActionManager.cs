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

using System.Runtime.InteropServices;

namespace EngageTimer.Game;

/**
 * https://github.com/UnknownX7/NoClippy/blob/master/Structures/ActionManager.cs
 */
[StructLayout(LayoutKind.Explicit)]
public struct TrimmedDownActionManager
{
    [FieldOffset(0x28)] public readonly bool isCasting;
    [FieldOffset(0x30)] public readonly float elapsedCastTime;
    [FieldOffset(0x34)] public readonly float castTime;
}