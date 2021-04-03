using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmbedIO.WebSockets;
using Swan.Formatters;

namespace EngageTimer
{
    public class Websocket : WebSocketModule
    {
        private readonly PluginUI _pluginUi;

        public Websocket(string urlPath, PluginUI pluginUi) : base(urlPath, false)
        {
            _pluginUi = pluginUi;
        }

        protected override Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer,
            IWebSocketReceiveResult result)
        {
            // do nothing because we don't care~
            return null;
        }

        private DateTime _lastUpdate = new DateTime();

        public void UpdateInfo()
        {
            if ((DateTime.Now - _lastUpdate).TotalSeconds > 1f)
            {
                _lastUpdate = DateTime.Now;
                this.BroadcastAsync(Json.Serialize(new
                {
                    CountingDown = _pluginUi.CountingDown,
                    CombatEnd = _pluginUi.CombatEnd,
                    CombatDuration = _pluginUi.CombatDuration,
                    CountDownValue = _pluginUi.CountDownValue
                }));
            }
        }
    }
}