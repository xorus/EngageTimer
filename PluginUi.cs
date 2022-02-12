using System;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Plugin;
using EngageTimer.UI;

namespace EngageTimer
{
    public class PluginUi : IDisposable
    {
        private readonly CountDown _countDown;
        private readonly Settings _settings;
        private readonly FloatingWindow _stopwatch;
        private readonly DtrBarUi _dtrBarUi;

        public PluginUi(DalamudPluginInterface pluginInterface,
            Configuration configuration,
            GameGui gui,
            string pluginPath,
            State state,
            DtrBar dtrBar
        )
        {
            var numbers = new NumberTextures(configuration, pluginInterface.UiBuilder, pluginPath);
            numbers.Load();
            _countDown = new CountDown(configuration, state, gui, numbers, pluginPath);
            _stopwatch = new FloatingWindow(configuration, state, pluginInterface);
            _settings = new Settings(configuration, state, pluginInterface.UiBuilder, numbers);
            _dtrBarUi = new DtrBarUi(configuration, state, dtrBar);
        }

        public void Draw()
        {
            _settings.Draw();
            _countDown.Draw();
            _stopwatch.Draw();
            _dtrBarUi.Update();
        }

        public void OpenSettings()
        {
            _settings.Visible = true;
        }

        public void Dispose()
        {
            _stopwatch?.Dispose();
            _dtrBarUi?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}