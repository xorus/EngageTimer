using System;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace EngageTimer;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public enum TextAlign
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    public const string DefaultCombatTimePrefix = "【 ";
    public const string DefaultCombatTimeSuffix = "】";
    public static readonly string[] BundledTextures = { "default", "yellow", "wow", "awk", "tall", "misaligned", "pixel", "moire", "mspaint" };

    // Dtr bar
    [NonSerialized] private bool _dtrCombatTimeEnabled;

    // Add any other properties or methods here.
    [NonSerialized] private DalamudPluginInterface _pluginInterface;

    // Counting mode
    public bool CountdownAccurateCountdown { get; set; } = false;
    public bool FloatingWindowAccurateCountdown { get; set; } = true;

    // Countdown
    public bool DisplayCountdown { get; set; } = true;
    public bool HideOriginalCountdown { get; set; } = false;
    public bool AutoHideStopwatch { get; set; } = true;
    public float AutoHideTimeout { get; set; } = 20f;
    public bool EnableTickingSound { get; set; } = false;
    public bool UseAlternativeSound { get; set; } = false;
    public bool EnableCountdownDecimal { get; set; } = false;
    public int CountdownDecimalPrecision { get; set; } = 1;
    public bool EnableCountdownDisplayThreshold { get; set; } = false;
    public int CountdownDisplayThreshold { get; set; } = 5;

    // Countdown style
    public string CountdownTexturePreset { get; set; } = "default";
    public string CountdownTextureDirectory { get; set; } = null;
    public float CountdownScale { get; set; } = 1f;
    public bool CountdownMonospaced { get; set; }
    public float? CountdownCustomNegativeMargin { get; set; } = null;
    public bool CountdownLeadingZero { get; set; }

    // Countdown color
    public int CountdownNumberHue { get; set; }
    public float CountdownNumberSaturation { get; set; }
    public float CountdownNumberLuminance { get; set; }
    public bool CountdownNumberRecolorMode { get; set; }
    public bool CountdownAnimate { get; set; }
    public bool CountdownAnimateScale { get; set; } = true;
    public bool CountdownAnimateOpacity { get; set; } = true;
    public Vector2 CountdownWindowOffset { get; set; } = Vector2.Zero;

    // Floating window
    public bool DisplayFloatingWindow { get; set; } = true;
    public bool FloatingWindowCountdown { get; set; } = false;
    public bool FloatingWindowStopwatch { get; set; } = true;

    public bool FloatingWindowLock { get; set; }

    public int FloatingWindowDecimalCountdownPrecision { get; set; }
    public int FloatingWindowDecimalStopwatchPrecision { get; set; }
    public bool FloatingWindowDisplayStopwatchOnlyInDuty { get; set; } = false;
    public bool FloatingWindowStopwatchAsSeconds { get; set; } = false;
    public bool FloatingWindowCountdownNegativeSign { get; set; } = true;
    public float FloatingWindowScale { get; set; } = 1f;
    public bool FloatingWindowShowPrePulling { get; set; } = false;
    public float FloatingWindowPrePullOffset { get; set; } = .0f;
    public Vector4 FloatingWindowPrePullColor { get; set; } = ImGuiColors.DalamudRed;

    // Stopwatch cosmetics
    public Vector4 FloatingWindowTextColor { get; set; } = new(255, 255, 255, 1);
    public Vector4 FloatingWindowBackgroundColor { get; set; } = new(0, 0, 0, 0);

    public TextAlign StopwatchTextAlign { get; set; } = TextAlign.Left;
    public TextAlign CountdownAlign { get; set; } = TextAlign.Center;
    public int FontSize { get; set; } = 16;

    // WebServer shenanigans
    public bool EnableWebServer { get; set; } = false;
    public int WebServerPort { get; set; } = 8952;

    public bool EnableWebStopwatchTimeout { get; set; } = false;
    public float WebStopwatchTimeout { get; set; } = 0f;

    // Retired properties (kept for some time to allow migrations)
    public bool DisplayStopwatch { get; set; } = true;
    public float StopwatchOpacity { get; set; } = 0f;
    public Vector4 StopwatchColor { get; set; } = new(255, 255, 255, 1);
    public bool StopwatchLock { get; set; } = false;
    public bool StopwatchTenths { get; set; } = false;
    public int StopwatchDecimalPrecision { get; set; } = 1;

    public bool MigrateCountdownOffsetToPercent { get; set; }

    public bool DtrCombatTimeEnabled
    {
        get => _dtrCombatTimeEnabled;
        set
        {
            _dtrCombatTimeEnabled = value;
            DtrBarCombatTimerEnableChange?.Invoke(this, EventArgs.Empty);
        }
    }

    public string DtrCombatTimePrefix { get; set; } = DefaultCombatTimePrefix;
    public string DtrCombatTimeSuffix { get; set; } = DefaultCombatTimeSuffix;
    public int DtrCombatTimeDecimalPrecision { get; set; } = 0;
    public bool DtrCombatTimeAlwaysDisableOutsideDuty { get; set; }
    public bool DtrCombatTimeEnableHideAfter { get; set; } = false;
    public float DtrCombatTimeHideAfter { get; set; } = 20f;
    public int Version { get; set; } = 2;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
    }

    public void Save()
    {
        _pluginInterface.SavePluginConfig(this);
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
    public event EventHandler DtrBarCombatTimerEnableChange;

    public void Migrate()
    {
        if (Version == 0)
        {
            PluginLog.Information($"Migrating plugin configuration from version {Version}");
            DisplayFloatingWindow = DisplayStopwatch;
            FloatingWindowBackgroundColor = new Vector4(0, 0, 0, 255 * StopwatchOpacity);
            FloatingWindowTextColor = StopwatchColor;
            FloatingWindowLock = StopwatchLock;
            FloatingWindowDecimalStopwatchPrecision = StopwatchTenths ? StopwatchDecimalPrecision : 0;
            FloatingWindowDecimalCountdownPrecision = StopwatchTenths ? StopwatchDecimalPrecision : 0;
            FloatingWindowAccurateCountdown = true;
            Version = 1;
            Save();
        }

        if (Version == 1)
        {
            PluginLog.Information($"Migrating plugin configuration from version {Version}");
            if (CountdownWindowOffset.X != 0 || CountdownWindowOffset.Y != 0)
                MigrateCountdownOffsetToPercent = true;
            Version = 2;
            Save();
        }
    }
}