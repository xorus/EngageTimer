using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Interface;
using Dalamud.Logging;
using EngageTimer.Ui.Color;
using ImGuiScene;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StbiSharp;
using XwContainer;

namespace EngageTimer.Ui;

public sealed class NumberTextures
{
    private readonly Configuration _configuration;
    private readonly string _dataPath;
    private readonly TextureWrap _error;
    private readonly Dictionary<int, StbiImage> _numberImages = new();
    private readonly Dictionary<int, TextureWrap> _numberTextures = new();
    private readonly Dictionary<int, TextureWrap> _numberTexturesAlt = new();
    private readonly UiBuilder _uiBuilder;

    public NumberTextures(Container container)
    {
        _configuration = container.Resolve<Configuration>();
        _uiBuilder = container.Resolve<UiBuilder>();
        _dataPath = container.Resolve<Plugin>().PluginPath;
        _error = _uiBuilder.LoadImage(Path.Combine(_dataPath, "Data", "error.png"));

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
        if (_configuration.CountdownTexturePreset == "" && _configuration.CountdownTextureDirectory != null)
        {
            texturePath = _configuration.CountdownTextureDirectory;
        }
        else
        {
            // otherwise we load the selected preset (or default is not found)
            var preset = Configuration.BundledTextures[0];
            if (Configuration.BundledTextures.Contains(_configuration.CountdownTexturePreset ?? ""))
                preset = _configuration.CountdownTexturePreset;
            texturePath = Path.Combine(_dataPath, "Data", "numbers", preset ?? "");
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
                PluginLog.Warning("Invalid json in " + settingsFile);
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
                    PluginLog.Warning("Invalid json or missing property in " + settingsFile + "\n" +
                                      exception);
                }
        }
        catch (IOException)
        {
            PluginLog.Information("No settings.json found in number texture directory");
            // the file/directory does not exist or is invalid, we don't have to worry about it here
        }
        catch (Exception e)
        {
            PluginLog.Warning(e.ToString());
            // some other error we don't really care about
        }
    }

    public void CreateTextures()
    {
        MaxTextureHeight = 0;
        MaxTextureWidth = 0;

        var success = false;
        for (var i = 0; i < 10; i++)
        {
            if (_numberImages.ContainsKey(i))
                try
                {
                    var image = _numberImages[i];
                    var bytes = image.Data.ToArray();
                    var bytesAlt = new byte[bytes.Length];
                    if (image.NumChannels == 4)
                        for (var p = 0; p < bytes.Length; p += 4)
                        {
                            var originalRgb = new HslConv.Rgb(bytes[p], bytes[p + 1], bytes[p + 2]);
                            var hsl = HslConv.RgbToHsl(originalRgb);
                            if (_configuration.CountdownNumberRecolorMode)
                                hsl.H = Math.Clamp(_configuration.CountdownNumberHue, 0, 360);
                            else
                                hsl.H += _configuration.CountdownNumberHue;
                            hsl.S = Math.Clamp(hsl.S + _configuration.CountdownNumberSaturation, 0f, 1f);
                            hsl.L = Math.Clamp(hsl.L + _configuration.CountdownNumberLuminance, 0f, 1f);
                            var modifiedRgb = HslConv.HslToRgb(hsl);
                            bytes[p] = modifiedRgb.R;
                            bytes[p + 1] = modifiedRgb.G;
                            bytes[p + 2] = modifiedRgb.B;

                            if (!_configuration.CountdownAnimate) continue;
                            var hslAlt = new HslConv.Hsl(hsl.H, hsl.S, hsl.L);
                            hslAlt.L = Math.Clamp(hslAlt.L + .3f, 0f, 1f);
                            var modifiedRgbAlt = HslConv.HslToRgb(hslAlt);
                            bytesAlt[p] = modifiedRgbAlt.R;
                            bytesAlt[p + 1] = modifiedRgbAlt.G;
                            bytesAlt[p + 2] = modifiedRgbAlt.B;
                            bytesAlt[p + 3] = bytes[p + 3];
                        }

                    var texture = _uiBuilder.LoadImageRaw(bytes, image.Width, image.Height, image.NumChannels);
                    var textureAlt =
                        _uiBuilder.LoadImageRaw(bytesAlt, image.Width, image.Height, image.NumChannels);

                    MaxTextureHeight = Math.Max(MaxTextureHeight, texture.Height);
                    MaxTextureWidth = Math.Max(MaxTextureWidth, texture.Width);
                    _numberTextures.Remove(i);
                    _numberTextures.Add(i, texture);
                    success = true;

                    if (!_configuration.CountdownAnimate) continue;
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
    }

    public TextureWrap GetTexture(int i)
    {
        return _numberTextures.ContainsKey(i) ? _numberTextures[i] : _error;
    }

    public TextureWrap GetAltTexture(int i)
    {
        return _numberTexturesAlt.ContainsKey(i) ? _numberTexturesAlt[i] : _error;
    }
}