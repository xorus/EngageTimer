using System;
using System.Threading.Tasks;
using EmbedIO.WebSockets;
using Swan.Formatters;

namespace EngageTimer.Web
{
    public class Websocket : WebSocketModule
    {
        private const string Format = "yyyy-MM-ddTHH:mm:ss.fffffffK";
        private readonly Configuration _configuration;
        private readonly State _state;

        private DateTime _lastUpdate;

        public Websocket(string urlPath, State state, Configuration configuration) : base(urlPath, true)
        {
            _state = state;
            _configuration = configuration;
        }

        protected override Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer,
            IWebSocketReceiveResult result)
        {
            // do nothing because we don't care~
            return null;
        }

        public void UpdateInfo()
        {
            if ((DateTime.Now - _lastUpdate).TotalSeconds > _configuration.WebSocketUpdateInterval)
            {
                _lastUpdate = DateTime.Now;
                BroadcastAsync(Json.Serialize(new
                {
                    _state.CountingDown,
                    _state.InCombat,
                    CombatStart = _state.CombatStart.ToString(Format),
                    CombatEnd = _state.CombatEnd.ToString(Format),
                    _state.CountDownValue,
                    Now = DateTime.Now.ToString(Format)
                }));
            }
        }
    }
}