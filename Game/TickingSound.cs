using System;
using EngageTimer.Status;
using XwContainer;

namespace EngageTimer.Game;

public class TickingSound
{
    private readonly Configuration _configuration;
    private readonly State _state;
    private readonly SfxPlay _sfx;

    private int? _lastNumberPlayed;

    // This is a workaround for CLR taking some time to init the pointy method call. 
    private bool _soundLoaded;

    public TickingSound(Container container)
    {
        _configuration = container.Resolve<Configuration>();
        _state = container.Resolve<State>();
        _sfx = new SfxPlay(container);
    }

    public void Update()
    {
        // if (!_configuration.DisplayCountdown) return;
        if (!_configuration.EnableTickingSound || _state.Mocked) return;
        if (!_soundLoaded)
        {
            _sfx.SoundEffect(0); // should be cursor sound
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
        _sfx.SoundEffect(_configuration.UseAlternativeSound ? SfxPlay.SmallTick : SfxPlay.CdTick);
    }
}