﻿// This file is part of EngageTimer
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using EngageTimer.Configuration;
using EngageTimer.Ui.Color;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StbiSharp;

namespace EngageTimer.Ui;

public sealed class NumberTextures
{
    private readonly IDalamudTextureWrap _error;
    private readonly Dictionary<int, StbiImage> _numberImages = new();
    private readonly Dictionary<int, IDalamudTextureWrap> _numberTextures = new();
    private readonly Dictionary<int, IDalamudTextureWrap> _numberTexturesAlt = new();
    private readonly IUiBuilder _uiBuilder = Plugin.PluginInterface.UiBuilder;

    public double LastTextureCreationDuration = 0d;

    public NumberTextures()
    {
        // plugin crash if the default file is not found is expected
        _error = Plugin.TextureProvider.CreateFromImageAsync(File.OpenRead(Path.Combine(Plugin.PluginPath, "Data",
            "error.png"))).Result;
        Load();
    }

    public int MaxTextureWidth { get; private set; }
    public int MaxTextureHeight { get; private set; }
    public int NumberNegativeMargin { get; private set; }
    public int NumberNegativeMarginMono { get; private set; }
    public int NumberBottomMargin { get; private set; }

    public void Load()
    {
        _numberTextures.Clear();
        _numberTexturesAlt.Clear();
        LoadImages();
        CreateTextures();
    }

    private void LoadImages()
    {
        _numberImages.Clear();
        string texturePath;

        // if mode is custom and a directory is specified
        if (Plugin.Config.Countdown.TexturePreset == "" && Plugin.Config.Countdown.TextureDirectory != null)
        {
            texturePath = Plugin.Config.Countdown.TextureDirectory;
        }
        else
        {
            // otherwise we load the selected preset (or default is not found)
            var preset = CountdownConfiguration.BundledTextures[0];
            if (CountdownConfiguration.BundledTextures.Contains(Plugin.Config.Countdown.TexturePreset ?? ""))
                preset = Plugin.Config.Countdown.TexturePreset;
            texturePath = Path.Combine(Plugin.PluginPath, "Data", "numbers", preset ?? "");
        }

        // Read pack settings file
        NumberNegativeMargin = 10;
        NumberNegativeMarginMono = 10;
        NumberBottomMargin = 20;
        ReadPackSettings(Path.Combine(texturePath, "settings.json"));

        // Load images
        for (var i = 0; i < 10; i++)
            try
            {
                using var stream = File.OpenRead(Path.Combine(texturePath, i + ".png"));
                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                _numberImages.Add(i, Stbi.LoadFromMemory(memoryStream, 4));
            }
            catch (Exception)
            {
                // the image does not exist or is invalid, we don't have to worry about it here
            }
    }

    private void ReadPackSettings(string settingsFile)
    {
        try
        {
            var json = File.ReadAllText(settingsFile);
            var parsed = JsonConvert.DeserializeObject<JToken>(json);
            if (parsed == null)
                Plugin.Logger.Warning("Invalid json in " + settingsFile);
            else
                try
                {
                    var nnnToken = parsed.SelectToken("NumberNegativeMargin");
                    if (nnnToken != null) NumberNegativeMargin = nnnToken.Value<int>();

                    var nnnmToken = parsed.SelectToken("NumberNegativeMarginMono");
                    if (nnnmToken != null)
                        NumberNegativeMarginMono = nnnmToken.Value<int>();
                    else
                        NumberNegativeMarginMono = NumberNegativeMargin;

                    var nbmToken = parsed.SelectToken("NumberBottomMargin");
                    if (nbmToken != null) NumberBottomMargin = nbmToken.Value<int>();
                }
                catch (Exception exception)
                {
                    Plugin.Logger.Warning("Invalid json or missing property in " + settingsFile + "\n" +
                                          exception);
                }
        }
        catch (IOException)
        {
            Plugin.Logger.Information("No settings.json found in number texture directory");
            // the file/directory does not exist or is invalid, we don't have to worry about it here
        }
        catch (Exception e)
        {
            Plugin.Logger.Warning(e.ToString());
            // some other error we don't really care about
        }
    }

    public void CreateTextures()
    {
        var watch = Stopwatch.StartNew();
        MaxTextureHeight = 0;
        MaxTextureWidth = 0;
        
        var createAltTextures = Plugin.Config.Countdown.Animate;

        var success = false;
        for (var i = 0; i < 10; i++)
        {
            if (_numberImages.TryGetValue(i, out var image))
                try
                {
                    var bytes = image.Data.ToArray();
                    var bytesAlt = new byte[bytes.Length];
                    if (image.NumChannels == 4)
                        for (var p = 0; p < bytes.Length; p += 4)
                        {
                            var originalRgb = new HslConv.Rgb(bytes[p], bytes[p + 1], bytes[p + 2]);
                            var hsl = HslConv.RgbToHsl(originalRgb);
                            if (Plugin.Config.Countdown.NumberRecolorMode)
                                hsl.H = Math.Clamp(Plugin.Config.Countdown.Hue, 0, 360);
                            else
                                hsl.H += Plugin.Config.Countdown.Hue;
                            hsl.S = Math.Clamp(hsl.S + Plugin.Config.Countdown.Saturation, 0f, 1f);
                            hsl.L = Math.Clamp(hsl.L + Plugin.Config.Countdown.Luminance, 0f, 1f);
                            var modifiedRgb = HslConv.HslToRgb(hsl);
                            bytes[p] = modifiedRgb.R;
                            bytes[p + 1] = modifiedRgb.G;
                            bytes[p + 2] = modifiedRgb.B;

                            if (!createAltTextures) continue;
                            var hslAlt = new HslConv.Hsl(hsl.H, hsl.S, hsl.L);
                            hslAlt.L = Math.Clamp(hslAlt.L + .3f, 0f, 1f);
                            var modifiedRgbAlt = HslConv.HslToRgb(hslAlt);
                            bytesAlt[p] = modifiedRgbAlt.R;
                            bytesAlt[p + 1] = modifiedRgbAlt.G;
                            bytesAlt[p + 2] = modifiedRgbAlt.B;
                            bytesAlt[p + 3] = bytes[p + 3];
                        }


                    var texture = Plugin.TextureProvider.CreateFromRaw(
                        RawImageSpecification.Rgba32(image.Width, image.Height),
                        bytes);
                    var textureAlt = Plugin.TextureProvider.CreateFromRaw(
                        RawImageSpecification.Rgba32(image.Width, image.Height),
                        bytesAlt);

                    MaxTextureHeight = Math.Max(MaxTextureHeight, texture.Height);
                    MaxTextureWidth = Math.Max(MaxTextureWidth, texture.Width);
                    _numberTextures.Remove(i);
                    _numberTextures.Add(i, texture);
                    success = true;

                    if (!createAltTextures) continue;
                    _numberTexturesAlt.Remove(i);
                    _numberTexturesAlt.Add(i, textureAlt);
                }
                catch (Exception)
                {
                    // a loading error occured
                }

            if (success) continue;
            MaxTextureWidth = _error.Width;
            MaxTextureHeight = _error.Height;
        }

        watch.Stop();
        LastTextureCreationDuration = watch.ElapsedMilliseconds / 1000d;
    }

    public IDalamudTextureWrap GetTexture(int i)
    {
        return _numberTextures.GetValueOrDefault(i, _error);
    }

    public IDalamudTextureWrap GetAltTexture(int i)
    {
        return _numberTexturesAlt.GetValueOrDefault(i, _error);
    }
}