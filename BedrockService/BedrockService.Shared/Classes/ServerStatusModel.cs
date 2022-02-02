using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum ServerStatus {
    Stopped,
    Starting,
    Stopping,
    Started
}

namespace BedrockService.Shared.Classes {
    public class ServerStatusModel {
        public byte ServerIndex { get; set; }
        public ServerStatus ServerStatus { get; set; }
        public List<IPlayer> ActivePlayerList { get; set; } = new List<IPlayer>();
    }
}
