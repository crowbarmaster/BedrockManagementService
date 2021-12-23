using BedrockService.Shared.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace BedrockService.Shared.Classes {
    public class ServiceInfo : IServiceConfiguration {
        private string hostName { get; set; }
        private string address { get; set; }
        private string hostDisplayName { get; set; }
        [JsonProperty]
        private string serverVersion { get; set; }
        [JsonProperty]
        private List<LogEntry> serviceLog = new List<LogEntry>();
        [JsonProperty]
        private List<IServerConfiguration> ServerList = new List<IServerConfiguration>();
        [JsonProperty]
        private List<Property> globals = new List<Property>();
        private readonly IProcessInfo _processInfo;

        public ServiceInfo(IProcessInfo processInfo) {
            _processInfo = processInfo;
        }

        public void InitializeDefaults() {
            globals.Clear();
            globals.Add(new Property("ServersPath", @"C:\MCBedrockService"));
            globals.Add(new Property("AcceptedMojangLic", "false"));
            globals.Add(new Property("ClientPort", "19134"));
            globals.Add(new Property("BackupEnabled", "false"));
            globals.Add(new Property("BackupPath", "Default"));
            globals.Add(new Property("BackupCron", "0 1 * * *"));
            globals.Add(new Property("MaxBackupCount", "25"));
            globals.Add(new Property("EntireBackups", "false"));
            globals.Add(new Property("CheckUpdates", "false"));
            globals.Add(new Property("UpdateCron", "0 2 * * *"));
            globals.Add(new Property("LogServerOutput", "true"));
            globals.Add(new Property("LogApplicationOutput", "true"));
        }

        public void ProcessConfiguration(string[] fileEntries) {
            foreach (string line in fileEntries) {
                if (!line.StartsWith("#") && !string.IsNullOrEmpty(line)) {
                    string[] split = line.Split('=');
                    if (split.Length == 1) {
                        split[1] = "";
                    }
                    if (split[0] == "BackupPath") {
                        if (split[1] == "Default")
                            split[1] = $@"{_processInfo.GetDirectory()}\Server\Backups";
                    }
                    if (!SetProp(split[0], split[1])) {
                        //Logger.AppendLine($"Error! Key \"{split[0]}\" was not found! Check configs!");
                    }
                }
            }
        }

        public bool SetProp(string name, string newValue) {
            try {
                Property GlobalToEdit = globals.First(glob => glob.KeyName == name);
                globals[globals.IndexOf(GlobalToEdit)].SetValue(newValue);
                return true;
            }
            catch {
                // handle soon.
                return false;
            }
        }

        public bool SetProp(Property propToSet) {
            try {
                Property GlobalToEdit = globals.First(glob => glob.KeyName == propToSet.KeyName);
                globals[globals.IndexOf(GlobalToEdit)] = propToSet;
            }
            catch {
                // handle soon.
                return false;
            }
            return true;
        }

        public Property GetProp(string name) {
            return globals.FirstOrDefault(prop => prop.KeyName == name);
        }

        public List<Property> GetAllProps() => globals;

        public void SetAllProps(List<Property> props) => globals = props;

        public IServerConfiguration GetServerInfoByName(string serverName) {
            return ServerList.FirstOrDefault(info => info.GetServerName() == serverName);
        }

        public IServerConfiguration GetServerInfoByIndex(int index) {
            return ServerList[index];
        }

        public void RemoveServerInfo(IServerConfiguration serverConfiguration) {
            ServerList.Remove(serverConfiguration.GetServerInfo());
        }

        public void SetServerInfo(IServerConfiguration newInfo) {
            ServerList[ServerList.IndexOf(newInfo.GetServerInfo())] = newInfo.GetServerInfo();
        }

        public List<IServerConfiguration> GetServerList() => ServerList;

        public void SetAllServerInfos(List<IServerConfiguration> newInfos) {
            ServerList = newInfos;
        }

        public void AddNewServerInfo(IServerConfiguration serverConfiguration) {
            ServerList.Add(serverConfiguration);
        }

        public void RemoveServerInfoByIndex(int serverIndex) {
            ServerList.RemoveAt(serverIndex);
        }

        public void SetServerVersion(string newVersion) => serverVersion = newVersion;

        public string GetServerVersion() => serverVersion;

        public List<LogEntry> GetLog() => serviceLog ?? new List<LogEntry>();

        public void SetLog(List<LogEntry> newLog) => serviceLog = newLog;

        public byte GetServerIndex(IServerConfiguration server) => (byte)ServerList.IndexOf(server);
    }
}