using Dalamud.Plugin;
using EngageTimer.UI;

namespace EngageTimer
{
    public class PluginUi
    {
        private readonly CountDown _countDown;
        private readonly Settings _settings;
        private readonly StopWatch _stopwatch;

        public PluginUi(DalamudPluginInterface pluginInterface, Configuration configuration, string dataPath,
            State state)
        {
            _countDown = new CountDown(configuration, state);
            _stopwatch = new StopWatch(configuration, state);
            _settings = new Settings(configuration);

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
    }
}