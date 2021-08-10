using System;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json;
using System.Numerics;

namespace EngageTimer
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; }
        public bool DisplayCountdown { get; set; } = true;
        public bool DisplayStopwatch { get; set; } = true;
        public bool HideOriginalCountdown { get; set; } = false;
        public bool AutoHideStopwatch { get; set; } = true;
        public float AutoHideTimeout { get; set; } = 20f;
        public bool EnableTickingSound { get; set; } = false;
        public float TickingSoundVolume { get; set; } = 0.05f;
        public bool StopwatchCountdown { get; set; } = false;
        public bool EnableCountdownDecimal { get; set; } = false;
        public int CountdownDecimalPrecision { get; set; } = 1;
        public bool StopwatchTenths { get; set; } = false;
        public int StopwatchDecimalPrecision { get; set; } = 1;

        // Stopwatch cosmetics
        public bool StopwatchLock { get; set; } = false;
        public float StopwatchOpacity { get; set; } = 0f;
        public Vector4 StopwatchColor { get; set; } = new Vector4(255, 255, 255, 1);

        public enum TextAlign
        {
            Left = 0,
            Center = 1,
            Right = 2
        };

        public TextAlign StopwatchTextAlign { get; set; } = TextAlign.Left;
        public int FontSize { get; set; } = 16;

        // WebServer shenanigans
        public bool EnableWebServer { get; set; } = false;
        public int WebServerPort { get; set; } = 8952;

        public bool EnableWebStopwatchTimeout { get; set; } = false;
        public float WebStopwatchTimeout { get; set; } = 0f;

        // Add any other properties or methods here.
        [NonSerialized] private DalamudPluginInterface _pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this._pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this._pluginInterface.SavePluginConfig(this);
            OnSave?.Invoke(this, EventArgs.Empty);
        }

        public object GetWebConfig()
        {
            return new
            {
                EnableWebStopwatchTimeout, WebStopwatchTimeout
            };
        }

        public event EventHandler OnSave;
    }
}