// This file is part of EngageTimer
// Copyright (C) 2024 Xorus <xorus@posteo.net>
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
using Dalamud.Interface.ManagedFontAtlas;

namespace EngageTimer.Ui;

public sealed class FloatingWindowFont : IDisposable
{
    public IFontHandle? FontHandle { get; private set; }

    public FloatingWindowFont()
    {
        UpdateFont();
    }

    public void UpdateFont()
    {
        this.FontHandle?.Dispose();
        this.FontHandle = Plugin.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(e => e.OnPreBuild(tk =>
        {
            // tk.AddDalamudDefaultFont(
            //     Math.Max(8, Plugin.Config.FloatingWindow.FontSize),
            //     FontAtlasBuildToolkitUtilities.ToGlyphRange([
            //         '-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', '.'
            //     ])
            // );
            // var spec = (SingleFontSpec)(Plugin.Config.FloatingWindow.FontSpec ??
            //                             Plugin.PluginInterface.UiBuilder.DefaultFontSpec);
            // var specWithRanges = new SingleFontSpec()
            // {
            //     FontId = spec.FontId,
            //     SizePx = spec.SizePx,
            //     SizePt = spec.SizePt,
            //     GlyphOffset = spec.GlyphOffset,
            //     LetterSpacing = spec.LetterSpacing,
            //     LineHeight = spec.LineHeight,
            //     GlyphRanges = FontAtlasBuildToolkitUtilities.ToGlyphRange([
            //         '-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', '.'
            //     ])
            // };

            var spec = Plugin.Config.FloatingWindow.FontSpec ?? Plugin.PluginInterface.UiBuilder.DefaultFontSpec;
            spec.AddToBuildToolkit(tk);
        }));
    }

    public void Dispose()
    {
        FontHandle?.Dispose();
    }
}