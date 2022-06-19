using System;
using System.IO;
using System.Threading;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using NAudio.Wave;

namespace EngageTimer.UI;

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
        SignatureHelper.Initialise(this);
    }
}

public static class SfxPlay
{
    public const uint SmallTick = 29;
    public const uint CdTick = 48;
    private static readonly GameSound GameSound = new();

    public static void SoundEffect(uint id)
    {
        // var s = new Stopwatch();
        // s.Start();
        unsafe
        {
            GameSound.PlaySoundEffect(id, IntPtr.Zero, IntPtr.Zero, 0);
        }
        // s.Stop();
        // PluginLog.Debug("Sound play took " + s.ElapsedMilliseconds + "ms");
    }

    /**
         * https://git.sr.ht/~jkcclemens/PeepingTom
         */
    public static void Legacy(string path, float volume)
    {
        new Thread(() =>
        {
            WaveStream reader;
            try
            {
                reader = new WaveFileReader(Path.Combine(path, "Data", "tick.wav"));
            }
            catch (Exception e)
            {
                PluginLog.Log($"Could not play sound file: {e.Message}");
                return;
            }

            using WaveChannel32 channel = new(reader)
            {
                Volume = volume,
                PadWithZeroes = false
            };

            using (reader)
            {
                using var output = new WaveOutEvent
                {
                    DeviceNumber = -1
                };
                output.Init(channel);
                output.Play();

                while (output.PlaybackState == PlaybackState.Playing) Thread.Sleep(500);
            }
        }).Start();
    }
}