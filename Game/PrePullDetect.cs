using Dalamud.Logging;
using EngageTimer.Status;
using ImGuiNET;
using XwContainer;

namespace EngageTimer.Game;

/**
 * Thanks Aireil for this feature
 */
public class PrePullDetect
{
    private readonly State _state;
    private readonly Configuration _configuration;

    public PrePullDetect(Container container)
    {
        _configuration = container.Resolve<Configuration>();
        _state = container.Resolve<State>();
    }

    public unsafe void Update()
    {
        if (_state.PrePulling) _state.PrePulling = false;
        if (!_configuration.FloatingWindowCountdown || !_configuration.FloatingWindowShowPrePulling) return;
        var actionManager = (TrimmedDownActionManager*)FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
        if (!_state.CountingDown || !actionManager->isCasting) return;
        _state.PrePulling =
            (actionManager->castTime - actionManager->elapsedCastTime + _configuration.FloatingWindowPrePullOffset) <
            _state.CountDownValue;
    }
}