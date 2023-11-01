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
using System.Globalization;
using Dalamud.Plugin;
using EngageTimer.Properties;
using XwContainer;

namespace EngageTimer.Ui;

public sealed class Translator : IDisposable
{
    private readonly DalamudPluginInterface _pluginInterface;

    public Translator(Container container)
    {
        _pluginInterface = Bag.PluginInterface;
        _pluginInterface.LanguageChanged += ConfigureLanguage;
        ConfigureLanguage();
    }

    public void Dispose()
    {
        _pluginInterface.LanguageChanged -= ConfigureLanguage;
    }

    public event EventHandler LocaleChanged;

    public string TransId(string id)
    {
        return $"{Resources.ResourceManager.GetString(id, Resources.Culture) ?? id}###EngageTimer_{id}";
    }

    public string TransId(string id, string fallback)
    {
        return $"{Resources.ResourceManager.GetString(id, Resources.Culture) ?? fallback}###EngageTimer_{id}";
    }

    public string Trans(string id)
    {
        return Resources.ResourceManager.GetString(id, Resources.Culture) ?? id;
    }

    public string Trans(string id, params string[] replacements)
    {
        var str = Trans(id);
        for (var index = 0; index < replacements.Length; index++)
        {
            var value = replacements[index];
            str = str.Replace("{" + index + "}", value);
        }

        return str;
    }

    private void ConfigureLanguage(string langCode = null)
    {
        var lang = (langCode ?? _pluginInterface.UiLanguage) switch
        {
            "fr" => "fr",
            "de" => "de",
            "ja" => "ja",
            "zh" => "zh",
            _ => "en"
        };
        Resources.Culture = new CultureInfo(lang ?? "en");
        LocaleChanged?.Invoke(this, EventArgs.Empty);
    }
}