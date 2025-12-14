using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MinecraftService.Client.Forms;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.JsonModels.Minecraft;
using MinecraftService.Shared.PackParser;
using MinecraftService.Shared.SerializeModels;

namespace MinecraftService.Client.Networking {
    public class TCPClient {
        public TcpClient OpenedTcpClient;
        public string ClientName;
        public NetworkStream stream;
        public bool EstablishedLink;
        public bool _enableRead;
        public List<MinecraftPackContainer> ReceivedPacks;
        public Task ClientReceiver;
        private CancellationTokenSource? _netCancelSource;
        private int _heartbeatFailTimeout;
        private const int _heartbeatFailTimeoutLimit = 2;
        private const int _heartbeatFireInterval = 100;
        private double _desiredChunkSize = 1024;
        private readonly MmsLogger _logger;
        private readonly Dictionary<MessageTypes, INetworkMessage> _messageLookupContainer;
        private readonly System.Timers.Timer _heartbeatTimer;
        ProgressDialog _progressDialog = new(null);

        public TCPClient(MmsLogger logger) {
            _logger = logger;
            _messageLookupContainer = new NetworkMessageLookup(logger, this).MessageLookupContainer;
            _heartbeatTimer = new System.Timers.Timer(_heartbeatFireInterval);
            _heartbeatTimer.Elapsed += ReceiveEvent;
        }

        public void ConnectHost(ClientSideServiceConfiguration host) {
            if (EstablishConnection(host.GetAddress(), int.Parse(host.GetPort()))) {
                SendData(Message.Connect);
                _logger.AppendLine("Sent connect message!");
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
                OpenedTcpClient = new TcpClient(addr, port);
                OpenedTcpClient.Client.ReceiveBufferSize = 64000;
                OpenedTcpClient.Client.ReceiveTimeout = 60000;
                OpenedTcpClient.Client.SendBufferSize = 64000;
                OpenedTcpClient.Client.SendTimeout = 60000;
                _logger.AppendLine("Service link established.");
                stream = OpenedTcpClient.GetStream();
                EstablishedLink = true;
                _enableRead = true;
                _heartbeatTimer.Start();
                ReceiveEvent(null, null);
            } catch (Exception e) {
                _logger.AppendLine($"Could not connect to Server: {e.Message}");
                if (ClientReceiver != null)
                    _netCancelSource.Cancel();
                ClientReceiver = null;
                return false;
            }
            return EstablishedLink;
        }

        public void CloseConnection() {
            if (_netCancelSource != null)
                _netCancelSource.Cancel();
        }

        public void ReceiveEvent(object sender, System.Timers.ElapsedEventArgs elapsedArgs) {
            if (_netCancelSource != null && _netCancelSource.IsCancellationRequested) {
                _heartbeatTimer.Stop();
                if (stream != null) {
                    stream.FlushAsync().Wait();
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }
                if (OpenedTcpClient != null) {
                    OpenedTcpClient.Close();
                    OpenedTcpClient.Dispose();
                    OpenedTcpClient = null;
                }
                EstablishedLink = false;
                _enableRead = false;
                _logger.AppendLine("Successfully disconnected from service.");
                return;
            }
            if (!_enableRead) {
                return;
            }
            Message incomingMsg = new();
            try {
                if (OpenedTcpClient.Client.Available > 0) {
                    byte[] buffer = new byte[4];
                    _enableRead = false;
                    int byteCount = OpenedTcpClient.Client.Receive(buffer);
                    if (byteCount != 4) {
                        _logger.AppendLine("Size chunk was NOT 4 bytes long!");
                    }
                    int expectedLen = BitConverter.ToInt32(buffer, 0);
                    if (expectedLen > 1000000) {
                        buffer = DownloadLargeFile(expectedLen);
                    } else {
                        buffer = new byte[expectedLen];
                        using MemoryStream ms = new MemoryStream();
                        do {
                            byte[] data = new byte[expectedLen > (int)_desiredChunkSize ? (int)_desiredChunkSize : expectedLen];
                            byteCount = OpenedTcpClient.Client.Receive(data, SocketFlags.Partial);
                            ms.Write(data);
                            if ((expectedLen > _desiredChunkSize && byteCount != _desiredChunkSize) || (expectedLen <= _desiredChunkSize && byteCount != expectedLen)) {
                                _logger.AppendLine("TCPClient Recv: Issue with chunk sizing!");
                                return;
                            }
                            expectedLen -= byteCount;
                        } while (expectedLen > 0);
                        buffer = ms.ToArray();
                    }
                    incomingMsg = new Message(buffer);
                    if (incomingMsg == Message.Empty()) {
                        return;
                    }
                    if (incomingMsg.Type == MessageTypes.Disconnect) {
                        FormManager.MainWindow.DisconnectResetClient();
                        CloseConnection();
                    }
                    if (_messageLookupContainer != null && _messageLookupContainer.TryGetValue(incomingMsg.Type, out var msgParser)) {
                        if (msgParser.ProcessMessage(incomingMsg).Result) {
                            if (FormManager.MainWindow.ConfigManager.DebugNetworkOutput) {
                                if (incomingMsg.Type < MessageTypes.ServerStatusRequest) {
                                    _logger.AppendLine($"Service message processed, Type: ({incomingMsg.Type})");
                                }
                            }
                        }
                    }
                    _enableRead = true;
                }
            } catch (Exception e) {
                _logger.AppendLine($"TCPClient error! ServerIndex: {incomingMsg.ServerIndex} MsgType: {Enum.GetName(incomingMsg.Type)} Stacktrace: {e.Message}\n{e.StackTrace}");
                _enableRead = true;
            }
        }

