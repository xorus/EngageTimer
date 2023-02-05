using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Dalamud.Game.Gui;
using Dalamud.Interface.Animation;
using Dalamud.Interface.Animation.EasingFunctions;
using EngageTimer.Status;
using EngageTimer.Ui.CustomEasing;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using XwContainer;

namespace EngageTimer.Ui;

public sealed class CountDown
{
    private const byte VisibleFlag = 0x20;

    private const float BaseNumberScale = 1f;
    private const int GameCountdownWidth = 60; // just trust in the magic numbers
    private const float AnimationSize = .7f;
    private readonly Configuration _configuration;

    private readonly Easing _easing = new OutCubic(new TimeSpan(0, 0, 0, 0, 1000));

    private readonly Easing _easingOpacity = new OpacityEasing(
        new TimeSpan(0, 0, 0, 0, 1000),
        1, -0.02, .71, 1
    );

    private readonly GameGui _gui;
    private readonly NumberTextures _numberTextures;
    private readonly State _state;
    private bool _accurateMode;

    /**
     * This is a workaround for ImGui taking some time to render a window for the first time.
     * It can cause a small amount on lag when starting a countdown, which we do not want, and this is why I will
     * draw the countdown windows for the first frame the plugin gets loaded so ImGui doesn't get a chance to lag
     * 
     * In my testing, this is about 10 to 20ms.
     */
    private bool _firstLoad = true;

    private int _lastSecond;
    private bool _originalAddonHidden;
    private bool _wasInMainViewport = true;

    public CountDown(Container container)
    {
        _configuration = container.Resolve<Configuration>();
        _state = container.Resolve<State>();
        _gui = container.Resolve<GameGui>();
        _numberTextures = container.Resolve<NumberTextures>();
        _configuration.OnSave += ConfigurationOnOnSave;
        _state.StartCountingDown += Start;
        UpdateFromConfig();
    }

    public static bool ShowBackground { get; set; }

    private void ConfigurationOnOnSave(object sender, EventArgs e)
    {
        UpdateFromConfig();
    }

    /**
     * Things I need to simplify/re-use in this class but dont need to compute every frame
     */
    private void UpdateFromConfig()
    {
        _accurateMode = _configuration.HideOriginalCountdown && _configuration.CountdownAccurateCountdown;
    }

    // finds the original CountDown addon and toggles its visibility flag
    private unsafe void ToggleOriginalAddon()
    {
        var addon = _gui.GetAddonByName("ScreenInfo_CountDown", 1);
        if (addon == IntPtr.Zero) return;

        try
        {
            var atkUnitBase = (AtkUnitBase*)addon;
            atkUnitBase->Flags ^= VisibleFlag;
            _originalAddonHidden = (atkUnitBase->Flags & VisibleFlag) == 0;
        }
        catch (Exception)
        {
            // invalid pointer, don't care and carry on
        }
    }

    // Fix for #19 (countdown overlapping when restarting)
    // ensures the countdown addon is marked as not hidden when starting or restarting a timer
    private void Start(object sender, EventArgs e)
    {
        _originalAddonHidden = false;
    }

