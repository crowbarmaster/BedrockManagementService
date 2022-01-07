using BedrockService.Service.Networking.Interfaces;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class CheckUpdates : IMessageParser {

        private readonly IUpdater _updater;

        public CheckUpdates(IUpdater updater) {
            _updater = updater;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            _updater.CheckUpdates().Wait();
            if (_updater.CheckVersionChanged()) {
                return (Array.Empty<byte>(), 0, NetworkMessageTypes.CheckUpdates);
            }
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}