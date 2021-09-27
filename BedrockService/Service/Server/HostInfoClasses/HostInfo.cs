using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BedrockService.Service.Server.HostInfoClasses
{
    public partial class HostInfo
    {
        public string HostName { get; set; }
        public string Address { get; set; }
        public string HostDisplayName { get; set; }
        public string ServerVersion { get; set; }

        public List<string> ServiceLog = new List<string>();
        public List<ServerInfo> Servers = new List<ServerInfo>();
        public List<Property> Globals = new List<Property>();

        public void SetGlobalsDefault()
        {
            Globals.Clear();
            Globals.Add(new Property("ServersPath", @"C:\Program Files (x86)\Minecraft Bedrock Server Launcher\Servers"));
            Globals.Add(new Property("AcceptedMojangLic", "false"));
            Globals.Add(new Property("ClientPort", "19134"));
            Globals.Add(new Property("BackupEnabled", "false"));
            Globals.Add(new Property("BackupPath", "Default"));
            Globals.Add(new Property("BackupCron", "0 1 * * *"));
            Globals.Add(new Property("MaxBackupCount", "25"));
            Globals.Add(new Property("EntireBackups", "false"));
            Globals.Add(new Property("CheckUpdates", "false"));
            Globals.Add(new Property("UpdateCron", "0 2 * * *"));
            Globals.Add(new Property("LogServersToFile", "true"));
            Globals.Add(new Property("LogServiceToFile", "true"));
            ServerVersion = "None";
        }

        public bool SetGlobalProperty(string name, string entry)
        {
            try
            {
                Property GlobalToEdit = Globals.FirstOrDefault(glob => glob.KeyName == name);
                Globals[Globals.IndexOf(GlobalToEdit)].Value = entry;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetGlobalValue(string key) => Globals.FirstOrDefault(prop => prop.KeyName == key).Value;

        public void SetGlobalProperty(Property propToSet)
        {
            Property GlobalToEdit = Globals.FirstOrDefault(glob => glob.KeyName == propToSet.KeyName);
            Globals[Globals.IndexOf(GlobalToEdit)].Value = propToSet.Value;
        }

        public void SetGlobalPropertyDefault(string name)
        {
            Property GlobalToEdit = Globals.FirstOrDefault(glob => glob.KeyName == name);
            Globals[Globals.IndexOf(GlobalToEdit)].Value = GlobalToEdit.DefaultValue;
        }

        public string ServiceLogToString()
        {
            StringBuilder OutString = new StringBuilder();
            foreach (string s in ServiceLog)
            {
                OutString.Append(s);
            }
            return OutString.ToString();
        }

        public void SetGlobalPropertyDefault(Property propToSet)
        {
            Property GlobalToEdit = Globals.FirstOrDefault(glob => glob.KeyName == propToSet.KeyName);
            Globals[Globals.IndexOf(GlobalToEdit)].Value = GlobalToEdit.DefaultValue;
        }

        public ServerInfo GetServerInfo(string serverName)
        {
            return Servers.FirstOrDefault(srv => srv.ServerName == serverName);
        }

        public List<ServerInfo> GetServerInfos() => Servers;

        public ServerInfo GetServerInfoByIndex(byte serverIndex) => serverIndex != 0xFF ? Servers[serverIndex] : null;

        public void ClearServerInfos() => Servers = new List<ServerInfo>();

        public List<Property> GetGlobals() => Globals;

        public void SetGlobals(List<Property> props) => Globals = props;
    }
}
