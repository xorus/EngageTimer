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

        private bool _forceUpdateNextTick = false;
        private float _updateInterval;

        private const float UpdateTimeIdle = 2f;
        private const float UpdateTimeInCombat = 20f;


        public Websocket(string urlPath, State state, Configuration configuration) : base(urlPath, true)
        {
            _state = state;
            _configuration = configuration;
            _updateInterval = (_state.InCombat || _state.CountingDown) ? UpdateTimeIdle : UpdateTimeInCombat;

            EventHandler e = (sender, args) =>
            {
                _updateInterval = (_state.InCombat || _state.CountingDown) ? UpdateTimeIdle : UpdateTimeInCombat;
                _forceUpdateNextTick = true;
            };

            _state.InCombatChanged += e;
            _state.CountingDownChanged += e;
        }

        protected override Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer,
            IWebSocketReceiveResult result)
        {
            // do nothing because we don't care~
            return null;
        }

        public void UpdateInfo()
        {
            if (_forceUpdateNextTick || (DateTime.Now - _lastUpdate).TotalSeconds > _updateInterval)
            {
                _forceUpdateNextTick = false;
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