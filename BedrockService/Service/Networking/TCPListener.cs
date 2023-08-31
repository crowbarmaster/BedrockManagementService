using BedrockService.Service.Networking.Interfaces;
using System.Net;
using System.Net.Sockets;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Networking {
    public class TCPListener : ITCPListener {
        private TcpClient? _client;
        private TcpListener? _inListener;
        private NetworkStream? _stream;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IServerLogger _logger;
        private int _heartbeatFailTimeout;
        private readonly int _heartbeatFailTimeoutLimit = 2;
        private Dictionary<NetworkMessageTypes, IMessageParser>? _standardMessageLookup;
        private Dictionary<NetworkMessageTypes, IFlaggedMessageParser>? _flaggedMessageLookup;
        private readonly IPAddress _ipAddress = IPAddress.Parse("0.0.0.0");
        private CancellationTokenSource _cancelTokenSource = new();
        private Task? _tcpTask;
        private Task? _recieverTask;
        private bool _resettingListener = false;
        private bool _canClientConnect = true;
        private bool _serviceStarted = false;

        public TCPListener(IServiceConfiguration serviceConfiguration, IServerLogger logger, IProcessInfo processInfo) {
            _logger = logger;
            _serviceConfiguration = serviceConfiguration;
            _cancelTokenSource = new CancellationTokenSource();
        }

        public void Initialize() {
            if (!_resettingListener) {
                ResetListener().Wait();
                if (_tcpTask == null) {
                    _tcpTask = StartListening();
                    _recieverTask = IncomingListener();
                    _tcpTask.Start();
                    _resettingListener = false;
                    _canClientConnect = true;
                }
            }
        }

        public Task StartListening() {
            return new Task(() => {
                _inListener = new TcpListener(_ipAddress, _serviceConfiguration.GetProp(ServicePropertyKeys.ClientPort).GetIntValue());
                try {

                    while (_standardMessageLookup == null) { Task.Delay(100).Wait(); }
                    _inListener.Start();
                } catch (SocketException e) {
                    _logger.AppendLine($"Error! {e.Message}");
                    Thread.Sleep(2000);
                    Environment.Exit(1);
                }
                while (!_cancelTokenSource.IsCancellationRequested) {
                    try {
                        if (_inListener != null && _inListener.Pending() && _serviceStarted) {
                            if (_canClientConnect) {
                                _logger.AppendLine("MMS Client has connected to service.");
                                _canClientConnect = false;
                                _cancelTokenSource = new CancellationTokenSource();
                                _client = _inListener.AcceptTcpClient();
                                _stream = _client.GetStream();
                                if (_recieverTask != null) {
                                    _recieverTask.Start();
                                }
                            } else {
                                try {
                                    TcpClient tempClient = _inListener.AcceptTcpClientAsync(_cancelTokenSource.Token).Result;
                                    if (tempClient != null) {
                                        _logger.AppendLine("Client connected before ready, or client already active. Rejected!");
                                        tempClient.GetStream().Write(CreatePacketHeader(new byte[0], NetworkMessageSource.Service, NetworkMessageDestination.Client, 0, NetworkMessageTypes.ClientReject, NetworkMessageFlags.None));
                                        tempClient.GetStream().Flush();
                                        tempClient.GetStream().Close();
                                    }
                                    tempClient.Close();
                                } catch (OperationCanceledException) {
                                }
                            }
                        }
                        Task.Delay(500).Wait();
                    } catch (NullReferenceException) {
                    } catch (InvalidOperationException) {
                        _inListener = null;
                        return;
                    } catch (SocketException) {
                    } catch (Exception e) {
                        _logger.AppendLine(e.ToString());
                    }
                }
                _logger.AppendLine("TCP Listener task canceled!");
                _inListener?.Stop();
                _inListener = null;
            }, _cancelTokenSource.Token);
        }

        public Task CancelAllTasks() {
            return Task.Run(() => {
                _cancelTokenSource.Cancel();
                while (_tcpTask?.Status == TaskStatus.Running || _recieverTask?.Status == TaskStatus.Running) {
                    Task.Delay(100).Wait();
                }
                if (_inListener != null) {
                    _inListener.Stop();
                }
            });
        }

        public void SetStrategyDictionaries(Dictionary<NetworkMessageTypes, IMessageParser> standard, Dictionary<NetworkMessageTypes, IFlaggedMessageParser> flagged) {
            _standardMessageLookup = standard;
            _flaggedMessageLookup = flagged;
        }

        public void SetServiceStarted() => _serviceStarted = true;

        public void SetServiceStopped() => _serviceStarted = false;

        private Task ResetListener() {
            return Task.Run(() => {
                if (!_resettingListener) {
                    _resettingListener = true;
                    _inListener?.Stop();
                    _logger?.AppendLine("Resetting listener!");
                    _stream?.Close();
                    _stream?.Dispose();
                    _client?.Close();
                    _cancelTokenSource?.Cancel();
                    while (_tcpTask?.Status == TaskStatus.Running || _recieverTask?.Status == TaskStatus.Running) {
                        Task.Delay(100).Wait();
                    }
                }
                _cancelTokenSource = new CancellationTokenSource();
                _tcpTask = null;
                _inListener = null;
            });
        }

        private void SendData(byte[] bytesToSend, NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type, NetworkMessageFlags status) {
            byte[] byteHeader = CreatePacketHeader(bytesToSend, source, destination, serverIndex, type, status);

            if (_tcpTask?.Status == TaskStatus.Running && _recieverTask?.Status == TaskStatus.Running && !_cancelTokenSource.IsCancellationRequested) {
                try {
                    _stream.Write(byteHeader, 0, byteHeader.Length);
                    _stream.Flush();
                    _heartbeatFailTimeout = 0;
                } catch {
                    if (_cancelTokenSource.IsCancellationRequested) {
                        return;
                    }
                    _logger.AppendLine("Error writing to network stream!");
                    _heartbeatFailTimeout++;
                    if (_heartbeatFailTimeout >= _heartbeatFailTimeoutLimit) {
                        Task.Run(() => Initialize());
                        _heartbeatFailTimeout = 0;
                    }
                }
            }
        }

        private static byte[] CreatePacketHeader(byte[] bytesToSend, NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type, NetworkMessageFlags status) {
            byte[] byteHeader = new byte[9 + bytesToSend.Length];
            byte[] len = BitConverter.GetBytes(5 + bytesToSend.Length);
            Buffer.BlockCopy(len, 0, byteHeader, 0, 4);
            byteHeader[4] = (byte)source;
            byteHeader[5] = (byte)destination;
            byteHeader[6] = serverIndex;
            byteHeader[7] = (byte)type;
            byteHeader[8] = (byte)status;
            Buffer.BlockCopy(bytesToSend, 0, byteHeader, 9, bytesToSend.Length);
            return byteHeader;
        }

        private void SendData((byte[] data, byte srvIndex, NetworkMessageTypes type) tuple) => SendData(tuple.data, NetworkMessageSource.Service, NetworkMessageDestination.Client, tuple.srvIndex, tuple.type, NetworkMessageFlags.None);

        private void SendData(NetworkMessageTypes type) => SendData(Array.Empty<byte>(), NetworkMessageSource.Service, NetworkMessageDestination.Client, 0, type, NetworkMessageFlags.None);

        private Task IncomingListener() {
            return new Task(() => {
                _logger.AppendLine("TCP Client packet listener started.");
                int byteCount = 0;
                NetworkMessageSource msgSource = 0;
                NetworkMessageDestination msgDest = 0;
                byte serverIndex = 0xFF;
                NetworkMessageTypes msgType = 0;
                NetworkMessageFlags msgFlag = 0;
                while (!_cancelTokenSource.IsCancellationRequested) {
                    try {
                        SendData(NetworkMessageTypes.Heartbeat);
                        byte[] buffer = new byte[4];
                        while (_client?.Client != null && _client.Client.Available != 0) // Recieve data from client.
                        {
                            byteCount = _stream.Read(buffer, 0, 4);
                            int expectedLen = BitConverter.ToInt32(buffer, 0);
                            buffer = new byte[expectedLen];
                            byteCount = _stream.Read(buffer, 0, expectedLen);
                            msgSource = (NetworkMessageSource)buffer[0];
                            msgDest = (NetworkMessageDestination)buffer[1];
                            serverIndex = buffer[2];
                            msgType = (NetworkMessageTypes)buffer[3];
                            msgFlag = (NetworkMessageFlags)buffer[4];
                            if (msgType == NetworkMessageTypes.Disconnect) {
                                Task.Run(Initialize);
                            }
                            if (msgType < NetworkMessageTypes.Heartbeat) {
                                try {
                                    while (_standardMessageLookup == null || _flaggedMessageLookup == null) {
                                        Task.Delay(100).Wait();
                                    }
                                    if (_standardMessageLookup.ContainsKey(msgType))
                                        SendData(_standardMessageLookup[msgType].ParseMessage(buffer, serverIndex));
                                    else
                                        SendData(_flaggedMessageLookup[msgType].ParseMessage(buffer, serverIndex, msgFlag));

                                } catch (Exception e) {
                                    _logger.AppendLine($"{e.Message} {e.StackTrace}");
                                }
                            }
                        }
                        Task.Delay(500).Wait();
                    } catch (OutOfMemoryException) {
                        _logger.AppendLine("Out of memory exception thrown.");
                    } catch (ObjectDisposedException) {
                        _logger.AppendLine("Client was disposed!");
                    } catch (InvalidOperationException e) {
                        if (msgType != NetworkMessageTypes.ConsoleLogUpdate) {
                            _logger.AppendLine(e.Message);
                            _logger.AppendLine(e.StackTrace);
                        }
                    } catch (Exception e) {
                        _logger.AppendLine($"Error: {e.Message} {e.StackTrace}");
                    }
                }
                _logger.AppendLine("TCP Client packet listener canceled!");
            }, _cancelTokenSource.Token);
        }

        
    }
}