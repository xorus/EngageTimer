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
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using EngageTimer.Commands;
using EngageTimer.Configuration;
using EngageTimer.Game;
using EngageTimer.Status;
using EngageTimer.Ui;
using JetBrains.Annotations;

namespace EngageTimer;

[PublicAPI]
public sealed class Plugin : IDalamudPlugin
{
    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    [PluginService] public static ICommandManager Commands { get; private set; } = null!;
    [PluginService] public static ICondition Condition { get; private set; } = null!;
    [PluginService] public static IDtrBar DtrBar { get; private set; } = null!;
    [PluginService] public static IPartyList PartyList { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] public static IGameInteropProvider GameInterop { get; private set; } = null!;
    [PluginService] public static IPluginLog Logger { get; private set; } = null!;
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] public static IToastGui ToastGui { get; private set; } = null!;
    public static ConfigurationFile Config { get; private set; } = null!;
    public static State State { get; private set; } = null!;
    public static Translator Translator { get; private set; } = null!;
    public static PluginUi PluginUi { get; private set; } = null!;
    public static NumberTextures NumberTextures { get; set; } = null!;
    public static string PluginPath { get; private set; } = null!;
    private static FrameworkThings FrameworkThings { get; set; } = null!;
    private static MainCommand MainCommand { get; set; } = null!;
    private static SettingsCommand SettingsCommand { get; set; } = null!;
    public static CombatAlarm CombatAlarm { get; set; } = null!;
    public static SfxPlay SfxPlay { get; set; } = null!;

    public Plugin(DalamudPluginInterface pluginInterface)
    {
        PluginPath = PluginInterface.AssemblyLocation.DirectoryName ??
                     throw new InvalidOperationException("Cannot find plugin directory");
        Config = ConfigurationLoader.Load();
        State = new State();
        Translator = new Translator();
        FrameworkThings = new FrameworkThings();
        MainCommand = new MainCommand();
        SettingsCommand = new SettingsCommand();
        PluginUi = new PluginUi();
        CombatAlarm = new CombatAlarm();
        SfxPlay = new SfxPlay();
    }

    void IDisposable.Dispose()
    {
        PluginInterface.SavePluginConfig(Config);
        CombatAlarm.Dispose();
        PluginUi.Dispose();
        FrameworkThings.Dispose();
        MainCommand.Dispose();
        SettingsCommand.Dispose();
        Translator.Dispose();
    }
}