        private byte[] DownloadLargeFile(int expectedLen) {
            int originalLen = expectedLen;
            int byteCount;
            using MemoryStream ms = new MemoryStream();
            if (_progressDialog == null || _progressDialog.IsDisposed) {
                _progressDialog = new(null);
            }
            FormManager.MainWindow.Invoke(_progressDialog.Show);
            _enableRead = false;
            _progressDialog.GetDialogProgress().Report(new("Downloading large file from service...", 0.0));
            double chunkCount = Math.Round(expectedLen / _desiredChunkSize, 0, MidpointRounding.ToPositiveInfinity);
            double currentProgress = 0.0;
            do {
                try {
                    do {
                        byte[] data = new byte[expectedLen > (int)_desiredChunkSize ? (int)_desiredChunkSize : expectedLen];
                        byteCount = OpenedTcpClient.Client.Receive(data, SocketFlags.Partial);
                        ms.Write(data);
                        if ((expectedLen > _desiredChunkSize && byteCount != _desiredChunkSize) || (expectedLen <= _desiredChunkSize && byteCount != expectedLen)) {
                            _logger.AppendLine("TCPClient Recv: Issue with chunk sizing!");
                            return [];
                        }
                        expectedLen -= byteCount;
                    } while (expectedLen > 0);
                } catch (ArgumentOutOfRangeException aoore) {
                    _logger.AppendLine($"Large file attempt, but stream did not contain such a length. Probably mis-read packet. Exception: {aoore.Message}");
                    break;
                }
                _progressDialog?.GetDialogProgress().Report(new($"Downloading large file from service, {(originalLen - expectedLen) / 1024} bytes recieved.", currentProgress));
                expectedLen -= byteCount;
            } while (expectedLen > 0);
            _enableRead = true;
            _progressDialog.EndProgress(new(() => {
                FormManager.MainWindow.Invoke(_progressDialog.Close);
                FormManager.MainWindow.Invoke(_progressDialog.Dispose);
                _progressDialog = null;
            }), 100);
            return ms.ToArray();
        }

        public bool SendData(Message message) {
            int sentCount = 0;
            byte[] sendBytes = message.GetMessageBytes();
            int sendBytesLength = sendBytes.Length;
            if (EstablishedLink) {
                try {
                    do {
                        int sendCount = sentCount + (int)_desiredChunkSize > sendBytesLength ? sendBytesLength - sentCount : (int)_desiredChunkSize;
                        int bytes = OpenedTcpClient.Client.Send(sendBytes, sentCount, sendCount, SocketFlags.Partial);
                        if (bytes != sendCount) {
                            _logger.AppendLine("TCPClient Send: Issue with chunk sizing!");
                            return false;
                        }
                        sentCount += bytes;
                    } while (sentCount != sendBytesLength);
                    _heartbeatFailTimeout = 0;
                    return true;
                } catch {
                    _logger.AppendLine("Error writing to network stream!");
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
            ClientReceiver = null;
            if (OpenedTcpClient != null) {
                OpenedTcpClient.Close();
                OpenedTcpClient.Dispose();
            }
        }
    }
}
