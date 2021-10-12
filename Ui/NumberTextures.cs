using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Interface;
using ImGuiScene;

namespace EngageTimer.UI
{
    public class NumberTextures
    {
        private readonly Configuration _configuration;
        private readonly UiBuilder _uiBuilder;
        private readonly string _dataPath;
        private readonly Dictionary<int, TextureWrap> _numberTextures = new();
        private string _loadedTexturePreset;
        private string _loadedTextureDirectory;

        public int MaxTextureWidth { get; private set; }

        public int MaxTextureHeight { get; private set; }

        public int NumberNegativeMargin { get; private set; }

        public NumberTextures(Configuration configuration, UiBuilder uiBuilder, string dataPath)
        {
            _configuration = configuration;
            _uiBuilder = uiBuilder;
            _dataPath = dataPath;
        }

        public void Load()
        {
            // if (_loadedTextureDirectory == _configuration.CountdownTextureDirectory) return;
            _numberTextures.Clear();

            MaxTextureHeight = 0;
            MaxTextureWidth = 0;
            NumberNegativeMargin = 10;

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
                texturePath = Path.Combine(_dataPath, "Data", "numbers", preset);
            }

            for (var i = 0; i < 10; i++)
            {
                var path = Path.Combine(texturePath, i + ".png");

                TextureWrap texture;
                // dear sir resharper, you're wrong. Texture if forced cast to null if Dalamud couldn't load it.
                // ReSharper disable once ConstantNullCoalescingCondition
                texture = _uiBuilder.LoadImage(path) ??
                          _uiBuilder.LoadImage(Path.Combine(_dataPath, "Data", "error.png"));

                MaxTextureHeight = Math.Max(MaxTextureHeight, texture.Height);
                _numberTextures.Add(i, texture);
                MaxTextureWidth = Math.Max(MaxTextureWidth, texture.Width);
            }

            _loadedTextureDirectory = _configuration.CountdownTextureDirectory;
            _loadedTexturePreset = _configuration.CountdownTexturePreset;
        }

        public TextureWrap GetTexture(int i)
        {
            return _numberTextures[i];
        }
    }
}