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
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace EngageTimer.Game;

public sealed class AddonHider : IDisposable
{
    private const byte VisibleFlag = 0x20;
    private IntPtr _lastCountdownAddon = IntPtr.Zero;
    private bool _addonHidden = false;

    public AddonHider()
    {
        Plugin.State.CountingDownChanged += ShowAddonNearEnd;
        Plugin.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "ScreenInfo_CountDown", HideAddon);
        Plugin.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "ScreenInfo_CountDown", AddonFinalize);
    }

    /**
     * Re-enables the countdown addon when done counting down to show "START"
     */
    private unsafe void ShowAddonNearEnd(object? sender, EventArgs eventArgs)
    {
        if (Plugin.State.CountingDown
            || !_addonHidden
            || !Plugin.Config.Countdown.HideOriginalAddon
            || _lastCountdownAddon == IntPtr.Zero) return;
        _addonHidden = false;
        try
        {
            var atkUnitBase = (AtkUnitBase*)_lastCountdownAddon;
            atkUnitBase->IsVisible = true;
            // atkUnitBase->Flags |= VisibleFlag;
            // Plugin.Logger.Debug("show addon");
        }
        catch (Exception)
        {
            // invalid pointer, don't care and carry on
        }
    }

    /**
     * reset our internal "_addonHidden" state when the game destroys it
     */
    private void AddonFinalize(AddonEvent type, AddonArgs args)
    {
        _lastCountdownAddon = IntPtr.Zero;
        _addonHidden = false;
    }

    private unsafe void HideAddon(AddonEvent type, AddonArgs args)
    {
        if (!Plugin.Config.Countdown.HideOriginalAddon) return;
        var addon = args.Addon;
        _lastCountdownAddon = addon;
        if (addon == IntPtr.Zero) return;
        _addonHidden = true;
        try
        {
            var atkUnitBase = (AtkUnitBase*)addon.Address;
            atkUnitBase->IsVisible = false;
            // atkUnitBase->Flags = (byte)(atkUnitBase->Flags & ~VisibleFlag);
            // Plugin.Logger.Debug("hide addon");
        }
        catch (Exception)
        {
            // invalid pointer, don't care and carry on
        }
    }

    public void Dispose()
    {
        Plugin.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "ScreenInfo_CountDown", HideAddon);
        Plugin.AddonLifecycle.UnregisterListener(AddonEvent.PreFinalize, "ScreenInfo_CountDown", AddonFinalize);
    }
}