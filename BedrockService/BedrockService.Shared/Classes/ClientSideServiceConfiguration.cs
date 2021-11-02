using BedrockService.Shared.Interfaces;

namespace BedrockService.Shared.Classes
{
    public class ClientSideServiceConfiguration : IClientSideServiceConfiguration
    {
        private readonly string hostName;
        private readonly string address;
        private readonly string port;
        private readonly string displayName;

        public ClientSideServiceConfiguration(string hostName, string address, string port)
        {
            this.hostName = hostName;
            this.address = address;
            this.port = port;
            displayName = $@"{hostName}@{address}:{port}";
        }

        public string GetHostName() => hostName;

        public string GetAddress() => address;

        public string GetPort() => port;

        public string GetDisplayName() => displayName;
    }
}
