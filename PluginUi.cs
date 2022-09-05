using System;
using Dalamud.Interface;
using EngageTimer.UI;
using XwContainer;

namespace EngageTimer;

public sealed class PluginUi : IDisposable
{
    private readonly Container _container;
    private readonly CountDown _countDown;
    private readonly Settings _settings;
    private readonly FloatingWindow _floatingWindow;

    public PluginUi(Container container)
    {
        _container = container;
        var numbers = new NumberTextures(container);
        container.Register(numbers);

        _countDown = container.Register<CountDown>();
        _floatingWindow = container.RegisterDisposable<FloatingWindow>();
        _settings = container.Register<Settings>();

        container.Resolve<UiBuilder>().Draw += Draw;
        container.Resolve<UiBuilder>().OpenConfigUi += OpenSettings;
    }

    public void Dispose()
    {
        _container.Resolve<UiBuilder>().Draw -= Draw;
        _container.Resolve<UiBuilder>().OpenConfigUi -= OpenSettings;
    }

    private void Draw()
    {
        _settings.Draw();
        _countDown.Draw();
        _floatingWindow.Draw();
    }

    public void OpenSettings()
    {
        _settings.Visible = true;
    }
}