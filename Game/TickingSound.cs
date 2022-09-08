using System;
using EngageTimer.Status;
using XwContainer;

namespace EngageTimer.Game;

public class TickingSound
{
    private readonly Configuration _configuration;
    private readonly string _path;
    private readonly State _state;

    private int? _lastNumberPlayed;

    // This is a workaround for CLR taking some time to init the pointy method call. 
    private bool _soundLoaded;

    public TickingSound(Container container)
    {
        _configuration = container.Resolve<Configuration>();
        _state = container.Resolve<State>();
        _path = container.Resolve<Plugin>().PluginPath;
    }

    public void Update()
    {
        // if (!_configuration.DisplayCountdown) return;
        if (!_configuration.EnableTickingSound || _state.Mocked) return;
        if (!_soundLoaded)
        {
            SfxPlay.SoundEffect(0); // should be cursor sound
            _soundLoaded = true;
            return;
        }

        if (_state.CountingDown && _state.CountDownValue > 5)
            TickSound((int)Math.Ceiling(_state.CountDownValue));
    }

    private void TickSound(int n)
    {
        if (!_configuration.EnableTickingSound || _lastNumberPlayed == n)
            return;
        _lastNumberPlayed = n;
        SfxPlay.SoundEffect(_configuration.UseAlternativeSound ? SfxPlay.SmallTick : SfxPlay.CdTick);
    }
}