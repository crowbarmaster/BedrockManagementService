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
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
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
        private readonly MmsLogger _logger;
        private readonly Dictionary<MessageTypes, INetworkMessage> _messageLookupContainer;
        private readonly System.Timers.Timer _heartbeatTimer;
        ProgressDialog _progressDialog = new(null);

        public TCPClient(MmsLogger logger) {
            _logger = logger;
            _messageLookupContainer = new NetworkMessageLookup(logger, this).MessageLookupContainer;
            _heartbeatTimer = new System.Timers.Timer(_heartbeatFireInterval);
            _heartbeatTimer.Elapsed += RecieveEvent;
        }

        public void ConnectHost(ClientSideServiceConfiguration host) {
            if (EstablishConnection(host.GetAddress(), int.Parse(host.GetPort()))) {
                SendData(Message.Connect);
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
            if (_netCancelSource != null && _netCancelSource.IsCancellationRequested) {
                _heartbeatTimer.Stop();
                return;
            }
            SendData(Message.Heartbeat);
            Message incomingMsg = new();
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
                        FormManager.MainWindow.Invoke(_progressDialog.Show);

                        _progressDialog.GetDialogProgress().Report(new("Downloading large file from service...", 0.0));
                        double chunkCount = Math.Round(expectedLen / 1024000.00, 0, MidpointRounding.ToPositiveInfinity);
                        double currentProgress = 0.0;
                        do {

                            int recvCount = expectedLen >= 1024000 ? 1024000 : expectedLen;
                            byteCount = stream.Read(buffer, originalLen - expectedLen, recvCount);
                            _progressDialog.GetDialogProgress().Report(new($"Downloading large file from service, {(originalLen - expectedLen) / 1024} bytes recieved.", currentProgress));
                            expectedLen -= recvCount;
                        } while (expectedLen > 0);
                        _progressDialog.EndProgress(new(() => {
                            FormManager.MainWindow.Invoke(_progressDialog.Close);
                            FormManager.MainWindow.Invoke(_progressDialog.Dispose);
                            _progressDialog = null;
                        }));
                    } else {
                        byteCount = stream.Read(buffer, 0, expectedLen);
                    }
                    incomingMsg = new Message(buffer);
                    if (incomingMsg == Message.Empty()) {
                        continue;
                    }
                    if (incomingMsg.Type != MessageTypes.PackFile || incomingMsg.Type != MessageTypes.LevelEditFile)
                    if (FormManager.MainWindow.ConfigManager.DebugNetworkOutput) {
                        _logger.AppendLine($@"Network msg: {incomingMsg.Type} * {incomingMsg.Flag} * {incomingMsg.ServerIndex}");
                        _logger.AppendLine($@"Data: {incomingMsg.GetDataString()}");
                    }
                    if (_messageLookupContainer != null && _messageLookupContainer.ContainsKey(incomingMsg.Type) && _messageLookupContainer[incomingMsg.Type].ProcessMessage(incomingMsg).Result) {
                        if (FormManager.MainWindow.ConfigManager.DebugNetworkOutput) {
                            _logger.AppendLine($@"Network message processed - success.");
                        }
                    }
                }
            } catch (Exception e) {
                _logger.AppendLine($"TCPClient error! ServerIndex: {incomingMsg.ServerIndex} MsgType: {Enum.GetName(incomingMsg.Type)} Stacktrace: {e.Message}\n{e.StackTrace}");
                _heartbeatTimer.Enabled = true;
            }
        }

        public bool SendData(Message message) {
            byte[] sendBtytes = message.GetMessageBytes();
            if (EstablishedLink) {
                try {
                    stream.Write(sendBtytes, 0, sendBtytes.Length);
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
    }
}
