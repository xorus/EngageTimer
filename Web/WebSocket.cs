using System;
using System.Threading.Tasks;
using EmbedIO.WebSockets;
using EngageTimer.Status;
using Swan.Formatters;

namespace EngageTimer.Web;

public class Websocket : WebSocketModule
{
    private const string Format = "yyyy-MM-ddTHH:mm:ss.fffffffK";

    private const float UpdateTimeIdle = 2f;
    private const float UpdateTimeInCombat = 20f;
    private readonly Configuration _configuration;
    private readonly State _state;

    private bool _forceUpdateNextTick;

    private DateTime _lastUpdate;

    private string _lastWebConfig;
    private float _updateInterval;

    public Websocket(string urlPath, State state, Configuration configuration) : base(urlPath, true)
    {
        _state = state;
        _configuration = configuration;
        _updateInterval = _state.InCombat || _state.CountingDown ? UpdateTimeIdle : UpdateTimeInCombat;

        void EventHandler(object o, EventArgs eventArgs)
        {
            _updateInterval = _state.InCombat || _state.CountingDown ? UpdateTimeIdle : UpdateTimeInCombat;
            _forceUpdateNextTick = true;
        }

        _state.InCombatChanged += EventHandler;
        _state.CountingDownChanged += EventHandler;

        _lastWebConfig = GetConfigMessage();
        _configuration.OnSave += (_, _) =>
        {
            var webConfig = GetConfigMessage();
            if (!_lastWebConfig.Equals(webConfig)) _lastWebConfig = webConfig;

            BroadcastAsync(_lastWebConfig);
        };
    }

    protected override Task OnClientConnectedAsync(IWebSocketContext context)
    {
        return SendAsync(context, _lastWebConfig)
            .ContinueWith(_ => SendAsync(context, GetUpdateMessage()));
    }

    protected override Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer,
        IWebSocketReceiveResult result)
    {
        // do nothing because we don't care~
        return null;
    }

    public void UpdateInfo()
    {
        if (!_forceUpdateNextTick && !((DateTime.Now - _lastUpdate).TotalSeconds > _updateInterval)) return;

        _forceUpdateNextTick = false;
        _lastUpdate = DateTime.Now;
        BroadcastAsync(GetUpdateMessage());
    }

    private string GetConfigMessage()
    {
        return Json.Serialize(new
        {
            Config = _configuration.GetWebConfig()
        });
    }

    private string GetUpdateMessage()
    {
        return Json.Serialize(new
        {
            _state.CountingDown,
            _state.InCombat,
            CombatStart = _state.CombatStart.ToString(Format),
            CombatEnd = _state.CombatEnd.ToString(Format),
            _state.CountDownValue,
            Now = DateTime.Now.ToString(Format)
        });
    }
}