// This file is part of EngageTimer
// Copyright (C) 2023 Xorus <xorus@posteo.net>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Timers;
using Dalamud.Configuration;
using Dalamud.Plugin;
using EngageTimer.Configuration.Legacy;

namespace EngageTimer.Configuration;

[Serializable]
public class ConfigurationFile : IPluginConfiguration
{
    public enum TextAlign
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    // Add any other properties or methods here.
    [NonSerialized] private DalamudPluginInterface _pluginInterface = Plugin.PluginInterface;
    public CountdownConfiguration Countdown = new();

    public DtrConfiguration Dtr = new();
    public FloatingWindowConfiguration FloatingWindow = new();
    public WebServerConfiguration WebServer = new();
    public CombatAlarmsConfiguration CombatAlarms = new();

    public int Version { get; set; } = 3;

    [NonSerialized] private Timer _saveTimer;

    public ConfigurationFile()
    {
        _saveTimer = new Timer(250);
        _saveTimer.AutoReset = false;
        _saveTimer.Elapsed += SaveTimerElapsed;
    }

    public void Save()
    {
        Plugin.Logger.Debug("Saving configuration");
        _pluginInterface.SavePluginConfig(this);
        OnSave?.Invoke(this, EventArgs.Empty);
    }


    private void SaveTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        Save();
    }

    public void DebouncedSave()
    {
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    public object GetWebConfig()
    {
        return new
        {
            EnableWebStopwatchTimeout = WebServer.EnableStopwatchTimeout,
            WebStopwatchTimeout = WebServer.StopwatchTimeout
        };
    }

    public event EventHandler? OnSave;

    public ConfigurationFile Import(OldConfig old)
    {
        Countdown.AccurateMode = old.CountdownAccurateCountdown;
        FloatingWindow.AccurateMode = old.FloatingWindowAccurateCountdown;
        Countdown.Display = old.DisplayCountdown;
        Countdown.HideOriginalAddon = old.HideOriginalCountdown;
        FloatingWindow.AutoHide = old.AutoHideStopwatch;
        FloatingWindow.AutoHideTimeout = old.AutoHideTimeout;
        Countdown.EnableTickingSound = old.EnableTickingSound;
        Countdown.UseAlternativeSound = old.UseAlternativeSound;
        Countdown.StartTickingFrom = old.StartTickingFrom;
        Countdown.EnableDecimals = old.EnableCountdownDecimal;
        Countdown.DecimalPrecision = old.CountdownDecimalPrecision;
        Countdown.EnableDisplayThreshold = old.EnableCountdownDisplayThreshold;
        Countdown.DisplayThreshold = old.CountdownDisplayThreshold;
        Countdown.TexturePreset = old.CountdownTexturePreset;
        Countdown.TextureDirectory = old.CountdownTextureDirectory;
        Countdown.Scale = old.CountdownScale;
        Countdown.Monospaced = old.CountdownMonospaced;
        Countdown.CustomNegativeMargin = old.CountdownCustomNegativeMargin;
        Countdown.LeadingZero = old.CountdownLeadingZero;
        Countdown.Hue = old.CountdownNumberHue;
        Countdown.Saturation = old.CountdownNumberSaturation;
        Countdown.Luminance = old.CountdownNumberLuminance;
        Countdown.NumberRecolorMode = old.CountdownNumberRecolorMode;
        Countdown.Animate = old.CountdownAnimate;
        Countdown.AnimateScale = old.CountdownAnimateScale;
        Countdown.AnimateOpacity = old.CountdownAnimateOpacity;
        Countdown.WindowOffset = old.CountdownWindowOffset;
        FloatingWindow.Display = old.DisplayFloatingWindow;
        FloatingWindow.EnableCountdown = old.FloatingWindowCountdown;
        FloatingWindow.EnableStopwatch = old.FloatingWindowStopwatch;
        FloatingWindow.Lock = old.FloatingWindowLock;
        FloatingWindow.DecimalCountdownPrecision = old.FloatingWindowDecimalCountdownPrecision;
        FloatingWindow.DecimalStopwatchPrecision = old.FloatingWindowDecimalStopwatchPrecision;
        FloatingWindow.StopwatchOnlyInDuty = old.FloatingWindowDisplayStopwatchOnlyInDuty;
        FloatingWindow.StopwatchAsSeconds = old.FloatingWindowStopwatchAsSeconds;
        FloatingWindow.CountdownNegativeSign = old.FloatingWindowCountdownNegativeSign;
        FloatingWindow.Scale = old.FloatingWindowScale;
        FloatingWindow.ShowPrePulling = old.FloatingWindowShowPrePulling;
        FloatingWindow.PrePullOffset = old.FloatingWindowPrePullOffset;
        FloatingWindow.PrePullColor = old.FloatingWindowPrePullColor;
        FloatingWindow.TextColor = old.FloatingWindowTextColor;
        FloatingWindow.BackgroundColor = old.FloatingWindowBackgroundColor;
        FloatingWindow.Align = (TextAlign)old.StopwatchTextAlign;
        Countdown.Align = (TextAlign)old.CountdownAlign;
        FloatingWindow.FontSize = old.FontSize;
        WebServer.Enable = old.EnableWebServer;
        WebServer.Port = old.WebServerPort;
        WebServer.EnableStopwatchTimeout = old.EnableWebStopwatchTimeout;
        WebServer.StopwatchTimeout = old.WebStopwatchTimeout;
        Dtr.CombatTimeEnabled = old.DtrCombatTimeEnabled;
        Dtr.CombatTimePrefix = old.DtrCombatTimePrefix;
        Dtr.CombatTimeSuffix = old.DtrCombatTimeSuffix;
        Dtr.CombatTimeDecimalPrecision = old.DtrCombatTimeDecimalPrecision;
        Dtr.CombatTimeAlwaysDisableOutsideDuty = old.DtrCombatTimeAlwaysDisableOutsideDuty;
        Dtr.CombatTimeEnableHideAfter = old.DtrCombatTimeEnableHideAfter;
        Dtr.CombatTimeHideAfter = old.DtrCombatTimeHideAfter;
        Version = 3;
        Save();

        return this;
    }
}