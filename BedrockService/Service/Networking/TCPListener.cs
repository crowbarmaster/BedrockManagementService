using BedrockService.Service.Logging;
using BedrockService.Service.Server;
using BedrockService.Service.Server.HostInfoClasses;
using BedrockService.Service.Server.Logging;
using BedrockService.Service.Server.PackParser;
using BedrockService.Service.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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
        private int heartbeatFailTimeoutLimit = 200;

        public void StartListening(int port)
        {
            IPAddress addr = IPAddress.Parse("0.0.0.0");
            InListener = InstanceProvider.InitTCPListener(addr, port);
            try
            {
                InListener.Start();
            }
            catch
            {
                InstanceProvider.ServiceLogger.AppendLine("Error! Port is occupied and cannot be opened... Program will be killed!");
                Thread.Sleep(2000);
                Environment.Exit(1);
            }

            while (!Program.IsExiting)
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
                    InstanceProvider.ServiceLogger.AppendLine(e.ToString());
                }
            }
            //listener.Stop();
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
                    InstanceProvider.ServiceLogger.AppendLine("Error writing to network stream!");
                    return false;
                }
            }
            return false;
        }

        public bool SendData(NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type) => SendData(new byte[0], source, destination, 0xFF, type, NetworkMessageFlags.None);

        public void SendData(NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type) => SendData(new byte[0], source, destination, serverIndex, type, NetworkMessageFlags.None);

        public bool SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type) => SendData(bytes, source, destination, serverIndex, type, NetworkMessageFlags.None);

        public bool SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type) => SendData(bytes, source, destination, 0xFF, type, NetworkMessageFlags.None);

        public bool SendData(NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type, NetworkMessageFlags status) => SendData(new byte[0], source, destination, 0xFF, type, status);

        private void IncomingListener()
        {
            keepalive = true;
            InstanceProvider.ServiceLogger.AppendLine("Established connection! Listening for incoming packets!");
            int AvailBytes = 0;
            int byteCount = 0;
            NetworkMessageSource msgSource = 0;
            NetworkMessageDestination msgDest = 0;
            byte serverIndex = 0xFF;
            NetworkMessageTypes msgType = 0;
            NetworkMessageFlags msgFlag = 0;
            while (InstanceProvider.GetClientServiceAlive() && !Program.IsExiting)
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
                        msgSource = (NetworkMessageSource)buffer[0];
                        msgDest = (NetworkMessageDestination)buffer[1];
                        serverIndex = buffer[2];
                        msgType = (NetworkMessageTypes)buffer[3];
                        msgFlag = (NetworkMessageFlags)buffer[4];
                        switch (msgDest)
                        {
                            case NetworkMessageDestination.Server:
                                ParseServerMessage(msgType, serverIndex, buffer, msgFlag);
                                break;
                            case NetworkMessageDestination.Service:
                                ParseServiceMessage(msgType, serverIndex, buffer, msgFlag);
                                break;
                        }
                        AvailBytes = client.Client.Available;
                    }
                    Thread.Sleep(200);
                }
                catch (OutOfMemoryException)
                {
                    InstanceProvider.ServiceLogger.AppendLine("Out of memory exception thrown.");
                }
                catch (ObjectDisposedException)
                {
                    InstanceProvider.ServiceLogger.AppendLine("Client was disposed! Killing thread...");
                    break;
                }
                catch (InvalidOperationException e)
                {
                    if (msgType != NetworkMessageTypes.ConsoleLogUpdate)
                    {
                        InstanceProvider.ServiceLogger.AppendLine(e.Message);
                        InstanceProvider.ServiceLogger.AppendLine(e.StackTrace);
                    }
                }
                catch (ThreadAbortException) { }

                catch (JsonException e)
                {
                    InstanceProvider.ServiceLogger.AppendLine($"Error parsing json array: {e.Message}");
                    InstanceProvider.ServiceLogger.AppendLine($"Stacktrace: {e.InnerException}");
                }
                catch (Exception e)
                {
                    InstanceProvider.ServiceLogger.AppendLine($"Error: {e.Message} {e.StackTrace}");
                }
                AvailBytes = client.Client.Available;
                if (InstanceProvider.ClientService.ThreadState == ThreadState.Aborted)
                    keepalive = false;
            }
            InstanceProvider.ServiceLogger.AppendLine("IncomingListener thread exited.");
        }

        private void ParseServerMessage(NetworkMessageTypes messageType, byte serverIndex, byte[] byteData, NetworkMessageFlags flag)
        {
            string serversPath = InstanceProvider.HostInfo.GetGlobalValue("ServersPath");
    
            string stringData = messageType != NetworkMessageTypes.PackFile || messageType != NetworkMessageTypes.LevelEditFile ? GetOffsetString(byteData) : "";
            switch (messageType)
            {
                case NetworkMessageTypes.PropUpdate:

                    List<Property> propList = JsonConvert.DeserializeObject<List<Property>>(stringData);
                    Property prop = propList.First(p => p.KeyName == "server-name");
                    InstanceProvider.GetServerInfoByIndex(serverIndex).ServerPropList = propList;
                    InstanceProvider.ConfigManager.SaveServerProps(InstanceProvider.GetServerInfoByIndex(serverIndex), true);
                    InstanceProvider.ConfigManager.LoadConfigs();
                    InstanceProvider.GetBedrockServerByIndex(serverIndex).CurrentServerStatus = BedrockServer.ServerStatus.Stopping;
                    while (InstanceProvider.GetBedrockServerByIndex(serverIndex).CurrentServerStatus == BedrockServer.ServerStatus.Stopping)
                    {
                        Thread.Sleep(100);
                    }
                    InstanceProvider.GetBedrockServerByIndex(serverIndex).CurrentServerStatus = BedrockServer.ServerStatus.Starting;
                    SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);

                    break;
                case NetworkMessageTypes.Restart:

                    RestartServer(serverIndex, false);
                    SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);

                    break;
                case NetworkMessageTypes.Backup:

                    RestartServer(serverIndex, true);
                    SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);

                    break;
                case NetworkMessageTypes.Command:

                    InstanceProvider.GetBedrockServerByIndex(serverIndex).StdInStream.WriteLine(stringData);
                    InstanceProvider.ServiceLogger.AppendLine($"Sent command {stringData} to stdInput stream");

                    break;
                case NetworkMessageTypes.PlayersRequest:

                    ServerInfo server = InstanceProvider.GetServerInfoByIndex(serverIndex);
                    byte[] serializeToBytes = GetBytesFromString(JsonConvert.SerializeObject(server.KnownPlayers));
                    SendData(serializeToBytes, NetworkMessageSource.Server, NetworkMessageDestination.Client, serverIndex, NetworkMessageTypes.PlayersRequest);

                    break;
                case NetworkMessageTypes.RemovePack:

                    MinecraftKnownPacksClass knownPacks = new MinecraftKnownPacksClass($@"{InstanceProvider.GetServerInfoByIndex(serverIndex).ServerPath.Value}\valid_known_packs.json", $@"{Program.ServiceDirectory}\Server\stock_packs.json");
                    List<MinecraftPackContainer> MCContainer = JsonConvert.DeserializeObject<List<MinecraftPackContainer>>(stringData);
                    foreach (MinecraftPackContainer cont in MCContainer)
                        knownPacks.RemovePackFromServer(InstanceProvider.GetServerInfoByIndex(serverIndex).ServerPath.Value, cont);

                    break;
                case NetworkMessageTypes.PackList:

                    if (!File.Exists($@"{Program.ServiceDirectory}\Server\stock_packs.json"))
                        File.Copy($@"{InstanceProvider.GetServerInfoByIndex(serverIndex).ServerPath}\valid_known_packs.json", $@"{Program.ServiceDirectory}\Server\stock_packs.json");
                    knownPacks = new MinecraftKnownPacksClass($@"{InstanceProvider.GetServerInfoByIndex(serverIndex).ServerPath}\valid_known_packs.json", $@"{Program.ServiceDirectory}\Server\stock_packs.json");
                    List<MinecraftPackParser> list = new List<MinecraftPackParser>();
                    foreach (MinecraftKnownPacksClass.KnownPack pack in knownPacks.KnownPacks)
                        list.Add(new MinecraftPackParser($@"{InstanceProvider.GetServerInfoByIndex(serverIndex).ServerPath}\{pack.path.Replace(@"/", @"\")}"));

                    SendData(GetBytesFromString(JArray.FromObject(list).ToString()), NetworkMessageSource.Server, NetworkMessageDestination.Client, NetworkMessageTypes.PackList);

                    break;
                case NetworkMessageTypes.LevelEditRequest:

                    server = InstanceProvider.GetServerInfoByIndex(serverIndex);
                    string pathToLevelDat = $@"{serversPath}\{server.GetServerProp("server-name")}\worlds\{server.GetServerProp("level-name")}\level.dat";
                    byte[] levelDatToBytes = File.ReadAllBytes(pathToLevelDat);
                    SendData(levelDatToBytes, NetworkMessageSource.Server, NetworkMessageDestination.Client, NetworkMessageTypes.LevelEditFile);

               break;
                case NetworkMessageTypes.PlayersUpdate:

                    List<Player> fetchedPlayers = JsonConvert.DeserializeObject<List<Player>>(stringData);
                    List<Player> known = InstanceProvider.GetServerInfoByIndex(serverIndex).KnownPlayers;
                    foreach (Player player in fetchedPlayers)
                    {
                        try
                        {
                            Player playerFound = InstanceProvider.GetServerInfoByIndex(serverIndex).KnownPlayers.First(p => p.XUID == player.XUID);
                            if (player != playerFound)
                                InstanceProvider.GetServerInfoByIndex(serverIndex).KnownPlayers[InstanceProvider.GetServerInfoByIndex(serverIndex).KnownPlayers.IndexOf(playerFound)] = player;
                        }
                        catch (Exception)
                        {
                            InstanceProvider.GetServerInfoByIndex(serverIndex).KnownPlayers.Add(player);
                        }
                    }
                    InstanceProvider.ConfigManager.SaveRegisteredPlayers(InstanceProvider.GetServerInfoByIndex(serverIndex));
                    InstanceProvider.ConfigManager.LoadConfigs();
                    InstanceProvider.ConfigManager.LoadRegisteredPlayers(InstanceProvider.GetServerInfoByIndex(serverIndex));
                    SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);

                    break;
                    case NetworkMessageTypes.PackFile:

                        MinecraftPackParser archiveParser = new MinecraftPackParser(byteData);
                        foreach (MinecraftPackContainer container in archiveParser.FoundPacks)
                        {
                            if (container.ManifestType == "WorldPack")
                                FileUtils.CopyFilesRecursively(container.PackContentLocation, new DirectoryInfo($@"{InstanceProvider.GetServerInfoByIndex(serverIndex).ServerPath.Value}\worlds\{container.FolderName}"));
                            if (container.ManifestType == "data")
                                FileUtils.CopyFilesRecursively(container.PackContentLocation, new DirectoryInfo($@"{InstanceProvider.GetServerInfoByIndex(serverIndex).ServerPath.Value}\behavior_packs\{container.FolderName}"));
                            if (container.ManifestType == "resources")
                                FileUtils.CopyFilesRecursively(container.PackContentLocation, new DirectoryInfo($@"{InstanceProvider.GetServerInfoByIndex(serverIndex).ServerPath.Value}\resource_packs\{container.FolderName}"));
                        }

                        break;
                    case NetworkMessageTypes.LevelEditFile:

                        byte[] stripHeaderFromBuffer = new byte[byteData.Length - 5];
                        Buffer.BlockCopy(byteData, 5, stripHeaderFromBuffer, 0, stripHeaderFromBuffer.Length);
                        server = InstanceProvider.GetServerInfoByIndex(serverIndex);
                        pathToLevelDat = $@"{serversPath}\{server.GetServerProp("server-name")}\worlds\{server.GetServerProp("level-name")}\level.dat";
                        if (InstanceProvider.GetBedrockServerByIndex(serverIndex).StopControl())
                            File.WriteAllBytes(pathToLevelDat, stripHeaderFromBuffer);
                        InstanceProvider.GetBedrockServerByIndex(serverIndex).CurrentServerStatus = BedrockServer.ServerStatus.Starting;

                        break;
                
            }
        }

        private void ParseServiceMessage(NetworkMessageTypes messageType, byte serverIndex, byte[] byteData, NetworkMessageFlags flag)
        {
            string stringData = GetOffsetString(byteData);
            string serversPath = InstanceProvider.HostInfo.GetGlobalValue("ServersPath");
            string[] dataSplit = null;
            switch (messageType)
            {
                case NetworkMessageTypes.Connect:

                    InstanceProvider.HostInfo.ServiceLog = InstanceProvider.ServiceLogger.Log;
                    byte[] serializeToBytes = GetBytesFromString(JsonConvert.SerializeObject(InstanceProvider.HostInfo));
                    SendData(serializeToBytes, NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Connect);
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
                    string[] split = stringData.Split('|');
                    for (int i = 0; i < split.Length; i++)
                    {
                        dataSplit = split[i].Split(';');
                        string srvName = dataSplit[0];
                        int srvTextLen;
                        int clientCurLen;
                        int loop;
                        if (srvName != "Service")
                        {
                            InstanceProvider.GetBedrockServerByName(srvName).serverInfo.ConsoleBuffer = InstanceProvider.GetBedrockServerByName(srvName).serverInfo.ConsoleBuffer ?? new ServerLogger(srvName);
                            ServerLogger srvText = InstanceProvider.GetBedrockServerByName(srvName).serverInfo.ConsoleBuffer;
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
                            ServiceLogger srvText = InstanceProvider.ServiceLogger;
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
                        SendData(GetBytesFromString(srvString.ToString()), NetworkMessageSource.Server, NetworkMessageDestination.Client, NetworkMessageTypes.ConsoleLogUpdate);
                    }

                    break;
                case NetworkMessageTypes.StartCmdUpdate:

                    InstanceProvider.GetServerInfoByIndex(serverIndex).StartCmds = JsonConvert.DeserializeObject<List<StartCmdEntry>>(stringData);
                    InstanceProvider.ConfigManager.SaveServerProps(InstanceProvider.GetServerInfoByIndex(serverIndex), true);

                    break;
                case NetworkMessageTypes.BackupAll:

                    foreach (BedrockServer server in InstanceProvider.BedrockService.bedrockServers)
                    {
                        RestartServer((byte)InstanceProvider.BedrockService.bedrockServers.IndexOf(server), true);
                    }
                    while (InstanceProvider.BedrockService.bedrockServers[InstanceProvider.BedrockService.bedrockServers.Count - 1].CurrentServerStatus != BedrockServer.ServerStatus.Started)
                        Thread.Sleep(250);
                    SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);

                    break;
                case NetworkMessageTypes.DelBackups:

                    List<string> backupFileNames = JsonConvert.DeserializeObject<List<string>>(stringData);
                    InstanceProvider.ConfigManager.DeleteBackupsForServer(serverIndex, backupFileNames);

                    break;
                case NetworkMessageTypes.EnumBackups:

                    serializeToBytes = GetBytesFromString(JsonConvert.SerializeObject(InstanceProvider.ConfigManager.EnumerateBackupsForServer(serverIndex)));
                    SendData(serializeToBytes, NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.EnumBackups);

                    break;
                case NetworkMessageTypes.BackupRollback:

                    InstanceProvider.ConfigManager.RollbackToBackup(serverIndex, stringData);
                    SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);

                    break;
                case NetworkMessageTypes.CheckUpdates:

                    if (Updater.CheckUpdates().Result)
                    {
                        if (Updater.VersionChanged)
                        {
                            SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.CheckUpdates);
                            break;
                        }
                    }
                    SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);

                    break;
                case NetworkMessageTypes.AddNewServer:

                    List<Property> propList = JsonConvert.DeserializeObject<List<Property>>(stringData);
                    Property serverNameProp = propList.First(p => p.KeyName == "server-name");
                    ServerInfo newServer = new ServerInfo
                    {
                        ServerName = serverNameProp.Value,
                        ServerPropList = propList,
                        FileName = $@"{serverNameProp.Value}.conf"
                    };
                    newServer.ServerPath.Value = $@"{serversPath}\{serverNameProp.Value}";
                    newServer.ServerExeName.Value = $"BDS_{serverNameProp.Value}.exe";
                    InstanceProvider.ConfigManager.SaveServerProps(newServer, true);
                    InstanceProvider.BedrockService.InitializeNewServer(newServer);
                    serializeToBytes = GetBytesFromString(JsonConvert.SerializeObject((InstanceProvider.HostInfo)));
                    SendData(serializeToBytes, NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Connect);
                    SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);


                    break;
                case NetworkMessageTypes.RemoveServer:

                    InstanceProvider.GetBedrockServerByIndex(serverIndex).CurrentServerStatus = BedrockServer.ServerStatus.Stopping;
                    while (InstanceProvider.GetBedrockServerByIndex(serverIndex).CurrentServerStatus != BedrockServer.ServerStatus.Stopped)
                        Thread.Sleep(200);
                    InstanceProvider.ConfigManager.RemoveServerConfigs(InstanceProvider.GetBedrockServerByIndex(serverIndex).serverInfo, flag);
                    InstanceProvider.HostInfo.ServerList.Remove(InstanceProvider.GetServerInfoByIndex(serverIndex));
                    serializeToBytes = GetBytesFromString(JsonConvert.SerializeObject(InstanceProvider.HostInfo));
                    SendData(serializeToBytes, NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Connect);
                    SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);

                    break;
            }

        }

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

        private void SendBackHeatbeatSignal()
        {
            InstanceProvider.ServiceLogger.AppendLine("HeartBeatSender started.");
            while (keepalive && !Program.IsExiting)
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
                                InstanceProvider.ServiceLogger.AppendLine("HeartBeatSender exited.");
                                return;
                            }
                        }
                        DisconnectClient();
                        InstanceProvider.ServiceLogger.AppendLine("HeartBeatSender exited.");
                        return;
                    }
                }
                heartbeatRecieved = false;
                heartbeatFailTimeout = 0;
                SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Heartbeat);
                Thread.Sleep(3000);
            }
            InstanceProvider.ServiceLogger.AppendLine("HeartBeatSender exited.");
        }

        private void RestartServer(byte serverIndex, bool performBackup)
        {
            if (InstanceProvider.GetBedrockServerByIndex(serverIndex).CurrentServerStatus == BedrockServer.ServerStatus.Started)
            {
                InstanceProvider.GetBedrockServerByIndex(serverIndex).CurrentServerStatus = BedrockServer.ServerStatus.Stopping;
                while (InstanceProvider.GetBedrockServerByIndex(serverIndex).CurrentServerStatus == BedrockServer.ServerStatus.Stopping)
                {
                    Thread.Sleep(100);
                }
                if (performBackup)
                {
                    if (InstanceProvider.GetBedrockServerByIndex(serverIndex).Backup())
                    {
                        SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Backup, NetworkMessageFlags.Passed);
                    }
                    else
                    {
                        SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Backup, NetworkMessageFlags.Failed);
                    }
                }
                InstanceProvider.GetBedrockServerByIndex(serverIndex).CurrentServerStatus = BedrockServer.ServerStatus.Starting;
                Thread.Sleep(1000);
            }
        }

        private string GetOffsetString(byte[] array) => Encoding.UTF8.GetString(array, 5, array.Length - 5);

        private byte[] GetBytesFromString(string input) => Encoding.UTF8.GetBytes(input);
    }
}