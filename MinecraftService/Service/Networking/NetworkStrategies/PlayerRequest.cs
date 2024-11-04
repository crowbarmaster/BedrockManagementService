using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class PlayerRequest : IMessageParser {

        private readonly IMinecraftService _service;

        public PlayerRequest(IMinecraftService service) {

            _service = service;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_service.GetServerByIndex(serverIndex).GetPlayerManager().GetPlayerList(), Formatting.Indented, SharedStringBase.GlobalJsonSerialierSettings));
            return (serializeToBytes, serverIndex, NetworkMessageTypes.PlayersRequest);
        }
    }
}

