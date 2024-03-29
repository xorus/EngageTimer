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
using System.Globalization;
using EngageTimer.Properties;

namespace EngageTimer.Localization;

public static class Translator
{
    public static event EventHandler? LocaleChanged;

    public static void Register()
    {
        Plugin.PluginInterface.LanguageChanged += ConfigureLanguage;
        ConfigureLanguage();
    }

    public static void Unregister()
    {
        Plugin.PluginInterface.LanguageChanged -= ConfigureLanguage;
    }

    public static string TrId(string id)
    {
        return $"{Strings.ResourceManager.GetString(id, Strings.Culture) ?? id}###EngageTimer_{id}";
    }

    public static string TrId(string id, string fallback)
    {
        return $"{Strings.ResourceManager.GetString(id, Strings.Culture) ?? fallback}###EngageTimer_{id}";
    }

    public static string Tr(string id)
    {
        return Strings.ResourceManager.GetString(id, Strings.Culture) ?? id;
    }

    public static string Re(string str, params string[] replacements)
    {
        for (var index = 0; index < replacements.Length; index++)
        {
            var value = replacements[index];
            str = str.Replace("{" + index + "}", value);
        }

        return str;
    }

    public static string Tr(string id, params string[] replacements)
    {
        return Re(id, replacements);
    }
    
    private static void ConfigureLanguage(string? langCode = null)
    {
        var lang = (langCode ?? Plugin.PluginInterface.UiLanguage) switch
        {
            "fr" => "fr",
            "de" => "de",
            "ja" => "ja",
            "zh" => "zh",
            _ => "en"
        };
        Strings.Culture = new CultureInfo(lang ?? "en");
        LocaleChanged?.Invoke(null, EventArgs.Empty);
    }
}