using BedrockService.Service.Server.HostInfoClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BedrockService.Client.Management
{
    class ConfigManager
    {
        public static string ConfigDir = $@"{Directory.GetCurrentDirectory()}\Client\Configs";
        public static string ConfigFile = $@"{ConfigDir}\Config.conf";
        public static List<HostInfo> HostConnectList = new List<HostInfo>();

        public static void LoadConfigs()
        {
            if (!Directory.Exists(ConfigDir))
            {
                Directory.CreateDirectory(ConfigDir);
            }
            string[] files = Directory.GetFiles(ConfigDir, "*.conf");

            if (files.Length > 0)
            {
                foreach (string file in files)
                {
                    string[] lines = File.ReadAllLines(file);
                    foreach (string line in lines)
                    {
                        string[] entrySplit = line.Split('=');
                        if (!string.IsNullOrEmpty(line) && !line.StartsWith("#") && entrySplit[0] == "HostEntry")
                        {
                                HostInfo hostToList = new HostInfo();
                                hostToList.SetGlobalsDefault();
                                string[] hostSplit = entrySplit[1].Split(';');
                                string[] addressSplit = hostSplit[1].Split(':');
                                hostToList.HostName = hostSplit[0];
                                hostToList.Address = addressSplit[0];
                                hostToList.SetGlobalProperty("ClientPort", addressSplit[1]);
                                hostToList.HostDisplayName = $@"Host {entrySplit[0]} @ {addressSplit[0]}:{addressSplit[1]}";
                                HostConnectList.Add(hostToList);
                            
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Config file missing! Regenerating default file...");
                CreateDefaultConfig();
                LoadConfigs();
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

        public static int[] GetPorts(string input)
        {
            string[] StrArr = input.Split(';');
            int[] Output = new int[StrArr.Length];

            for (int i = 0; i < StrArr.Length; i++)
            {
                Output[i] = Convert.ToInt32(StrArr[i]);
            }
            return Output;
        }
    }
}
