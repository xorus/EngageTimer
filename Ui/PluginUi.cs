using System;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using XwContainer;

namespace EngageTimer.Ui;

public sealed class PluginUi : IDisposable
{
    private readonly Container _container;
    private readonly CountDown _countDown;
    private readonly FloatingWindow _floatingWindow;
    private readonly Settings _settings;
    private readonly WindowSystem _windowSystem;

    public PluginUi(Container container)
    {
        _container = container;
        var numbers = new NumberTextures(container);
        container.Register(numbers);

        _windowSystem = new WindowSystem(container.Resolve<Plugin>().Name);
        _countDown = container.Register<CountDown>();
        _floatingWindow = container.RegisterDisposable<FloatingWindow>();
        _settings = container.Register<Settings>();

        _windowSystem.AddWindow(_settings);

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
        _windowSystem.Draw();
        _countDown.Draw();
        _floatingWindow.Draw();
    }

    public void OpenSettings()
    {
        _settings.IsOpen = true;
    }
}