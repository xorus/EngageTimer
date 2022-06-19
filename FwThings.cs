using System;
using Dalamud.Game;
using EngageTimer.UI;
using EngageTimer.Web;

namespace EngageTimer;

public sealed class FwThings : IDisposable
{
    private readonly CombatStopwatch _combatStopwatch;
    private readonly CountdownHook _countdownHook;
    private readonly WebServer _server;
    private readonly DtrBarUi _dtrBarUi;
    private readonly Container _container;

    public FwThings(Container container)
    {
        _container = container;
        _combatStopwatch = _container.Register<CombatStopwatch>();
        _countdownHook = _container.Register<CountdownHook>();
        _server = _container.RegisterDisposable<WebServer>();
        _dtrBarUi = _container.RegisterDisposable<DtrBarUi>();
        
        _container.Resolve<Framework>().Update += OnUpdate;
    }

    private void OnUpdate(Framework framework)
    {
        _server.Update();
        _combatStopwatch.UpdateEncounterTimer();
        _countdownHook.Update();
        _dtrBarUi.Update();
    }

    public void Dispose()
    {
        _container.Resolve<Framework>().Update -= OnUpdate;
    }
}