using MinecraftService.Shared.Interfaces;

namespace MinecraftService.Shared.Classes.Service.Configuration
{
    public class ClientSideServiceConfiguration
    {
        private readonly string _hostName;
        private readonly string _address;
        private readonly string _port;
        private readonly string _displayName;

        public ClientSideServiceConfiguration(string hostName, string address, string port)
        {
            _hostName = hostName;
            _address = address;
            _port = port;
            _displayName = $@"{hostName}@{address}:{port}";
        }

        public string GetHostName() => _hostName;

        public string GetAddress() => _address;

        public string GetPort() => _port;

        public string GetDisplayName() => _displayName;
    }
}