    public void Draw()
    {
#if DEBUG
        if (ImGui.Begin("egdebug"))
        {
            ImGui.Text("_originalAddonHidden: " + _originalAddonHidden);
            ImGui.Text("CountingDown: " + _state.CountingDown);
            ImGui.Text("CountDownValue: " + _state.CountDownValue);
            ImGui.Text("Mocked: " + _state.Mocked);
            ImGui.Text("CombatDuration: " + _state.CombatDuration);
            ImGui.Text("CombatEnd: " + _state.CombatEnd);
            ImGui.Text("CombatStart: " + _state.CombatStart);
            ImGui.Text("InCombat: " + _state.InCombat);
            ImGui.Text("InInstance: " + _state.InInstance);
        }

        ImGui.End();
#endif

        if (_configuration.MigrateCountdownOffsetToPercent)
        {
            _configuration.CountdownWindowOffset /= ImGui.GetIO().DisplaySize;
            _configuration.MigrateCountdownOffsetToPercent = false;
            _configuration.Save();
        }

        // display is disabled
        if (!_configuration.DisplayCountdown) return;


        if (!_firstLoad && (!_state.CountingDown || !_configuration.DisplayCountdown))
        {
            // re-enable the original addon at the last possible moment (when done counting down) to show "START"
            if (_originalAddonHidden && _configuration.HideOriginalCountdown) ToggleOriginalAddon();
            return;
        }

        if (_configuration.HideOriginalCountdown && _state.CountDownValue <= 5 && !_originalAddonHidden)
            ToggleOriginalAddon();

        var showMainCountdown = _firstLoad || _state.CountDownValue > 5 || _configuration.HideOriginalCountdown;
        if (showMainCountdown && _configuration.EnableCountdownDisplayThreshold &&
            _state.CountDownValue > _configuration.CountdownDisplayThreshold)
            return;

        var numberScale = BaseNumberScale;
        var maxNumberScale = numberScale;
        if (showMainCountdown)
        {
            numberScale *= _configuration.CountdownScale;
            maxNumberScale = numberScale;
            // numberScale += (_state.CountDownValue % 1) * 0.7f;

            if (_configuration.CountdownAnimate)
            {
                var second = (int)_state.CountDownValue;
                if (_lastSecond != second)
                {
                    _easing.Restart();
                    _easingOpacity.Restart();
                    _lastSecond = second;
                }

                _easing.Update();
                _easingOpacity.Update();
                if (_configuration.CountdownAnimateScale)
                {
                    maxNumberScale = numberScale + AnimationSize;
                    numberScale += AnimationSize * (1 - (float)_easing.Value);
                }
            }
        }

        var negativeMargin = _configuration.CountdownCustomNegativeMargin ?? (
            _configuration.CountdownMonospaced
                ? _numberTextures.NumberNegativeMarginMono
                : _numberTextures.NumberNegativeMargin
        ) * numberScale;

        var io = ImGui.GetIO();
        var windowPosition = new Vector2(io.DisplaySize.X * 0.5f, io.DisplaySize.Y * 0.5f) +
                             _configuration.CountdownWindowOffset * io.DisplaySize;
        var windowSize = new Vector2(
            maxNumberScale * (_numberTextures.MaxTextureWidth *
                              (_configuration.EnableCountdownDecimal
                                  ? 3f + _configuration.CountdownDecimalPrecision * .5f
                                  : 2
                              )),
            _numberTextures.MaxTextureHeight * maxNumberScale + 30);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.SetNextWindowPos(windowPosition, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

        var flags = ImGuiWindowFlags.NoTitleBar
                    | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar
                    | ImGuiWindowFlags.NoDecoration
                    | ImGuiWindowFlags.NoFocusOnAppearing
                    | ImGuiWindowFlags.NoNavFocus
                    | ImGuiWindowFlags.NoInputs
                    | ImGuiWindowFlags.NoMouseInputs
            ;
        if (_wasInMainViewport) flags |= ImGuiWindowFlags.NoBackground;

        var visible = true;
        // prevent a big 0 appearing on screen when "initializing" the countdown window by alpha-ing it
        if (_firstLoad) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0f);
        if (ImGui.Begin("EngageTimer Countdown", ref visible, flags))
        {
            _wasInMainViewport = ImGui.GetWindowViewport().ID == ImGui.GetMainViewport().ID;
            if (ShowBackground)
            {
                var d = ImGui.GetBackgroundDrawList();
                d.AddRect(
                    ImGui.GetWindowPos(),
                    ImGui.GetWindowPos() + ImGui.GetWindowSize(),
                    ImGui.GetColorU32(ImGuiCol.Text), 0f, ImDrawFlags.None,
                    7f + (float)Math.Sin(ImGui.GetTime() * 2) * 5f);
                ShowBackground = false;
            }

            DrawCountdown(io, showMainCountdown, numberScale, negativeMargin, false);
            if (_configuration.CountdownAnimate && _configuration.CountdownAnimateOpacity)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, (float)_easingOpacity.Value);
                DrawCountdown(io, showMainCountdown, numberScale, negativeMargin, true);
                ImGui.PopStyleVar();
            }
        }

        if (_firstLoad)
        {
            ImGui.PopStyleVar();
            _firstLoad = false;
        }

        ImGui.PopStyleVar();
        ImGui.End();
    }

    private void DrawCountdown(ImGuiIOPtr io, bool showMainCountdown, float numberScale, float negativeMarginScaled,
        bool alternateMode)
    {
        var windowSize = ImGui.GetWindowSize();
        var totalHeight = _numberTextures.MaxTextureHeight * numberScale;
        ImGui.SetCursorPosY((windowSize.Y - totalHeight) / 2f);

        var mainTotalWidth = 0f;
        List<int> mainNumbers = null;

        if (showMainCountdown)
        {
            var number = _accurateMode
                ? Math.Floor(_state.CountDownValue).ToString(CultureInfo.InvariantCulture)
                : Math.Ceiling(_state.CountDownValue).ToString(CultureInfo.InvariantCulture);

            if (_configuration.CountdownLeadingZero && number.Length == 1) number = "0" + number;

            mainNumbers = NumberList(number);
            // First loop to compute total width
            if (_configuration.CountdownMonospaced)
                mainTotalWidth = (_numberTextures.MaxTextureWidth * numberScale - negativeMarginScaled) *
                                 mainNumbers.Count;
            else
                foreach (var i in mainNumbers)
                {
                    var texture = _numberTextures.GetAltTexture(i);
                    mainTotalWidth += texture.Width * numberScale - negativeMarginScaled;
                }

            mainTotalWidth += negativeMarginScaled;
        }

        var decimalTotalWidth = 0f;
        List<int> decimalNumbers = null;

        var smolNumberScale = numberScale * .5f;
        var smolMaxWidthScaled = _numberTextures.MaxTextureWidth * smolNumberScale;
        var smolNumberCursorY = 0f;
        if (_configuration.EnableCountdownDecimal)
        {
            var decimalPart =
                (_state.CountDownValue - Math.Truncate(_state.CountDownValue))
                .ToString("F" + _configuration.CountdownDecimalPrecision, CultureInfo.InvariantCulture)[2..];

            // align the small numbers on the number baseline
            var offsetY = _numberTextures.MaxTextureHeight * numberScale
                          - _numberTextures.MaxTextureHeight * smolNumberScale
                          - _numberTextures.NumberBottomMargin * smolNumberScale;

            smolNumberCursorY = ImGui.GetCursorPosY() + offsetY;
            decimalNumbers = NumberList(decimalPart);
            decimalTotalWidth = _numberTextures.MaxTextureWidth * (decimalNumbers.Count * smolNumberScale) -
                                (decimalNumbers.Count - 1) * negativeMarginScaled * smolNumberScale;
        }

        // draw main
        if (mainNumbers != null)
        {
            if (_configuration.CountdownAlign == Configuration.TextAlign.Left)
                ImGui.SetCursorPosX(0f);
            else if (_configuration.CountdownAlign == Configuration.TextAlign.Center)
                ImGui.SetCursorPosX(windowSize.X / 2f - mainTotalWidth / 2f);
            else if (_configuration.CountdownAlign == Configuration.TextAlign.Right)
                ImGui.SetCursorPosX(windowSize.X - (mainTotalWidth + decimalTotalWidth));

            // Draw the images \o/
            foreach (var i in mainNumbers)
                DrawNumber(alternateMode, i,
                    numberScale, negativeMarginScaled,
                    _numberTextures.MaxTextureWidth * numberScale, _configuration.CountdownMonospaced);
        }

        if (mainNumbers == null && decimalNumbers != null) ImGui.SetCursorPosX(windowSize.X / 2f + GameCountdownWidth);

        if (decimalNumbers == null) return;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + negativeMarginScaled);
        foreach (var i in decimalNumbers)
        {
            ImGui.SetCursorPosY(smolNumberCursorY);
            // small numbers are always fixed width
            DrawNumber(alternateMode, i, smolNumberScale,
                negativeMarginScaled * smolNumberScale, smolMaxWidthScaled, true);
        }
    }

    private void DrawNumber(bool alternateMode, int i, float numberScale, float negativeMarginScaled,
        float maxWidthScaled,
        bool fixedWidth)
    {
        var texture = alternateMode ? _numberTextures.GetAltTexture(i) : _numberTextures.GetTexture(i);
        var width = texture.Width * numberScale;
        var cursorX = ImGui.GetCursorPosX();
        if (fixedWidth) ImGui.SetCursorPosX(cursorX + (maxWidthScaled - width) / 2);

        ImGui.Image(texture.ImGuiHandle,
            new Vector2(texture.Width * numberScale, texture.Height * numberScale));
        ImGui.SameLine();

        if (fixedWidth)
            ImGui.SetCursorPosX(cursorX + maxWidthScaled - negativeMarginScaled);
        else
            ImGui.SetCursorPosX(cursorX + width - negativeMarginScaled);
    }

    private static List<int> NumberList(string number)
    {
        var integers = new List<int>();
        foreach (var c in number)
        {
            int i;
            if (int.TryParse(c.ToString(), out i)) integers.Add(i);
        }

        return integers;
    }
}