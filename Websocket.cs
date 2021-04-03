using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Plugin;
using EmbedIO.WebSockets;
using Swan.Formatters;

namespace EngageTimer
{
    public class Websocket : WebSocketModule
    {
        private readonly PluginUI _pluginUi;
        private readonly Configuration _configuration;

        public Websocket(string urlPath, PluginUI pluginUi, Configuration configuration) : base(urlPath, true)
        {
            _pluginUi = pluginUi;
            _configuration = configuration;
        }

        protected override Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer,
            IWebSocketReceiveResult result)
        {
            PluginLog.Log("received messaage");
            // do nothing because we don't care~
            return null;
        }

        private DateTime _lastUpdate = new DateTime();

        private const string Format = "yyyy-MM-ddTHH:mm:ss.fffffffK";

        public void UpdateInfo()
        {
            if ((DateTime.Now - _lastUpdate).TotalSeconds > _configuration.WebSocketUpdateInterval)
            {
                _lastUpdate = DateTime.Now;
                this.BroadcastAsync(Json.Serialize(new
                {
                    CountingDown = _pluginUi.CountingDown,
                    InCombat = _pluginUi.InCombat,
                    CombatStart = _pluginUi.CombatStart.ToString(Format),
                    CombatEnd = _pluginUi.CombatEnd.ToString(Format),
                    CountDownValue = _pluginUi.CountDownValue,
                    Now = DateTime.Now.ToString(Format)
                }));
            }
        }
    }
}