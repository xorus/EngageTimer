using System;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Plugin;
using EngageTimer.UI;

namespace EngageTimer;

public class PluginUi : IDisposable
{
    private readonly CountDown _countDown;
    private readonly DtrBarUi _dtrBarUi;
    private readonly Settings _settings;
    private readonly FloatingWindow _stopwatch;

    public PluginUi(Container container)
    {
        var numbers = new NumberTextures(container);
        container.Register(numbers);

        _countDown = new CountDown(container);
        _stopwatch = new FloatingWindow(container);
        _dtrBarUi = new DtrBarUi(container);
        _settings = new Settings(container);
        
        container.Register(_countDown);
        container.RegisterDisposable(_stopwatch);
        container.RegisterDisposable(_dtrBarUi);
        container.Register(_settings);
    }

    public void Dispose()
    {
        _stopwatch?.Dispose();
        _dtrBarUi?.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Draw()
    {
        _settings.Draw();
        _countDown.Draw();
        _stopwatch.Draw();
        
        // fixme: move to framework update
        _dtrBarUi.Update();
    }

    public void OpenSettings()
    {
        _settings.Visible = true;
    }
}