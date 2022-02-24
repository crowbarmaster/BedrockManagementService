using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum ServiceStatus {
    Stopped,
    Starting,
    Stopping,
    Started
}

public enum ServerStatus {
    Stopped,
    Starting,
    Stopping,
    Started
}

namespace BedrockService.Shared.Classes {
    public class StatusUpdateModel {
        public ServiceStatusModel ServiceStatusModel { get; set; }
        public ServerStatusModel ServerStatusModel { get; set; }
    }

    public class ServiceStatusModel {
        public DateTime ServiceUptime { get; set; }
        public ServiceStatus ServiceStatus { get; set; }
        public List<IPlayer> ActivePlayerList { get; set; } = new List<IPlayer>();
        public int TotalBackups { get; set; }
        public int TotalBackupSize { get; set; }
        public string LatestVersion { get; set; }
    }

    public class ServerStatusModel {
        public DateTime ServerUptime { get; set; }
        public byte ServerIndex { get; set; }
        public ServerStatus ServerStatus { get; set; }
        public List<IPlayer> ActivePlayerList { get; set; } = new List<IPlayer>();
        public int TotalBackups { get; set; }
        public int TotalSizeOfBackups { get; set; }
        public string DeployedVersion { get; set; }

        public override bool Equals(object obj) {
            return obj is ServerStatusModel model &&
                   ServerStatus == model.ServerStatus &&
                   ServerIndex == model.ServerIndex &&
                   TotalBackups == model.TotalBackups &&
                   TotalSizeOfBackups == model.TotalSizeOfBackups &&
                   DeployedVersion == model.DeployedVersion;
        }

        public override int GetHashCode() {
            return HashCode.Combine(ServerStatus, TotalBackups, ServerIndex, TotalSizeOfBackups, DeployedVersion);
        }
    }
}
