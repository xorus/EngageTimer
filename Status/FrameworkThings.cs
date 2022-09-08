using System;
using Dalamud.Game;
using EngageTimer.Game;
using EngageTimer.Ui;
using EngageTimer.Web;
using XwContainer;

namespace EngageTimer.Status;

public sealed class FrameworkThings : IDisposable
{
    private readonly CombatStopwatch _combatStopwatch;
    private readonly Container _container;
    private readonly CountdownHook _countdownHook;
    private readonly DtrBarUi _dtrBarUi;
    private readonly WebServer _server;
    private readonly TickingSound _sound;
    private readonly PrePullDetect _prePull;

    public FrameworkThings(Container container)
    {
        _container = container;
        _combatStopwatch = _container.Register<CombatStopwatch>();
        _countdownHook = _container.Register<CountdownHook>();
        _server = _container.RegisterDisposable<WebServer>();
        _dtrBarUi = _container.RegisterDisposable<DtrBarUi>();
        _sound = _container.Register<TickingSound>();
        _prePull = _container.Register<PrePullDetect>();

        _container.Resolve<Framework>().Update += OnUpdate;
    }

    public void Dispose()
    {
        _container.Resolve<Framework>().Update -= OnUpdate;
    }

    private void OnUpdate(Framework framework)
    {
        _server.Update();
        _combatStopwatch.UpdateEncounterTimer();
        _countdownHook.Update();
        _dtrBarUi.Update();
        _sound.Update();
        _prePull.Update();
    }
}