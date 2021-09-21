using BedrockService.Client.Forms;
using BedrockService.Client.Management;
using BedrockService.Service.Networking;
using BedrockService.Service.Server.HostInfoClasses;
using BedrockService.Service.Server.Logging;
using BedrockService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BedrockService.Client.Networking
{
    public class TCPClient
    {
        public TcpClient OpenedTcpClient;
        public string ClientName;
        public NetworkStream stream;
        public bool Connected;
        public bool EnableRead;
        public bool PlayerInfoArrived;
        public Thread ClientReciever;
        public Thread HeartbeatThread;
        private int heartbeatFailTimeout;
        private int heartbeatFailTimeoutLimit = 200;
        private bool heartbeatRecieved;

        public bool ConnectHost(HostInfo host)
        {
            if (EstablishConnection(host.Address, int.Parse(host.GetGlobalValue("ClientPort"))))
            {
                SendData(NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.Connect);
                return true;
            }
            return false;
        }

        public bool EstablishConnection(string addr, int port)
        {
            Console.WriteLine("Connecting to Server");
            try
            {
                EnableRead = false;
                OpenedTcpClient = new TcpClient(addr, port);
                stream = OpenedTcpClient.GetStream();
                Connected = true;
                ClientReciever = new Thread(new ThreadStart(ReceiveListener));
                ClientReciever.Name = "ClientPacketReviever";
                ClientReciever.IsBackground = true;
                ClientReciever.Start();
            }
            catch
            {
                Console.WriteLine("Could not connect to Server");
                return false;
            }
            return Connected;
        }

        public void CloseConnection()
        {
            try
            {
                stream.Dispose();
                stream = null;
                Connected = false;
            }
            catch (NullReferenceException)
            {
                Connected = false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error closing connection: {e.StackTrace}");
            }
        }

        public void ReceiveListener()
        {
            while (Connected)
            {
                try
                {
                    byte[] buffer = new byte[4];
                    while (OpenedTcpClient != null && OpenedTcpClient.Client.Available != 0)
                    {
                        int byteCount = stream.Read(buffer, 0, 4);
                        int expectedLen = BitConverter.ToInt32(buffer, 0);
                        buffer = new byte[expectedLen];
                        byteCount = stream.Read(buffer, 0, expectedLen);
                        NetworkMessageSource source = (NetworkMessageSource)buffer[0];
                        NetworkMessageDestination destination = (NetworkMessageDestination)buffer[1];
                        NetworkMessageTypes msgType = (NetworkMessageTypes)buffer[2];
                        NetworkMessageStatus msgStatus = (NetworkMessageStatus)buffer[3];
                        string data = GetString(buffer);
                        if (destination != NetworkMessageDestination.Client)
                            continue;
                        int srvCurLen = 0;
                        switch (source)
                        {
                            case NetworkMessageSource.Service:
                                switch (msgType)
                                {
                                    case NetworkMessageTypes.Connect:
                                        try
                                        {
                                            JsonParser message = JsonParser.Deserialize(data);
                                            if (message.Type == typeof(HostInfo))
                                            {
                                                Console.WriteLine($"{message.Value}");
                                                Console.WriteLine("Connection to Host successful!");
                                                MainWindow.connectedHost = message.Value.ToObject<HostInfo>();
                                                heartbeatFailTimeout = 0;
                                                HeartbeatThread = new Thread(new ThreadStart(SendHeatbeatSignal))
                                                {
                                                    IsBackground = true,
                                                    Name = "HeartBeatThread"
                                                };
                                                HeartbeatThread.Start();
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine($"Error: ConnectMan reported error: {e.Message}\n{e.StackTrace}");
                                        }
                                        break;
                                    case NetworkMessageTypes.Heartbeat:
                                        heartbeatRecieved = true;
                                        if (!HeartbeatThread.IsAlive)
                                        {
                                            HeartbeatThread = new Thread(new ThreadStart(SendHeatbeatSignal));
                                            HeartbeatThread.IsBackground = true;
                                            HeartbeatThread.Name = "HeartBeatThread";
                                            HeartbeatThread.Start();
                                            Thread.Sleep(500);
                                        }
                                        break;
                                }
                                break;
                            case NetworkMessageSource.Server:
                                switch (msgType)
                                {
                                    case NetworkMessageTypes.ConsoleLogUpdate:
                                        string[] strings = data.Split('|');
                                        for (int i = 0; i < strings.Length; i++)
                                        {
                                            string[] srvSplit = strings[i].Split(';');
                                            string srvName = srvSplit[0];
                                            string srvText = srvSplit[1];
                                            srvCurLen = int.Parse(srvSplit[2]);
                                            if (srvName != "Service")
                                            {
                                                ServerInfo bedrockServer = MainWindow.connectedHost.GetServerInfos().First(srv => srv.ServerName == srvName);
                                                bedrockServer.ConsoleBuffer = bedrockServer.ConsoleBuffer ?? new ServerLogger(bedrockServer.ServerName);
                                                int curCount = bedrockServer.ConsoleBuffer.Count();
                                                if (curCount == srvCurLen)
                                                {
                                                    bedrockServer.ConsoleBuffer.Append(srvText);
                                                }
                                            }
                                            else
                                            {
                                                int curCount = MainWindow.connectedHost.ServiceLog.Count;
                                                if (curCount == srvCurLen)
                                                {
                                                    MainWindow.connectedHost.ServiceLog.Add(srvText);
                                                }
                                            }
                                        }
                                        break;
                                    case NetworkMessageTypes.Backup:

                                        Console.WriteLine(msgStatus.ToString());

                                        break;
                                    case NetworkMessageTypes.PlayersRequest:

                                        string[] dataSplit = data.Split(';');
                                        JsonParser deserialized = JsonParser.Deserialize(dataSplit[1]);
                                        List<Player> fetchedPlayers = (List<Player>)deserialized.Value.ToObject(typeof(List<Player>));
                                        MainWindow.connectedHost.GetServerInfos().First(srv => srv.ServerName == dataSplit[0]).KnownPlayers = fetchedPlayers;
                                        PlayerInfoArrived = true;

                                        break;
                                }
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"TCPClient error! Stacktrace: {e.Message}\n{e.StackTrace}");
                }
                Thread.Sleep(200);
            }
        }

        private string GetString(byte[] array) => Encoding.UTF8.GetString(array, 4, array.Length - 4);

        public void SendHeatbeatSignal()
        {
            Console.WriteLine("HeartbeatThread started.");
            while (Connected)
            {
                heartbeatRecieved = false;
                SendData(NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.Heartbeat);
                while (!heartbeatRecieved)
                {
                    Thread.Sleep(100);
                    heartbeatFailTimeout++;
                    if (heartbeatFailTimeout > heartbeatFailTimeoutLimit)
                    {
                        FormManager.GetMainWindow.HeartbeatFailDisconnect();
                        HeartbeatThread.Abort();
                        heartbeatFailTimeout = 0;
                    }
                }
                // Console.WriteLine("ThumpThump");
                heartbeatRecieved = false;
                heartbeatFailTimeout = 0;
                Thread.Sleep(3000);
            }
        }

        public bool SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type, NetworkMessageStatus status)
        {
            byte[] compiled = new byte[8 + bytes.Length];
            byte[] len = BitConverter.GetBytes(4 + bytes.Length);
            Buffer.BlockCopy(len, 0, compiled, 0, 4);
            compiled[4] = (byte)source;
            compiled[5] = (byte)destination;
            compiled[6] = (byte)type;
            compiled[7] = (byte)status;
            Buffer.BlockCopy(bytes, 0, compiled, 8, bytes.Length);
            if (Connected)
            {
                try
                {
                    stream.Write(compiled, 0, compiled.Length);
                    stream.Flush();
                    return true;

                }
                catch
                {
                    Console.WriteLine("Error writing to network stream!");
                    return false;
                }
            }
            return false;
        }

        public bool SendData(NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type)
        {
            if (SendData(new byte[0], source, destination, type, NetworkMessageStatus.None))
                return true;
            return false;
        }

        public bool SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type)
        {
            if (SendData(bytes, source, destination, type, NetworkMessageStatus.None))
                return true;
            return false;
        }

        public bool SendData(NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type, NetworkMessageStatus status)
        {
            if (SendData(new byte[0], source, destination, type, status))
                return true;
            return false;
        }

        public void Dispose()
        {
            if (HeartbeatThread != null && HeartbeatThread.IsAlive)
                HeartbeatThread.Abort();
            if (ClientReciever != null && ClientReciever.IsAlive)
                ClientReciever.Abort();
            HeartbeatThread = null;
            ClientReciever = null;
            if (OpenedTcpClient != null)
            {
                OpenedTcpClient.Close();
                OpenedTcpClient.Dispose();
            }

        }
    }
}
