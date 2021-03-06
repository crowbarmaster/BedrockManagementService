using BedrockManagementServiceASP.BedrockService.Networking.MessageInterfaces;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace BedrockManagementServiceASP.BedrockService.Networking {
    public class TCPListener : ITCPListener, IMessageSender {
        private TcpClient? _client;
        private TcpListener? _inListener;
        private NetworkStream? _stream;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IBedrockLogger _logger;
        private int _heartbeatFailTimeout;
        private readonly int _heartbeatFailTimeoutLimit = 2;
        private Dictionary<NetworkMessageTypes, IMessageParser>? _standardMessageLookup;
        private Dictionary<NetworkMessageTypes, IFlaggedMessageParser>? _flaggedMessageLookup;
        private readonly IPAddress _ipAddress = IPAddress.Parse("0.0.0.0");
        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private Task? _tcpTask;
        private Task? _recieverTask;
        private bool _resettingListener = false;
        private bool _canClientConnect = true;

        public TCPListener(IServiceConfiguration serviceConfiguration, IBedrockLogger logger, IProcessInfo processInfo) {
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
                }
            }
        }

        public void BlockClientConnections() {
            _canClientConnect = false;
        }

        public void UnblockClientConnections() {
            _canClientConnect = true;
        }

        public void SetStrategyDictionaries(Dictionary<NetworkMessageTypes, IMessageParser> standard, Dictionary<NetworkMessageTypes, IFlaggedMessageParser> flagged) {
            _standardMessageLookup = standard;
            _flaggedMessageLookup = flagged;
        }

        public Task StartListening() {
            return new Task(() => {
                _logger.AppendLine("TCP listener task started.");
                _inListener = new TcpListener(_ipAddress, int.Parse(_serviceConfiguration.GetProp("ClientPort").ToString()));
                try {

                    while (_standardMessageLookup == null) { Task.Delay(100).Wait(); }
                    _inListener.Start();
                } catch (SocketException e) {
                    _logger.AppendLine($"Error! {e.Message}");
                    Thread.Sleep(2000);
                    Environment.Exit(1);
                }
                while (true) {
                    try {
                        if (_inListener != null && _inListener.Pending() && _canClientConnect) {
                            _cancelTokenSource = new CancellationTokenSource();
                            _client = _inListener.AcceptTcpClient();
                            _stream = _client.GetStream();
                            if (_recieverTask != null) {
                                _recieverTask.Start();
                            }
                        }

                        if (_cancelTokenSource.IsCancellationRequested) {
                            _logger.AppendLine("TCP Listener task canceled!");
                            if (_inListener != null) {
                                _inListener.Stop();
                            }
                            _inListener = null;
                            return;
                        }
                        Task.Delay(500).Wait();
                    } catch (NullReferenceException) { } catch (InvalidOperationException) {
                        _inListener = null;
                        return;
                    } catch (SocketException) { } catch (Exception e) {
                        _logger.AppendLine(e.ToString());
                    }
                }
            }, _cancelTokenSource.Token);
        }

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

        public void SendData(byte[] bytesToSend, NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type, NetworkMessageFlags status) {
            byte[] byteHeader = new byte[9 + bytesToSend.Length];
            byte[] len = BitConverter.GetBytes(5 + bytesToSend.Length);
            Buffer.BlockCopy(len, 0, byteHeader, 0, 4);
            byteHeader[4] = (byte)source;
            byteHeader[5] = (byte)destination;
            byteHeader[6] = serverIndex;
            byteHeader[7] = (byte)type;
            byteHeader[8] = (byte)status;
            Buffer.BlockCopy(bytesToSend, 0, byteHeader, 9, bytesToSend.Length);

            if (_tcpTask?.Status == TaskStatus.Running && _recieverTask?.Status == TaskStatus.Running) {
                try {
                    _stream.Write(byteHeader, 0, byteHeader.Length);
                    _stream.Flush();
                    _heartbeatFailTimeout = 0;
                } catch {
                    _logger.AppendLine("Error writing to network stream!");
                    _heartbeatFailTimeout++;
                    if (_heartbeatFailTimeout >= _heartbeatFailTimeoutLimit) {
                        Initialize();
                        _heartbeatFailTimeout = 0;
                    }
                }
            }
        }

        public void SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type) => SendData(bytes, source, destination, serverIndex, type, NetworkMessageFlags.None);

        public void SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type) => SendData(bytes, source, destination, 0xFF, type, NetworkMessageFlags.None);

        public void SendData(NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type) => SendData(new byte[0], source, destination, 0xFF, type, NetworkMessageFlags.None);

        public void SendData(NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type) => SendData(new byte[0], source, destination, serverIndex, type, NetworkMessageFlags.None);

        public void SendData(NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type, NetworkMessageFlags status) => SendData(new byte[0], source, destination, 0xFF, type, status);

        private Task IncomingListener() {
            return new Task(() => {
                _logger.AppendLine("TCP Client packet listener started.");
                int AvailBytes = 0;
                int byteCount = 0;
                NetworkMessageSource msgSource = 0;
                NetworkMessageDestination msgDest = 0;
                byte serverIndex = 0xFF;
                NetworkMessageTypes msgType = 0;
                NetworkMessageFlags msgFlag = 0;
                while (true) {
                    SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Heartbeat);
                    if (_cancelTokenSource.IsCancellationRequested) {
                        _logger.AppendLine("TCP Client packet listener canceled!");
                        return;
                    }
                    try {
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
                                        _standardMessageLookup[msgType].ParseMessage(buffer, serverIndex);
                                    else
                                        _flaggedMessageLookup[msgType].ParseMessage(buffer, serverIndex, msgFlag);

                                } catch { }
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
                    try {
                        AvailBytes = _client.Client != null ? _client.Client.Available : 0;
                    } catch { }
                }
            }, _cancelTokenSource.Token);
        }
    }
}