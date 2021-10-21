using BedrockService.Shared;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BedrockService.Client.Management
{
    class ConfigManager
    {
        public static string ConfigDir = $@"{Directory.GetCurrentDirectory()}\Client\Configs";
        public static string ConfigFile = $@"{ConfigDir}\Config.conf";
        public static List<IClientSideServiceConfiguration> HostConnectList = new List<IClientSideServiceConfiguration>();
        public static string NBTStudioPath = "";

        public static void LoadConfigs()
        {
            if (!Directory.Exists(ConfigDir))
            {
                Directory.CreateDirectory(ConfigDir);
            }
            if (!File.Exists(ConfigFile))
            {
                Console.WriteLine("Config file missing! Regenerating default file...");
                CreateDefaultConfig();
                LoadConfigs();
                return;
            }
            string[] lines = File.ReadAllLines(ConfigFile);
            foreach (string line in lines)
            {
                string[] entrySplit = line.Split('=');
                if (!string.IsNullOrEmpty(line) && !line.StartsWith("#"))
                {
                    if (entrySplit[0] == "HostEntry")
                    {
                        string[] hostSplit = entrySplit[1].Split(';');
                        string[] addressSplit = hostSplit[1].Split(':');
                        IClientSideServiceConfiguration hostToList = new ClientSideServiceConfiguration(hostSplit[0], addressSplit[0], addressSplit[1], $@"Host {entrySplit[0]} @ {addressSplit[0]}:{addressSplit[1]}");
                        HostConnectList.Add(hostToList);
                    }
                    if (entrySplit[0] == "NBTStudioPath")
                    {
                        NBTStudioPath = entrySplit[1];
                    }
                }
            }
        }

        public static void CreateDefaultConfig()
        {
            string[] Config = new string[]
            {
                "HostEntry=host1;127.0.0.1:19134"
            };
            StringBuilder builder = new StringBuilder();
            builder.Append("# Hosts\n");
            foreach (string entry in Config)
            {
                builder.Append($"{entry}\n");
            }
            builder.Append("\n# Settings\n");
            builder.Append("NBTStudioPath=\n");
            File.WriteAllText(ConfigFile, builder.ToString());
        }

        public static void SaveConfigFile()
        {
            StringBuilder fileContent = new StringBuilder();
            fileContent.Append("# hosts\n\n");
            foreach (ClientSideServiceConfiguration host in HostConnectList)
            {
                fileContent.Append($"HostEntry={host.GetHostName()};{host.GetAddress()}:{host.GetPort()}\n");
            }
            fileContent.Append("\n# Settings\n");
            fileContent.Append($"NBTStudioPath={NBTStudioPath}");
            File.WriteAllText(ConfigFile, fileContent.ToString());
        }
    }
}
