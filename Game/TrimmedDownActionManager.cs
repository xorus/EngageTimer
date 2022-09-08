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