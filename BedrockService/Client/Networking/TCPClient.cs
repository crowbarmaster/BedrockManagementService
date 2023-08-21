using BedrockService.Client.Management;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using BedrockService.Shared.PackParser;
using BedrockService.Shared.SerializeModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BedrockService.Client.Networking {
    public class TCPClient {
        public TcpClient OpenedTcpClient;
        public string ClientName;
        public NetworkStream stream;
        public bool EstablishedLink;
        public bool Connected;
        public bool EnableRead;
        public bool PlayerInfoArrived;
        public bool EnumBackupsArrived;
        public List<BackupInfoModel> BackupList;
        public List<MinecraftPackContainer> RecievedPacks;
        public Task ClientReciever;
        private CancellationTokenSource? _netCancelSource;
        private int _heartbeatFailTimeout;
        private const int _heartbeatFailTimeoutLimit = 2;
        private readonly IServerLogger _logger;

        public TCPClient(IServerLogger logger) {
            _logger = logger;
        }

        public void ConnectHost(IClientSideServiceConfiguration host) {
            if (EstablishConnection(host.GetAddress(), int.Parse(host.GetPort()))) {
                SendData(NetworkMessageTypes.Connect);
                return;
            }
        }

        public bool EstablishConnection(string addr, int port) {
            _logger.AppendLine("Connecting to Server");
            _netCancelSource = new CancellationTokenSource();
            try {
                EnableRead = false;
                OpenedTcpClient = new TcpClient(addr, port);
                stream = OpenedTcpClient.GetStream();
                EstablishedLink = true;
                ClientReciever = Task.Factory.StartNew(new Action(ReceiveListener), _netCancelSource.Token);
            } catch(Exception e) {
                _logger.AppendLine($"Could not connect to Server: {e.Message}");
                if (ClientReciever != null)
                    _netCancelSource.Cancel();
                ClientReciever = null;
                return false;
            }
            return EstablishedLink;
        }

        public void CloseConnection() {
            try {
                if (stream != null)
                    stream.Dispose();
                stream = null;
                Connected = false;
                EstablishedLink = false;
                _netCancelSource.Cancel();
            } catch (NullReferenceException) {
                Connected = false;
                EstablishedLink = false;
            } catch (Exception e) {
                _logger.AppendLine($"Error closing connection: {e.StackTrace}");
            }
        }

        public void ReceiveListener() {
            while (!_netCancelSource.IsCancellationRequested) {
                SendData(NetworkMessageTypes.Heartbeat);
                try {
                    byte[] buffer = new byte[4];
                    while (OpenedTcpClient.Client.Available > 0) {
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
                        if (FormManager.MainWindow.ConfigManager.DebugNetworkOutput) {
                            _logger.AppendLine($@"Network msg: {source} * {destination} * {msgType} * {msgStatus} * {serverIndex}");
                            _logger.AppendLine($@"Data: {data}");
                        }
                        if (destination != NetworkMessageDestination.Client)
                            continue;
                        int srvCurLen = 0;
                        JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                        switch (msgType) {
                            case NetworkMessageTypes.Connect:
                                try {
                                    if (!string.IsNullOrEmpty(data)) {
                                        _logger.AppendLine("Connection to Host successful!");
                                        FormManager.MainWindow.connectedHost = JsonConvert.DeserializeObject<IServiceConfiguration>(data, settings);
                                        Connected = true;
                                        FormManager.MainWindow.RefreshServerContents();
                                        FormManager.MainWindow.ServerBusy = false;
                                    }
                                } catch (Exception e) {
                                    _logger.AppendLine($"Error: ConnectMan reported error: {e.Message}\n{e.StackTrace}");
                                    CloseConnection();
                                    _netCancelSource.Cancel();
                                }
                                break;

                            case NetworkMessageTypes.EnumBackups:

                                BackupList = JsonConvert.DeserializeObject<List<BackupInfoModel>>(data);
                                FormManager.MainWindow.Invoke(() => FormManager.MainWindow.UpdateBackupManagerData());

                                break;
                            case NetworkMessageTypes.CheckUpdates:

                                //TODO: Ask user if update now or perform later.
                                UnlockUI();

                                break;
                            case NetworkMessageTypes.UICallback:

                                UnlockUI();

                                break;
                            case NetworkMessageTypes.ConsoleLogUpdate:
                                string[] strings = data.Split('|');
                                for (int i = 0; i < strings.Length; i++) {
                                    string[] srvSplit = strings[i].Split(';');
                                    string srvName = srvSplit[0];
                                    string srvText = srvSplit[1];
                                    srvCurLen = int.Parse(srvSplit[2]);
                                    if (srvName != "Service") {
                                        IServerConfiguration bedrockServer = FormManager.MainWindow.connectedHost.GetServerInfoByName(srvName);
                                        int curCount = bedrockServer.GetLog().Count;
                                        if (curCount == srvCurLen) {
                                            bedrockServer.GetLog().Add(new LogEntry(srvText));
                                        }
                                    } else {
                                        int curCount = FormManager.MainWindow.connectedHost.GetLog().Count;
                                        if (curCount == srvCurLen) {
                                            FormManager.MainWindow.connectedHost.GetLog().Add(new LogEntry(srvText));
                                        }
                                    }
                                }
                                break;
                            case NetworkMessageTypes.Backup:

                                _logger.AppendLine(msgStatus.ToString());

                                break;
                            case NetworkMessageTypes.PackList:

                                List<MinecraftPackContainer> temp = new();
                                JArray jArray = JArray.Parse(data);
                                foreach (JToken token in jArray)
                                    temp.Add(token.ToObject<MinecraftPackContainer>());
                                FormManager.MainWindow.RecievePackData(serverIndex, temp);

                                break;
                            case NetworkMessageTypes.PlayersRequest:

                                List<IPlayer> fetchedPlayers = JsonConvert.DeserializeObject<List<IPlayer>>(data, settings);
                                FormManager.MainWindow.RecievePlayerData(serverIndex, fetchedPlayers);

                                break;
                            case NetworkMessageTypes.LevelEditFile:

                                byte[] stripHeaderFromBuffer = new byte[buffer.Length - 5];
                                Buffer.BlockCopy(buffer, 5, stripHeaderFromBuffer, 0, stripHeaderFromBuffer.Length);
                                string pathToLevelDat = $@"{Path.GetTempPath()}\level.dat";
                                File.WriteAllBytes(pathToLevelDat, stripHeaderFromBuffer);
                                UnlockUI();

                                break;
                            case NetworkMessageTypes.ServerStatusRequest:

                                StatusUpdateModel status = JsonConvert.DeserializeObject<StatusUpdateModel>(data, settings);
                                if (status != null && status.ServerStatusModel != null && status.ServerStatusModel.ServerIndex != 255) {
                                    ServerStatusModel formerServerStatus = FormManager.MainWindow.selectedServer.GetStatus();
                                    if (!status.ServerStatusModel.Equals(formerServerStatus)) {
                                        FormManager.MainWindow.connectedHost.GetServerInfoByIndex(status.ServerStatusModel.ServerIndex).SetStatus(status.ServerStatusModel);
                                        FormManager.MainWindow.Invoke(() => FormManager.MainWindow.RefreshAllCompenentStates());
                                        FormManager.TCPClient.SendData(serverIndex, NetworkMessageTypes.EnumBackups);
                                        Task.Delay(1000).Wait();
                                    }
                                    FormManager.MainWindow.ServiceStatus = status.ServiceStatusModel;
                                }

                                break;
                            case NetworkMessageTypes.ClientReject:
                                
                                FormManager.MainWindow.Invoke((MethodInvoker)delegate { FormManager.MainWindow.ServerInfoBox.Text = "Connection attempt rejected by Service!"; });
                                FormManager.MainWindow.Invoke((MethodInvoker)delegate { FormManager.MainWindow.HostInfoLabel.Text = "Connection attempt rejected by Service!"; });

                                break;
                            case NetworkMessageTypes.ExportFile:

                                ExportImportFileModel exportModel = JsonConvert.DeserializeObject<ExportImportFileModel>(data, settings);
                                if (exportModel != null) {
                                    FormManager.MainWindow.Invoke(() => FormManager.MainWindow.RecieveExportData(exportModel));
                                }

                                break;
                        }
                    }
                } catch (Exception e) {
                    _logger.AppendLine($"TCPClient error! Stacktrace: {e.Message}\n{e.StackTrace}");
                }
                Thread.Sleep(200);
            }
        }

        public bool SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type, NetworkMessageFlags status) {
            byte[] compiled = new byte[9 + bytes.Length];
            byte[] len = BitConverter.GetBytes(5 + bytes.Length);
            Buffer.BlockCopy(len, 0, compiled, 0, 4);
            compiled[4] = (byte)source;
            compiled[5] = (byte)destination;
            compiled[6] = serverIndex;
            compiled[7] = (byte)type;
            compiled[8] = (byte)status;
            Buffer.BlockCopy(bytes, 0, compiled, 9, bytes.Length);
            if (EstablishedLink) {
                try {
                    stream.Write(compiled, 0, compiled.Length);
                    stream.Flush();
                    _heartbeatFailTimeout = 0;
                    return true;
                } catch {
                    _logger.AppendLine("Error writing to network stream!");
                    Thread.Sleep(100);
                    _heartbeatFailTimeout++;
                    if (_heartbeatFailTimeout > _heartbeatFailTimeoutLimit) {
                        Task.Run(() => { FormManager.MainWindow.HeartbeatFailDisconnect(); });
                        _netCancelSource.Cancel();
                        EstablishedLink = false;
                        _heartbeatFailTimeout = 0;
                    }
                    return false;
                }
            }
            return false;
        }

        public void SendData(NetworkMessageTypes type) => SendData(new byte[0], NetworkMessageSource.Client, NetworkMessageDestination.Service, 0xFF, type, NetworkMessageFlags.None);

        public void SendData(byte serverIndex, NetworkMessageTypes type) => SendData(new byte[0], NetworkMessageSource.Client, NetworkMessageDestination.Service, serverIndex, type, NetworkMessageFlags.None);

        public void SendData(byte serverIndex, NetworkMessageTypes type, NetworkMessageFlags flag) => SendData(new byte[0], NetworkMessageSource.Client, NetworkMessageDestination.Service, serverIndex, type, flag);

        public void SendData(byte[] bytes, byte serverIndex, NetworkMessageTypes type) => SendData(bytes, NetworkMessageSource.Client, NetworkMessageDestination.Service, serverIndex, type, NetworkMessageFlags.None);

        public void SendData(byte[] bytes, NetworkMessageTypes type) => SendData(bytes, NetworkMessageSource.Client, NetworkMessageDestination.Service, 0xFF, type, NetworkMessageFlags.None);

        public void SendData(NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type, NetworkMessageFlags status) => SendData(new byte[0], source, destination, 0xFF, type, status);

        public void Dispose() {
            if (_netCancelSource != null) {
                _netCancelSource.Cancel();
                _netCancelSource.Dispose();
                _netCancelSource = null;
            }
            ClientReciever = null;
            if (OpenedTcpClient != null) {
                OpenedTcpClient.Close();
                OpenedTcpClient.Dispose();
            }

        }

        private string GetOffsetString(byte[] array) => Encoding.UTF8.GetString(array, 5, array.Length - 5);

        private void UnlockUI() => FormManager.MainWindow.ServerBusy = false;
    }
}
