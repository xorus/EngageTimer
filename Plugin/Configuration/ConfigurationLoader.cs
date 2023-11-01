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
using System.IO;
using EngageTimer.Configuration.Legacy;
using Newtonsoft.Json;

namespace EngageTimer.Configuration;

public static class ConfigurationLoader
{
    public static ConfigurationFile Load()
    {
        var logger = Plugin.Logger;

        var configFile = Plugin.PluginInterface.GetPluginConfig();

        if (configFile == null) return new ConfigurationFile();
        if (configFile.Version >= 3) return (ConfigurationFile)configFile;

        Plugin.Logger.Information($"Migrating config file from v{configFile?.Version}");
        // config files before 3 needs some BIG MIGRATION WORK
        try
        {
            return new ConfigurationFile().Import(
                JsonConvert.DeserializeObject<OldConfig>(
                    File.ReadAllText(Plugin.PluginInterface.ConfigFile.FullName)
                ).Migrate()
            );
        }
        catch (Exception exception)
        {
            // welcome to the error space, I don't really care what went wrong, I'll just generate a new file
            logger.Error(exception, "the config could not be migrated >>>..////..<<< sowwwyyyy");
            return new ConfigurationFile();
        }
    }
}