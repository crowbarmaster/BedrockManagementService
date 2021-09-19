using BedrockService.Service.Logging;
using BedrockService.Service.Server;
using BedrockService.Service.Server.HostInfoClasses;
using BedrockService.Service.Server.Logging;
using BedrockService.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BedrockService.Service.Networking
{
    public class TCPListener
    {
        public TcpClient client;
        private TcpListener InListener;
        private NetworkStream stream;
        private bool keepalive;
        private bool heartbeatRecieved = false;
        private bool firstHeartbeatRecieved = false;
        private int heartbeatFailTimeout;
        private int heartbeatFailTimeoutLimit = 500;

        public void StartListening(int port)
        {
            IPAddress addr = IPAddress.Parse("127.0.0.1");
            InListener = InstanceProvider.InitTCPListener(addr, port);
            try
            {
                InListener.Start();
            }
            catch
            {
                InstanceProvider.GetServiceLogger().AppendLine("Error! Port is occupied and cannot be opened... Program will be killed!");
                Thread.Sleep(2000);
                Environment.Exit(1);
            }

            while (true)
            {
                try
                {
                    client = InListener.AcceptTcpClient();
                    stream = client.GetStream();
                    InstanceProvider.InitClientService(new ThreadStart(IncomingListener)).Start();
                    InstanceProvider.InitHeartbeatThread(new ThreadStart(SendBackHeatbeatSignal)).Start();
                }
                catch (ThreadStateException) { }
                catch (Exception e)
                {
                    InstanceProvider.GetServiceLogger().AppendLine(e.ToString());
                }
            }
            //listener.Stop();
        }
        private void IncomingListener()
        {
            keepalive = true;
            InstanceProvider.GetServiceLogger().AppendLine("Established connection! Listening for incoming packets!");
            int AvailBytes = 0;
            int byteCount = 0;
            while (InstanceProvider.GetClientServiceAlive())
            {
                try
                {
                    byte[] buffer = new byte[4];
                    AvailBytes = client.Client.Available;
                    while (AvailBytes != 0) // Recieve data from client.
                    {
                        byteCount = stream.Read(buffer, 0, 4);
                        int expectedLen = BitConverter.ToInt32(buffer, 0);
                        buffer = new byte[expectedLen];
                        byteCount = stream.Read(buffer, 0, expectedLen);
                        NetworkMessageSource msgSource = (NetworkMessageSource)buffer[0];
                        NetworkMessageDestination msgDest = (NetworkMessageDestination)buffer[1];
                        NetworkMessageTypes msgType = (NetworkMessageTypes)buffer[2];
                        NetworkMessageStatus msgStatus = (NetworkMessageStatus)buffer[3];
                        string data = GetOffsetString(buffer);
                        string[] dataSplit = null;
                        switch (msgDest)
                        {
                            case NetworkMessageDestination.Server:

                                JsonParser message;
                                switch (msgType)
                                {
                                    case NetworkMessageTypes.PropUpdate:

                                        message = JsonParser.Deserialize(data);
                                        List<Property> propList = message.Value.ToObject<List<Property>>();
                                        Property prop = propList.First(p => p.KeyName == "server-name");
                                        InstanceProvider.GetBedrockServer(prop.Value).serverInfo.ServerPropList = propList;
                                        InstanceProvider.GetConfigManager().SaveServerProps(InstanceProvider.GetBedrockServer(prop.Value).serverInfo, true);
                                        InstanceProvider.GetConfigManager().LoadConfigs();
                                        InstanceProvider.GetBedrockServer(prop.Value).CurrentServerStatus = BedrockServer.ServerStatus.Stopping;
                                        while (InstanceProvider.GetBedrockServer(prop.Value).CurrentServerStatus == BedrockServer.ServerStatus.Stopping)
                                        {
                                            Thread.Sleep(100);
                                        }
                                        InstanceProvider.GetBedrockServer(prop.Value).StartControl(InstanceProvider.GetBedrockService()._hostControl);
                                        SendData(NetworkMessageSource.Server, NetworkMessageDestination.Client, NetworkMessageTypes.PropUpdate);

                                        break;
                                    case NetworkMessageTypes.Restart:

                                        RestartServer(data, false);
                                        break;

                                    case NetworkMessageTypes.Backup:

                                        RestartServer(data, true);
                                        break;
                                    case NetworkMessageTypes.Command:

                                        dataSplit = data.Split(';');
                                        InstanceProvider.GetBedrockServer(dataSplit[0]).StdInStream.WriteLine(dataSplit[1]);
                                        InstanceProvider.GetServiceLogger().AppendLine($"Sent command {dataSplit[1]} to stdInput stream");

                                        break;
                                    case NetworkMessageTypes.PlayersRequest:

                                        ServerInfo server = InstanceProvider.GetBedrockServer(data).serverInfo;
                                        string jsonString = $"{data};{JsonParser.Serialize(JsonParser.FromValue(server.KnownPlayers))}";
                                        SendData(Encoding.UTF8.GetBytes(jsonString), NetworkMessageSource.Server, NetworkMessageDestination.Client, NetworkMessageTypes.PlayersRequest);

                                        break;
                                    case NetworkMessageTypes.PlayersUpdate:

                                        dataSplit = data.Split(';');
                                        JsonParser deserialized = JsonParser.Deserialize(dataSplit[1]);
                                        List<Player> fetchedPlayers = (List<Player>)deserialized.Value.ToObject(typeof(List<Player>));
                                        List<Player> known = InstanceProvider.GetBedrockServer(dataSplit[0]).serverInfo.KnownPlayers;
                                        foreach (Player player in fetchedPlayers)
                                        {
                                            try
                                            {
                                                Player playerFound = InstanceProvider.GetBedrockServer(dataSplit[0]).serverInfo.KnownPlayers.First(p => p.XUID == player.XUID);
                                                if (player != playerFound)
                                                    InstanceProvider.GetBedrockServer(dataSplit[0]).serverInfo.KnownPlayers[InstanceProvider.GetBedrockServer(dataSplit[0]).serverInfo.KnownPlayers.IndexOf(playerFound)] = player;
                                            }
                                            catch (Exception)
                                            {
                                                InstanceProvider.GetBedrockServer(dataSplit[0]).serverInfo.KnownPlayers.Add(player);
                                            }
                                        }
                                        InstanceProvider.GetConfigManager().SaveRegisteredPlayers(InstanceProvider.GetBedrockServer(dataSplit[0]).serverInfo);
                                        RestartServer(dataSplit[0], false);

                                        break;
                                }
                                break;
                            case NetworkMessageDestination.Service:
                                switch (msgType)
                                {
                                    case NetworkMessageTypes.Connect:

                                        InstanceProvider.GetHostInfo().ServiceLog = InstanceProvider.GetServiceLogger().Log;
                                        string jsonString = JsonParser.Serialize(JsonParser.FromValue(InstanceProvider.GetHostInfo()));
                                        byte[] stringAsBytes = GetBytes(jsonString);
                                        SendData(stringAsBytes, NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Connect);
                                        heartbeatRecieved = false;

                                        break;
                                    case NetworkMessageTypes.Disconnect:

                                        DisconnectClient();

                                        break;
                                    case NetworkMessageTypes.Heartbeat:

                                        if (InstanceProvider.GetHeartbeatThreadAlive())
                                            heartbeatRecieved = true;
                                        else
                                        {
                                            InstanceProvider.InitHeartbeatThread(new ThreadStart(SendBackHeatbeatSignal)).Start();
                                            Thread.Sleep(500);
                                            heartbeatRecieved = true;
                                        }

                                        break;
                                    case NetworkMessageTypes.ConsoleLogUpdate:

                                        StringBuilder srvString = new StringBuilder();
                                        string[] split = data.Split('|');
                                        for (int i = 0; i < split.Length; i++)
                                        {
                                            dataSplit = split[i].Split(';');
                                            string srvName = dataSplit[0];
                                            int srvTextLen;
                                            int clientCurLen;
                                            int loop;
                                            if(srvName != "Service")
                                            {
                                                ServerLogger srvText = InstanceProvider.GetBedrockServer(srvName).serverInfo.ConsoleBuffer;
                                                srvTextLen = srvText.Count();
                                                clientCurLen = int.Parse(dataSplit[1]);
                                                loop = clientCurLen;
                                                while (loop < srvTextLen)
                                                {
                                                    srvString.Append($"{srvName};{srvText.FromIndex(loop)};{loop}|");
                                                    loop++;
                                                }

                                            }
                                            else
                                            {
                                                ServiceLogger srvText = InstanceProvider.GetServiceLogger();
                                                srvTextLen = srvText.Count();
                                                clientCurLen = int.Parse(dataSplit[1]);
                                                loop = clientCurLen;
                                                while (loop < srvTextLen)
                                                {
                                                    srvString.Append($"{srvName};{srvText.FromIndex(loop)};{loop}|");
                                                    loop++;
                                                }
                                            }
                                        }
                                        if (srvString.Length > 1)
                                        {
                                            srvString.Remove(srvString.Length - 1, 1);
                                            stringAsBytes = GetBytes(srvString.ToString());
                                            SendData(stringAsBytes, NetworkMessageSource.Server, NetworkMessageDestination.Client, NetworkMessageTypes.ConsoleLogUpdate);
                                        }
                                        break;

                                }
                                break;
                        }
                        AvailBytes = client.Client.Available;
                    }
                    Thread.Sleep(200);
                }
                catch (OutOfMemoryException)
                {
                    InstanceProvider.GetServiceLogger().AppendLine("");

                }
                catch (ObjectDisposedException e)
                {
                    InstanceProvider.GetServiceLogger().AppendLine("Client was disposed! Killing thread...");
                    break;
                }
                catch (ThreadAbortException) { }

                catch (JsonException e)
                {
                    InstanceProvider.GetServiceLogger().AppendLine($"Error parsing json array: {e.Message}");
                    InstanceProvider.GetServiceLogger().AppendLine($"Stacktrace: {e.InnerException}");
                }
                catch (Exception e)
                {
                    //InstanceProvider.GetServiceLogger().AppendLine($"Error: {e.Message} {e.StackTrace}");
                    //InstanceProvider.GetServiceLogger().AppendLine($"Error: {e.Message}: {AvailBytes}, {byteCount}\n{e.StackTrace}");
                }
                AvailBytes = client.Client.Available;
                if (InstanceProvider.GetClientService().ThreadState == ThreadState.Aborted)
                    keepalive = false;
            }
            InstanceProvider.GetServiceLogger().AppendLine("IncomingListener thread exited.");
        }

        private string GetOffsetString(byte[] array) => Encoding.UTF8.GetString(array, 4, array.Length - 4);

        private byte[] GetBytes(string input) => Encoding.UTF8.GetBytes(input);

        private void DisconnectClient()
        {
            try
            {
                stream.Close();
                stream.Dispose();
                client.Close();
                client.Dispose();
                InstanceProvider.DisposeClientService();
                InstanceProvider.DisposeHeartbeatThread();
            }
            catch { }
        }

        public void SendBackHeatbeatSignal()
        {
            InstanceProvider.GetServiceLogger().AppendLine("HeartBeatSender started.");
            while (keepalive)
            {
                heartbeatRecieved = false;
                while (!heartbeatRecieved)
                {
                    Thread.Sleep(100);
                    heartbeatFailTimeout++;
                    if (heartbeatFailTimeout > heartbeatFailTimeoutLimit)
                    {
                        if (!firstHeartbeatRecieved)
                        {
                            try
                            {
                                SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Heartbeat);
                                heartbeatFailTimeout = 0;
                            }
                            catch
                            {
                                InstanceProvider.GetServiceLogger().AppendLine("HeartBeatSender exited.");
                                return;
                            }
                        }
                        DisconnectClient();
                        InstanceProvider.GetServiceLogger().AppendLine("HeartBeatSender exited.");
                        return;
                    }
                }
                heartbeatRecieved = false;
                heartbeatFailTimeout = 0;
                SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Heartbeat);
                Thread.Sleep(3000);
            }
            InstanceProvider.GetServiceLogger().AppendLine("HeartBeatSender exited.");
        }

        private void RestartServer(string payload, bool performBackup)
        {
            if (InstanceProvider.GetBedrockServer(payload).CurrentServerStatus == BedrockServer.ServerStatus.Started)
            {
                InstanceProvider.GetBedrockServer(payload).CurrentServerStatus = BedrockServer.ServerStatus.Stopping;
                while (InstanceProvider.GetBedrockServer(payload).CurrentServerStatus == BedrockServer.ServerStatus.Stopping)
                {
                    Thread.Sleep(100);
                }
                if (performBackup)
                {
                    if (InstanceProvider.GetBedrockServer(payload).Backup())
                    {
                        SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Backup, NetworkMessageStatus.Passed);
                    }
                    else
                    {
                        SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Backup, NetworkMessageStatus.Failed);
                    }
                }
                InstanceProvider.GetBedrockServer(payload).CurrentServerStatus = BedrockServer.ServerStatus.Starting;
                Thread.Sleep(1000);
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
            if (keepalive)
            {
                try
                {
                    stream.Write(compiled, 0, compiled.Length);
                    stream.Flush();
                    return true;

                }
                catch
                {
                    InstanceProvider.GetServiceLogger().AppendLine("Error writing to network stream!");
                    return false;
                }
            }
            return false;
        }

        public bool SendData(NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type) => SendData(new byte[0], source, destination, type, NetworkMessageStatus.None);

        public bool SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type) => SendData(bytes, source, destination, type, NetworkMessageStatus.None);

        public bool SendData(NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type, NetworkMessageStatus status) => SendData(new byte[0], source, destination, type, status);
    }
}