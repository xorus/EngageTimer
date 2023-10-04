using System;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using XwContainer;

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

    public GameSound(IGameInteropProvider gameInterop)
    {
        gameInterop.InitializeFromAttributes(this);
    }
}

public class SfxPlay
{
    public const uint SmallTick = 29;
    public const uint CdTick = 48;
    private readonly GameSound _gameSound;

    public SfxPlay(Container container)
    {
        _gameSound = new GameSound(container.Resolve<IGameInteropProvider>());
    }

    public void SoundEffect(uint id)
    {
        // var s = new Stopwatch();
        // s.Start();
        unsafe
        {
            _gameSound.PlaySoundEffect(id, IntPtr.Zero, IntPtr.Zero, 0);
        }
        // s.Stop();
        // PluginLog.Debug("Sound play took " + s.ElapsedMilliseconds + "ms");
    }
}