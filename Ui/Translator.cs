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
        _pluginInterface = container.Resolve<DalamudPluginInterface>();
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
            _ => "en"
        };
        Resources.Culture = new CultureInfo(lang ?? "en");
        LocaleChanged?.Invoke(this, EventArgs.Empty);
    }
}