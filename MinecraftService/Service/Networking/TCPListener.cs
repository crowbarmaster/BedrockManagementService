using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Networking {
    public class TCPListener : ITCPObject {
        private TcpClient? _client;
        private TcpListener? _inListener;
        private NetworkStream? _stream;
        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly MmsLogger _logger;
        private int _heartbeatFailTimeout;
        private readonly int _heartbeatFailTimeoutLimit = 2;
        private Dictionary<MessageTypes, IMessageParser>? _messageLookup;
        private readonly IPAddress _ipAddress = IPAddress.Parse("0.0.0.0");
        private CancellationTokenSource _cancelTokenSource = new();
        private System.Timers.Timer ReceiveTimer;
        private System.Timers.Timer TcpTimer;
        private DateTime _lastMessageReceived;
        private bool _resettingListener = false;
        private bool _canClientConnect = true;
        private bool _serviceStarted = false;
        private bool _clientActive = false;

        public TCPListener(ServiceConfigurator serviceConfiguration, MmsLogger logger, ProcessInfo processInfo) {
            _logger = logger;
            _serviceConfiguration = serviceConfiguration;
            _cancelTokenSource = new CancellationTokenSource();
            ReceiveTimer = new(100);
            TcpTimer = new(500);
            TcpTimer.Elapsed += TcpTimer_Elapsed;
            ReceiveTimer.Elapsed += ReceiveTimer_Elapsed;
        }

        private void TcpTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs _) {
            if (_cancelTokenSource.IsCancellationRequested) {
                _logger.AppendLine("TCP Listener task canceled!");
                Initialize();
                return;
            }
            try {
                if (_canClientConnect && _inListener != null && _inListener.Pending() && _serviceStarted) {
                    _logger.AppendLine("MMS Client has connected to service.");
                    _resettingListener = false;
                    _canClientConnect = false;
                    _lastMessageReceived = DateTime.Now;
                    _cancelTokenSource = new CancellationTokenSource();
                    _client = _inListener.AcceptTcpClient();
                    _stream = _client.GetStream();
                    _clientActive = true;
                    ReceiveTimer.Start();
                }
            } catch (NullReferenceException) {
            } catch (InvalidOperationException) {
                _inListener = null;
                return;
            } catch (SocketException) {
            } catch (Exception e) {
                _logger.AppendLine(e.ToString());
            }

        }

        private void ReceiveTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs _) {
            int byteCount = 0;
            Message incomingMsg = new();
            if (_cancelTokenSource.IsCancellationRequested) {
                ReceiveTimer.Stop();
                _logger.AppendLine("TCP Client packet listener canceled!");
                return;
            }
            if (_clientActive && DateTime.Now > _lastMessageReceived + TimeSpan.FromSeconds(5)) {
                _cancelTokenSource.Cancel();
                return;
            }
            try {
                byte[] buffer = new byte[4];
                if (_client?.Client != null && _client.Client.Available != 0 && !_cancelTokenSource.IsCancellationRequested) {
                    byteCount = _stream.Read(buffer, 0, 4);
                    int expectedLen = BitConverter.ToInt32(buffer, 0);
                    buffer = new byte[expectedLen];
                    using MemoryStream ms = new MemoryStream();
                    do {
                        byteCount = _stream.Read(buffer, 0, expectedLen);
                        ms.Write(buffer, 0, byteCount);
                        expectedLen -= byteCount;
                    } while (expectedLen > 0);
                    buffer = ms.ToArray();
                    try {
                        incomingMsg = new Message(buffer);
                        _lastMessageReceived = DateTime.Now;
                    } catch (Exception ex) {
                        _logger.AppendLine(ex.Message);
                    }
                    if (incomingMsg == Message.Empty()) {
                        return;
                    }
                    if (incomingMsg.Type == MessageTypes.Disconnect) {
                        SendData(new() { Type = MessageTypes.Disconnect });
                        Task.Run(Initialize);
                    }
                    if (incomingMsg.Type < MessageTypes.Heartbeat) {
                        try {
                            if (_messageLookup.TryGetValue(incomingMsg.Type, out IMessageParser parser)) {
                                SendData(parser.ParseMessage(incomingMsg));
                            }
                        } catch (Exception e) {
                            _logger.AppendLine($"TCPListener ParseMessage (MsgType: {incomingMsg.Type}) event caught error:\n{e.Message}\n{e.StackTrace}");
                        }
                    }
                }
            } catch (OutOfMemoryException) {
                _logger.AppendLine("Out of memory exception thrown.");
            } catch (ObjectDisposedException) {
                _logger.AppendLine("Client was disposed!");
            } catch (InvalidOperationException ex) {
                if (incomingMsg.Type != MessageTypes.ConsoleLogUpdate) {
                    _logger.AppendLine(ex.Message);
                    _logger.AppendLine(ex.StackTrace);
                }
            } catch (Exception e) {
                _logger.AppendLine($"Error: {e.Message} {e.StackTrace}");
            }
        }

        public void Initialize() {
            if (!_resettingListener) { 
                _inListener?.Stop();
                ReceiveTimer.Stop();
                TcpTimer.Stop();
                _logger?.AppendLine("Resetting listener!");
                _stream?.Close();
                _stream?.Dispose();
                _client?.Close();
                _cancelTokenSource?.Cancel();
                _clientActive = false;
                _cancelTokenSource = new CancellationTokenSource();
                _inListener = new TcpListener(_ipAddress, _serviceConfiguration.GetProp(ServicePropertyKeys.ClientPort).GetIntValue());
                try {

                    while (_messageLookup == null) { Task.Delay(100).Wait(); }
                    _inListener.Start();
                    TcpTimer.Start();
                    ReceiveTimer.Start();
                } catch (SocketException e) {
                    _logger.AppendLine($"Error! {e.Message}");
                    Thread.Sleep(2000);
                    Environment.Exit(1);
                }
            }
            _canClientConnect = true;
        }

        public Task CancelAllTasks() {
            return Task.Run(() => {
                _cancelTokenSource.Cancel();
            });
        }

        public void SetStrategies(Dictionary<MessageTypes, IMessageParser> strategies) {
            _messageLookup = strategies;
        }

        public void SetServiceStarted() => _serviceStarted = true;

        public void SetServiceStopped() => _serviceStarted = false;

        private void SendData(Message message) {
            byte[] sendBytes = message.GetMessageBytes();
            if (_client != null && _clientActive && !_cancelTokenSource.IsCancellationRequested) {
                try {
                    _stream.Write(sendBytes, 0, sendBytes.Length);
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
    }
}