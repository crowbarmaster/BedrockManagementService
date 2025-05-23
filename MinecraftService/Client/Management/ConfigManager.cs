﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Text;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;

namespace MinecraftService.Client.Management {
    public class ConfigManager {
        public string ConfigDir = $@"{Directory.GetCurrentDirectory()}";
        public string ConfigFile;
        public List<ClientSideServiceConfiguration> HostConnectList = new();
        public string NBTStudioPath;
        public bool DefaultScrollLock = false;
        public bool DisplayTimestamps = false;
        public bool DebugNetworkOutput = false;
        private readonly MmsLogger Logger;

        public ConfigManager(MmsLogger logger) {
            Logger = logger;
            ConfigFile = $@"{ConfigDir}\Client.conf";
        }

        public void LoadConfigs() {
            HostConnectList.Clear();
            if (!Directory.Exists(ConfigDir)) {
                Directory.CreateDirectory(ConfigDir);
            }
            if (!File.Exists(ConfigFile)) {
                Logger.AppendLine("Config file missing! Regenerating default file...");
                CreateDefaultConfig();
            }
            string[] lines = File.ReadAllLines(ConfigFile);
            foreach (string line in lines) {
                string[] entrySplit = line.Split('=');
                if (!string.IsNullOrEmpty(line) && !line.StartsWith("#")) {
                    if (entrySplit[0] == "HostEntry") {
                        string[] hostSplit = entrySplit[1].Split(';');
                        string[] addressSplit = hostSplit[1].Split(':');
                        ClientSideServiceConfiguration hostToList = new ClientSideServiceConfiguration(hostSplit[0], addressSplit[0], addressSplit[1]);
                        HostConnectList.Add(hostToList);
                    }
                    if (entrySplit[0] == "NBTStudioPath") {
                        NBTStudioPath = entrySplit[1];
                    }
                    if (entrySplit[0] == "EnableScrollbarLockDefault") {
                        DefaultScrollLock = entrySplit[1].ToLower().Equals(bool.TrueString.ToLower());
                    }
                    if (entrySplit[0] == "DisplayTimestamps") {
                        DisplayTimestamps = entrySplit[1].ToLower().Equals(bool.TrueString.ToLower());
                    }
                }
            }
        }

        public void CreateDefaultConfig() {
            string[] Config = new string[]
            {
                "HostEntry=host1;127.0.0.1:19134"
            };
            StringBuilder builder = new();
            builder.Append("# Hosts\n");
            foreach (string entry in Config) {
                builder.Append($"{entry}\n");
            }
            builder.Append("\n# Settings\n");
            builder.Append("NBTStudioPath=\n");
            builder.Append($"EnableScrollbarLockDefault={DefaultScrollLock}\n");
            File.WriteAllText(ConfigFile, builder.ToString());
        }

        public void SaveConfigFile() {
            StringBuilder fileContent = new();
            fileContent.Append("# hosts\n\n");
            foreach (ClientSideServiceConfiguration host in HostConnectList) {
                fileContent.Append($"HostEntry={host.GetHostName()};{host.GetAddress()}:{host.GetPort()}\n");
            }
            fileContent.Append("\n# Settings\n");
            fileContent.Append($"NBTStudioPath={NBTStudioPath}\n");
            fileContent.Append($"EnableScrollbarLockDefault={DefaultScrollLock}\n");
            fileContent.Append($"DisplayTimestamps={DisplayTimestamps}\n");
            File.WriteAllText(ConfigFile, fileContent.ToString());
        }
    }
}
