using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftService.Client.Networking {
    internal interface INetworkMessage {
        Task<bool> ProcessMessage(byte[] messageData);
    }
}
