using System;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using EngageTimer.UI;

namespace EngageTimer
{
    public class PluginUi : IDisposable
    {
        private readonly CountDown _countDown;
        private readonly Settings _settings;
        private readonly StopWatch _stopwatch;

        public PluginUi(DalamudPluginInterface pluginInterface,
            Configuration configuration,
            GameGui gui,
            string dataPath,
            State state
            )
        {
            _countDown = new CountDown(configuration, state, gui);
            _stopwatch = new StopWatch(configuration, state, pluginInterface);
            _settings = new Settings(configuration, pluginInterface.UiBuilder);

            _countDown.Load(pluginInterface, dataPath);
        }

        public void Draw()
        {
            _settings.Draw();
            _countDown.Draw();
            _stopwatch.Draw();
        }

        public void OpenSettings()
        {
            _settings.Visible = true;
        }

        public void Dispose()
        {
            _stopwatch?.Dispose();
        }
    }
}