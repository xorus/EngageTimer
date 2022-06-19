using System;
using System.Globalization;
using Dalamud.Plugin;
using EngageTimer.Properties;

namespace EngageTimer;

public sealed class Locale : IDisposable
{
    private readonly Container _container;
    private readonly DalamudPluginInterface _pluginInterface;

    public Locale(Container container)
    {
        _container = container;
        _pluginInterface = container.Resolve<DalamudPluginInterface>();

        _pluginInterface.LanguageChanged += ConfigureLanguage;
        ConfigureLanguage();
    }

    private void ConfigureLanguage(string langCode = null)
    {
        var lang = (langCode ?? _pluginInterface.UiLanguage) switch
        {
            "fr" => "fr",
            "de" => "de",
            // "ja" => "ja",
            _ => "en"
        };
        Resources.Culture = new CultureInfo(lang ?? "en");
    }

    public void Dispose()
    {
        _pluginInterface.LanguageChanged -= ConfigureLanguage;
    }
}