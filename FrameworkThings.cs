using System;
using Dalamud.Game;
using EngageTimer.UI;
using EngageTimer.Web;

namespace EngageTimer;

public sealed class FrameworkThings : IDisposable
{
    private readonly CombatStopwatch _combatStopwatch;
    private readonly CountdownHook _countdownHook;
    private readonly WebServer _server;
    private readonly DtrBarUi _dtrBarUi;
    private readonly Container _container;
    private readonly TickingSound _sound;

    public FrameworkThings(Container container)
    {
        _container = container;
        _combatStopwatch = _container.Register<CombatStopwatch>();
        _countdownHook = _container.Register<CountdownHook>();
        _server = _container.RegisterDisposable<WebServer>();
        _dtrBarUi = _container.RegisterDisposable<DtrBarUi>();
        _sound = _container.Register<TickingSound>();

        _container.Resolve<Framework>().Update += OnUpdate;
    }

    private void OnUpdate(Framework framework)
    {
        _server.Update();
        _combatStopwatch.UpdateEncounterTimer();
        _countdownHook.Update();
        _dtrBarUi.Update();
        _sound.Update();
    }

    public void Dispose()
    {
        _container.Resolve<Framework>().Update -= OnUpdate;
    }
}