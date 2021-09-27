using BedrockService.Service.Logging;
using BedrockService.Service.Management;
using BedrockService.Service.Server;
using BedrockService.Service.Server.HostInfoClasses;
using BedrockService.Service.Server.Management;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BedrockService.Service
{
    public static class InstanceProvider
    {
        private static BedrockService bedrockService;
        private static ConfigManager configManager;
        private static ServiceLogger serviceLogger;
        private static Thread clientservice;
        private static Thread heartbeatThread;
        private static TcpListener tcpListener;
        private static IPAddress storedAddress;
        private static HostInfo hostInfo;
        private static PlayerManager playerManager;
        private static int storedPort;
        public static BedrockService BedrockService
        {
            get
            {
                if (bedrockService == null)
                {
                    bedrockService = new BedrockService();
                }
                return bedrockService;
            }
        }
        public static HostInfo HostInfo
        {
            get
            {
                if (hostInfo == null)
                {
                    hostInfo = new HostInfo();
                }
                return hostInfo;
            }
        }
        public static PlayerManager PlayerManager
        {
            get
            {
                if (playerManager == null)
                {
                    playerManager = new PlayerManager();
                }
                return playerManager;
            }
        }
        public static ConfigManager ConfigManager
        {
            get
            {
                if (configManager == null)
                {
                    configManager = new ConfigManager();
                }
                return configManager;
            }
        }
        public static ServiceLogger ServiceLogger
        {
            get
            {
                if (serviceLogger == null)
                {
                    serviceLogger = new ServiceLogger(true);
                }
                return serviceLogger;
            }
        }
        public static Thread ClientService
        {
            get
            {
                return clientservice;
            }
        }

        public static Thread InitClientService(ThreadStart threadStart)
        {
            if (clientservice == null || clientservice.ThreadState == ThreadState.Stopped || clientservice.ThreadState == ThreadState.Aborted)
            {
                clientservice = new Thread(threadStart)
                {
                    Name = "ClientService",
                    IsBackground = true
                };
            }
            return clientservice;
        }

        public static Thread InitHeartbeatThread(ThreadStart threadStart)
        {
            if (heartbeatThread == null || heartbeatThread.ThreadState == ThreadState.Stopped || heartbeatThread.ThreadState == ThreadState.Aborted)
            {
                heartbeatThread = new Thread(threadStart)
                {
                    Name = "HeartbeatThread",
                    IsBackground = true
                };
            }
            return heartbeatThread;
        }

        public static TcpListener InitTCPListener(IPAddress address, int port)
        {
            storedAddress = address;
            storedPort = port;
            tcpListener = null;
            tcpListener = new TcpListener(storedAddress, storedPort);
            return tcpListener;
        }

        public static BedrockServer GetBedrockServerByName(string serverName) => BedrockService.bedrockServers.First(srv => srv.serverInfo.ServerName == serverName);

        public static BedrockServer GetBedrockServerByIndex(byte serverIndex) => serverIndex != 0xFF ? BedrockService.bedrockServers[serverIndex] : null;

        public static ServerInfo GetServerInfoByIndex(byte index) => HostInfo.GetServerInfoByIndex(index);

        public static bool GetClientServiceAlive() => clientservice != null && clientservice.IsAlive;

        public static bool GetHeartbeatThreadAlive() => heartbeatThread != null && heartbeatThread.IsAlive;

        public static void DisposeClientService()
        {
            if (clientservice != null)
            {
                if (clientservice.IsAlive)
                {
                    clientservice.Abort();
                }
                clientservice = null;
            }
        }

        public static void DisposeHeartbeatThread()
        {
            if (heartbeatThread != null)
            {
                if (heartbeatThread.IsAlive)
                {
                    heartbeatThread.Abort();
                }
                heartbeatThread = null;
            }
        }
    }
}
