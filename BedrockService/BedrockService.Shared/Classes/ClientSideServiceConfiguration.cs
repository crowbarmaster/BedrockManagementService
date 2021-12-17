using BedrockService.Shared.Interfaces;

namespace BedrockService.Shared.Classes {
    public class ClientSideServiceConfiguration : IClientSideServiceConfiguration {
        private readonly string _hostName;
        private readonly string _address;
        private readonly string _port;
        private readonly string _displayName;

        public ClientSideServiceConfiguration(string hostName, string address, string port) {
            this._hostName = hostName;
            this._address = address;
            this._port = port;
            _displayName = $@"{hostName}@{address}:{port}";
        }

        public string GetHostName() => _hostName;

        public string GetAddress() => _address;

        public string GetPort() => _port;

        public string GetDisplayName() => _displayName;
    }
}
