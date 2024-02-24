// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MinecraftService.Client.Forms;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.PackParser;
using MinecraftService.Shared.SerializeModels;

namespace MinecraftService.Client.Networking {
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
        private const int _heartbeatFireInterval = 300;
        private readonly IServerLogger _logger;
        private readonly Dictionary<NetworkMessageTypes, INetworkMessage> _messageLookupContainer;
        private readonly System.Timers.Timer _heartbeatTimer;
        ProgressDialog _progressDialog = new(null);

        public TCPClient(IServerLogger logger) {
            _logger = logger;
            _messageLookupContainer = new NetworkMessageLookup(logger, this).MessageLookupContainer;
            _heartbeatTimer = new System.Timers.Timer(_heartbeatFireInterval);
            _heartbeatTimer.Elapsed += RecieveEvent;
        }

        public void ConnectHost(IClientSideServiceConfiguration host) {
            if (EstablishConnection(host.GetAddress(), int.Parse(host.GetPort()))) {
                SendData(NetworkMessageTypes.Connect);
                return;
            }
        }

        public void Cancel() {
            _netCancelSource.Cancel();
        }

        public bool EstablishConnection(string addr, int port) {
            _logger.AppendLine("Connecting to Server");
            _netCancelSource = new CancellationTokenSource();
            try {
                EnableRead = false;
                OpenedTcpClient = new TcpClient(addr, port);
                stream = OpenedTcpClient.GetStream();
                EstablishedLink = true;
                _heartbeatTimer.Start();
                // ClientReciever = Task.Factory.StartNew(new Action(ReceiveListener), _netCancelSource.Token);
            } catch (Exception e) {
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
                if (_netCancelSource != null)
                    _netCancelSource.Cancel();
            } catch (NullReferenceException) {
                Connected = false;
                EstablishedLink = false;
            } catch (Exception e) {
                _logger.AppendLine($"Error closing connection: {e.StackTrace}");
            }
        }

        public void RecieveEvent(object sender, System.Timers.ElapsedEventArgs elapsedArgs) {
            if (_netCancelSource.IsCancellationRequested) {
                _heartbeatTimer.Stop();
                return;
            }
            SendData(NetworkMessageTypes.Heartbeat);
            NetworkMessageSource source = 0;
            NetworkMessageDestination destination = 0;
            byte serverIndex = 0;
            NetworkMessageTypes msgType = 0;
            try {
                byte[] buffer = new byte[4];
                while (OpenedTcpClient.Client.Available > 0) {
                    int byteCount = stream.Read(buffer, 0, 4);
                    int expectedLen = BitConverter.ToInt32(buffer, 0);
                    int originalLen = expectedLen;
                    buffer = new byte[expectedLen];
                    if (expectedLen > 102400000 && !FormManager.MainWindow.ServerBusy) {
                        if (_progressDialog == null || _progressDialog.IsDisposed) {
                            _progressDialog = new(null);
                        }
                        _progressDialog.Show();
                        _progressDialog.GetDialogProgress().Report(new("Downloading large file from service...", 0.0));
                        double chunkCount = Math.Round(expectedLen / 1024000.00, 0, MidpointRounding.ToPositiveInfinity);
                        double currentProgress = 0.0;
                        do {
                            int recvCount = expectedLen >= 1024000 ? 1024000 : expectedLen;
                            byteCount = stream.Read(buffer, originalLen - expectedLen, recvCount);
                            _progressDialog.GetDialogProgress().Report(new($"Downloading large file from service, {originalLen - expectedLen} bytes recieved.", currentProgress));
                            expectedLen -= recvCount;
                        } while (expectedLen > 0);
                        _progressDialog.EndProgress(new(() => {
                            _progressDialog.Invoke(_progressDialog.Close);
                            _progressDialog.Invoke(_progressDialog.Dispose);
                            _progressDialog = null;
                        }));
                    } else {
                        byteCount = stream.Read(buffer, 0, expectedLen);
                    }
                    source = (NetworkMessageSource)buffer[0];
                    destination = (NetworkMessageDestination)buffer[1];
                    serverIndex = buffer[2];
                    msgType = (NetworkMessageTypes)buffer[3];
                    NetworkMessageFlags msgStatus = (NetworkMessageFlags)buffer[4];
                    string data = "";
                    if (msgType != NetworkMessageTypes.PackFile || msgType != NetworkMessageTypes.LevelEditFile)
                        data = GetOffsetString(buffer);
                    if (FormManager.MainWindow.ConfigManager.DebugNetworkOutput) {
                        _logger.AppendLine($@"Network msg: {source} * {destination} * {msgType} * {msgStatus} * {serverIndex}");
                        _logger.AppendLine($@"Data: {data}");
                    }
                    if (destination != NetworkMessageDestination.Client) {
                        continue;
                    }
                    if (_messageLookupContainer != null && _messageLookupContainer.ContainsKey(msgType) && _messageLookupContainer[msgType].ProcessMessage(buffer).Result) {
                        if (FormManager.MainWindow.ConfigManager.DebugNetworkOutput) {
                            _logger.AppendLine($@"Network message processed - success.");
                        }
                    }
                }
            } catch (Exception e) {
                _logger.AppendLine($"TCPClient error! MsgSource: {source} ServerIndex: {serverIndex} MsgType: {msgType} Stacktrace: {e.Message}\n{e.StackTrace}");
                _heartbeatTimer.Enabled = true;
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
    }
}
