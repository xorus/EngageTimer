using System;
using Dalamud.Plugin;
using EngageTimer.Attributes;
using Dalamud.Hooking;
using System.Runtime.InteropServices;
using ImGuiNET;
using System.Numerics;
using System.IO;
using System.Reflection;

/**
 * Based on the work of https://github.com/Haplo064/Europe
 **/
namespace EngageTimer
{
    public class Plugin : IDalamudPlugin
    {
        private DalamudPluginInterface _pluginInterface;
        private PluginCommandManager<Plugin> _commandManager;
        private Configuration _configuration;
        private PluginUI _ui;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr CountdownTimer(ulong p1);

        private CountdownTimer _countdownTimer;
        private Hook<CountdownTimer> _countdownTimerHook;
        private IntPtr _countdownPtr;

        private ulong _countDown = 0;
        private float _countUp = 0.00f;
        private DateTime _cdEnd = new DateTime(2010);

        private WebServer _server;

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string AssemblyLocation { get; set; }

        public string Name => "Engage Timer";

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this._pluginInterface = pluginInterface;
            string localPath;
            try
            {
                // For loading with Dalamud
                localPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            catch
            {
                // For loading with LPL
                localPath = Path.GetDirectoryName(AssemblyLocation);
            }

            this._configuration = (Configuration) this._pluginInterface.GetPluginConfig() ?? new Configuration();
            this._configuration.Initialize(this._pluginInterface);

            this._ui = new PluginUI(this._pluginInterface, this._configuration, localPath);
            this._pluginInterface.UiBuilder.OnBuildUi += this._ui.Draw;

            this._commandManager = new PluginCommandManager<Plugin>(this, this._pluginInterface);

            _countdownPtr = pluginInterface.TargetModuleScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 40 8B 41");
            _countdownTimer = new CountdownTimer(CountdownTimerFunc);
            try
            {
                _countdownTimerHook = new Hook<CountdownTimer>(_countdownPtr, _countdownTimer, this);
                _countdownTimerHook.Enable();
            }
            catch (Exception e)
            {
                PluginLog.Log("Could not hook to timer\n" + e.ToString());
            }

            this._pluginInterface.UiBuilder.OnBuildUi += DrawUi;
            this._pluginInterface.UiBuilder.OnOpenConfigUi += OpenConfigUi;

            this._server = new WebServer(_configuration, localPath, this._ui);
        }

        [Command("/egsettings")]
        [HelpMessage("Opens up the settings")]
        public void ExampleCommand1(string command, string args)
        {
            this._ui.SettingsVisible = true;
            // // You may want to assign these references to private variables for convenience.
            // // Keep in mind that the local player does not exist until after logging in.
            // var chat = this.pluginInterface.Framework.Gui.Chat;
            // var world = this.pluginInterface.ClientState.LocalPlayer.CurrentWorld.GameData;
            // chat.Print($"Hello {world.Name}!");
            // PluginLog.Log("Message sent successfully.");
        }

        /// <summary>
        /// Ticks since the timer stalled
        /// </summary>
        private int _countDownStallTicks = 0;

        private float _lastCountDownValue = 0;
        private bool _countDownRunning = false;

        private IntPtr CountdownTimerFunc(ulong param_1)
        {
            _countDown = param_1;


            return _countdownTimerHook.Original(param_1);
        }

        private DateTime _combatTimeStart = new DateTime();
        private DateTime _combatTimeEnd = new DateTime();
        private bool _shouldRestartCombatTimer = true;

        private void UpdateEncounterTimer()
        {
            if (_pluginInterface.ClientState.Condition[Dalamud.Game.ClientState.ConditionFlag.InCombat])
            {
                if (_shouldRestartCombatTimer)
                {
                    _shouldRestartCombatTimer = false;
                    _combatTimeStart = DateTime.Now;
                }

                _combatTimeEnd = DateTime.Now;
            }
            else
            {
                _shouldRestartCombatTimer = true;
            }

            if (!_configuration.DisplayStopwatch)
                return;
            _ui.CombatDuration = _combatTimeEnd - _combatTimeStart;
            _ui.CombatEnd = _combatTimeEnd;
        }

        private void UpdateCountDown()
        {
            this._ui.CountingDown = false;
            if (_countDown != 0)
            {
                _cdEnd = DateTime.Now;

                float countDownPointerValue = Marshal.PtrToStructure<float>((IntPtr) _countDown + 0x2c);

                // is last value close enough (workaround for floating point approx)
                if (Math.Abs(countDownPointerValue - _lastCountDownValue) < 0.001f) 
                {
                    _countDownStallTicks++;
                }
                else
                {
                    _countDownStallTicks = 0;
                    _countDownRunning = true;
                }

                if (_countDownStallTicks > 50)
                {
                    _countDownRunning = false;
                }

                if (countDownPointerValue > 0 && _countDownRunning)
                {
                    this._ui.CountDownValue = Marshal.PtrToStructure<float>((IntPtr) _countDown + 0x2c);
                    this._ui.CountingDown = true;
                }

                _lastCountDownValue = countDownPointerValue;
            }
        }

        private void DrawUi()
        {
            this.UpdateCountDown();
            this.UpdateEncounterTimer();
        }

        private void OpenConfigUi(object sender, EventArgs args)
        {
            this._ui.SettingsVisible = true;
        }


        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            this._commandManager.Dispose();
            this._pluginInterface.SavePluginConfig(this._configuration);
            this._pluginInterface.UiBuilder.OnBuildUi -= this._ui.Draw;
            this._pluginInterface.Dispose();
            _countdownTimerHook.Disable();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}