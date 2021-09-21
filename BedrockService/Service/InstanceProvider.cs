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

        public static BedrockService GetBedrockService()
        {
            if (bedrockService == null)
            {
                bedrockService = new BedrockService();
            }
            return bedrockService;
        }

        public static BedrockServer GetBedrockServer(string serverName)
        {
            BedrockServer serverFound = GetBedrockService().bedrockServers.First(srv => srv.serverInfo.ServerName == serverName);
            return serverFound;
        }

        public static HostInfo GetHostInfo()
        {
            if (hostInfo == null)
            {
                hostInfo = new HostInfo();
            }
            return hostInfo;
        }

        public static PlayerManager GetPlayerManager()
        {
            if (playerManager == null)
            {
                playerManager = new PlayerManager();
            }
            return playerManager;
        }

        public static ConfigManager GetConfigManager()
        {
            if (configManager == null)
            {
                configManager = new ConfigManager();
            }
            return configManager;
        }

        public static ServiceLogger GetServiceLogger()
        {
            if (serviceLogger == null)
            {
                serviceLogger = new ServiceLogger(true);
            }
            return serviceLogger;
        }

        public static TcpListener InitTCPListener(IPAddress address, int port)
        {
            storedAddress = address;
            storedPort = port;
            tcpListener = null;
            tcpListener = new TcpListener(storedAddress, storedPort);
            return tcpListener;
        }

        public static TcpListener GetTcpListener()
        {
            if (tcpListener != null)
                return tcpListener;
            return null;
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

        public static Thread GetClientService() => clientservice;

        public static bool GetClientServiceAlive() => clientservice != null && clientservice.IsAlive;

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

        public static Thread GetHeartbeatThread() => heartbeatThread;

        public static bool GetHeartbeatThreadAlive() => heartbeatThread != null && heartbeatThread.IsAlive;


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
