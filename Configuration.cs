using System;
using Dalamud.Configuration;
using Dalamud.Plugin;
using System.Numerics;
using Dalamud.Logging;

namespace EngageTimer
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        // Counting mode
        public bool CountdownAccurateCountdown { get; set; } = false;
        public bool FloatingWindowAccurateCountdown { get; set; } = true;

        // Countdown
        public bool DisplayCountdown { get; set; } = true;
        public bool HideOriginalCountdown { get; set; } = false;
        public bool AutoHideStopwatch { get; set; } = true;
        public float AutoHideTimeout { get; set; } = 20f;
        public bool EnableTickingSound { get; set; } = false;
        public float TickingSoundVolume { get; set; } = 0.05f;
        public bool EnableCountdownDecimal { get; set; } = false;
        public int CountdownDecimalPrecision { get; set; } = 1;

        public string CountdownTexturePreset { get; set; } = "default";
        public string CountdownTextureDirectory { get; set; } = null;
        public static readonly string[] BundledTextures = { "default", "yellow", "wow" };

        // Floating window
        public bool DisplayFloatingWindow { get; set; } = true;
        public bool FloatingWindowCountdown { get; set; } = false;
        public bool FloatingWindowStopwatch { get; set; } = true;

        public bool FloatingWindowLock { get; set; } = false;

        public int FloatingWindowDecimalCountdownPrecision { get; set; } = 0;
        public int FloatingWindowDecimalStopwatchPrecision { get; set; } = 0;
        public bool FloatingWindowDisplayStopwatchOnlyInDuty { get; set; } = false;

        // Stopwatch cosmetics

        public Vector4 FloatingWindowTextColor { get; set; } = new Vector4(255, 255, 255, 1);
        public Vector4 FloatingWindowBackgroundColor { get; set; } = new Vector4(0, 0, 0, 0);

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

        // Retired properties (kept for some time to allow migrations)
        public bool DisplayStopwatch { get; set; } = true;
        public float StopwatchOpacity { get; set; } = 0f;
        public Vector4 StopwatchColor { get; set; } = new Vector4(255, 255, 255, 1);
        public bool StopwatchLock { get; set; } = false;
        public bool StopwatchTenths { get; set; } = false;
        public int StopwatchDecimalPrecision { get; set; } = 1;

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

        public void Migrate()
        {
            if (Version == 0)
            {
                PluginLog.Information("Mother. I require migration. Migrating plugin configuration from version " +
                                      Version);

                DisplayFloatingWindow = DisplayStopwatch;
                FloatingWindowBackgroundColor = new Vector4(0, 0, 0, 255 * StopwatchOpacity);
                FloatingWindowTextColor = StopwatchColor;
                FloatingWindowLock = StopwatchLock;
                FloatingWindowDecimalStopwatchPrecision = StopwatchTenths ? StopwatchDecimalPrecision : 0;
                FloatingWindowDecimalCountdownPrecision = StopwatchTenths ? StopwatchDecimalPrecision : 0;
                FloatingWindowAccurateCountdown = true;
                Version = 1;

                this.Save();
            }
        }
    }
}