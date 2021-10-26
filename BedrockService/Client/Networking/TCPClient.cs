using BedrockService.Client.Management;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using BedrockService.Shared.PackParser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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
        public bool EnumBackupsArrived;
        public List<Property> BackupList;
        public List<MinecraftPackParser> RecievedPacks;
        public Thread ClientReciever;
        public Thread HeartbeatThread;
        private int heartbeatFailTimeout;
        private int heartbeatFailTimeoutLimit = 250;
        private bool heartbeatRecieved;
        private bool keepAlive;

        public bool ConnectHost(IClientSideServiceConfiguration host)
        {
            if (EstablishConnection(host.GetAddress(), int.Parse(host.GetPort())))
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
                keepAlive = true;
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
            return keepAlive;
        }

        public void CloseConnection()
        {
            try
            {
                if (stream != null)
                    stream.Dispose();
                stream = null;
                Connected = false;
                keepAlive = false;
            }
            catch (NullReferenceException)
            {
                Connected = false;
                keepAlive = false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error closing connection: {e.StackTrace}");
            }
        }

        public void ReceiveListener()
        {
            List<byte[]> byteBlocks = new List<byte[]>();
            while (keepAlive)
            {
                try
                {
                    byte[] buffer = new byte[4];
                    while (OpenedTcpClient.Client.Available > 0)
                    {
                        int byteCount = stream.Read(buffer, 0, 4);
                        int expectedLen = BitConverter.ToInt32(buffer, 0);
                        buffer = new byte[expectedLen];
                        byteCount = stream.Read(buffer, 0, expectedLen);
                        NetworkMessageSource source = (NetworkMessageSource)buffer[0];
                        NetworkMessageDestination destination = (NetworkMessageDestination)buffer[1];
                        byte serverIndex = buffer[2];
                        NetworkMessageTypes msgType = (NetworkMessageTypes)buffer[3];
                        NetworkMessageFlags msgStatus = (NetworkMessageFlags)buffer[4];
                        string data = "";
                        if (msgType != NetworkMessageTypes.PackFile || msgType != NetworkMessageTypes.LevelEditFile)
                            data = GetOffsetString(buffer);
                        if (destination != NetworkMessageDestination.Client)
                            continue;
                        int srvCurLen = 0;
                        JsonSerializerSettings settings = new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.All
                        };
                        switch (source)
                        {
                            case NetworkMessageSource.Service:
                                switch (msgType)
                                {
                                    case NetworkMessageTypes.Connect:
                                        try
                                        {
                                            Console.WriteLine("Connection to Host successful!");
                                            FormManager.GetMainWindow.connectedHost = null;
                                            FormManager.GetMainWindow.connectedHost = JsonConvert.DeserializeObject<IServiceConfiguration>(data, settings);
                                            Connected = true;
                                            FormManager.GetMainWindow.RefreshServerContents();
                                            heartbeatFailTimeout = 0;
                                            if (HeartbeatThread == null || !HeartbeatThread.IsAlive)
                                                HeartbeatThread = new Thread(new ThreadStart(SendHeatbeatSignal))
                                                {
                                                    IsBackground = true,
                                                    Name = "HeartBeatThread"
                                                };
                                            HeartbeatThread.Start();

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

                                    case NetworkMessageTypes.EnumBackups:

                                        BackupList = JsonConvert.DeserializeObject<List<Property>>(data, settings);
                                        EnumBackupsArrived = true;

                                        break;
                                    case NetworkMessageTypes.CheckUpdates:

                                        //TODO: Ask user if update now or perform later.
                                        UnlockUI();

                                        break;
                                    case NetworkMessageTypes.UICallback:

                                        UnlockUI();

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
                                                IServerConfiguration bedrockServer = FormManager.GetMainWindow.connectedHost.GetServerInfoByName(srvName);
                                                int curCount = bedrockServer.GetLog().Count;
                                                if (curCount == srvCurLen)
                                                {
                                                    bedrockServer.GetLog().Add(srvText);
                                                }
                                            }
                                            else
                                            {
                                                int curCount = FormManager.GetMainWindow.connectedHost.GetLog().Count;
                                                if (curCount == srvCurLen)
                                                {
                                                    FormManager.GetMainWindow.connectedHost.GetLog().Add(srvText);
                                                }
                                            }
                                        }
                                        break;
                                    case NetworkMessageTypes.Backup:

                                        Console.WriteLine(msgStatus.ToString());

                                        break;
                                    case NetworkMessageTypes.UICallback:

                                        UnlockUI();

                                        break;
                                    case NetworkMessageTypes.PackList:

                                        List<MinecraftPackParser> temp = new List<MinecraftPackParser>();
                                        JArray jArray = JArray.Parse(data);
                                        foreach (JToken token in jArray)
                                            temp.Add(token.ToObject<MinecraftPackParser>());
                                        RecievedPacks = temp;

                                        break;
                                    case NetworkMessageTypes.PlayersRequest:

                                        List<IPlayer> fetchedPlayers = JsonConvert.DeserializeObject<List<IPlayer>>(data, settings);
                                        FormManager.GetMainWindow.connectedHost.GetServerInfoByIndex(serverIndex).SetPlayerList(fetchedPlayers);
                                        PlayerInfoArrived = true;

                                        break;
                                    case NetworkMessageTypes.LevelEditFile:

                                        byte[] stripHeaderFromBuffer = new byte[buffer.Length - 5];
                                        Buffer.BlockCopy(buffer, 5, stripHeaderFromBuffer, 0, stripHeaderFromBuffer.Length);
                                        string pathToLevelDat = $@"{Path.GetTempPath()}\level.dat";
                                        File.WriteAllBytes(pathToLevelDat, stripHeaderFromBuffer);
                                        UnlockUI();

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

        public void SendHeatbeatSignal()
        {
            Console.WriteLine("HeartbeatThread started.");
            while (keepAlive)
            {
                heartbeatRecieved = false;
                SendData(NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.Heartbeat);
                while (!heartbeatRecieved && keepAlive)
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

        public bool SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type, NetworkMessageFlags status)
        {
            byte[] compiled = new byte[9 + bytes.Length];
            byte[] len = BitConverter.GetBytes(5 + bytes.Length);
            Buffer.BlockCopy(len, 0, compiled, 0, 4);
            compiled[4] = (byte)source;
            compiled[5] = (byte)destination;
            compiled[6] = serverIndex;
            compiled[7] = (byte)type;
            compiled[8] = (byte)status;
            Buffer.BlockCopy(bytes, 0, compiled, 9, bytes.Length);
            if (keepAlive)
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

        public void SendData(NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type) => SendData(new byte[0], source, destination, 0xFF, type, NetworkMessageFlags.None);

        public void SendData(NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type) => SendData(new byte[0], source, destination, serverIndex, type, NetworkMessageFlags.None);

        public void SendData(NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type, NetworkMessageFlags flag) => SendData(new byte[0], source, destination, serverIndex, type, flag);

        public void SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type) => SendData(bytes, source, destination, serverIndex, type, NetworkMessageFlags.None);

        public void SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type) => SendData(bytes, source, destination, 0xFF, type, NetworkMessageFlags.None);

        public void SendData(NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type, NetworkMessageFlags status) => SendData(new byte[0], source, destination, 0xFF, type, status);

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

        private string GetOffsetString(byte[] array) => Encoding.UTF8.GetString(array, 5, array.Length - 5);

        private void UnlockUI() => FormManager.GetMainWindow.ServerBusy = false;
    }
}
