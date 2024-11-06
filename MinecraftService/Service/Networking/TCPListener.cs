using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Networking
{
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
        private Task? _tcpTask;
        private Task? _recieverTask;
        private bool _resettingListener = false;
        private bool _canClientConnect = true;
        private bool _serviceStarted = false;

        public TCPListener(ServiceConfigurator serviceConfiguration, MmsLogger logger, ProcessInfo processInfo) {
            _logger = logger;
            _serviceConfiguration = serviceConfiguration;
            _cancelTokenSource = new CancellationTokenSource();
        }

        public void Initialize() {
            if (!_resettingListener) {
                ResetListener().Wait();
                if (_tcpTask == null) {
                    _tcpTask = Begin();
                    _recieverTask = IncomingListener();
                    _tcpTask.Start();
                    _resettingListener = false;
                    _canClientConnect = true;
                }
            }
        }

        public Task Begin() {
            return new Task(() => {
                _inListener = new TcpListener(_ipAddress, _serviceConfiguration.GetProp(ServicePropertyKeys.ClientPort).GetIntValue());
                try {

                    while (_messageLookup == null) { Task.Delay(100).Wait(); }
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
                                        tempClient.GetStream().Write((new Message { Type = MessageTypes.ClientReject }).GetMessageBytes());
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

        public void SetStrategies(Dictionary<MessageTypes, IMessageParser> strategies) {
            _messageLookup = strategies;
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

        private void SendData(Message message) {
            byte[] sendBytes = message.GetMessageBytes();

            if (_tcpTask?.Status == TaskStatus.Running && _recieverTask?.Status == TaskStatus.Running && !_cancelTokenSource.IsCancellationRequested) {
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

        private Task IncomingListener() {
            return new Task(() => {
                _logger.AppendLine("TCP Client packet listener started.");
                int byteCount = 0;
                Message incomingMsg = new();
                while (!_cancelTokenSource.IsCancellationRequested) {
                    try {
                        SendData(Message.Heartbeat);
                        byte[] buffer = new byte[4];
                        while (_client?.Client != null && _client.Client.Available != 0) // Recieve data from client.
                        {
                            byteCount = _stream.Read(buffer, 0, 4);
                            int expectedLen = BitConverter.ToInt32(buffer, 0);
                            buffer = new byte[expectedLen];
                            byteCount = _stream.Read(buffer, 0, expectedLen);
                            try {
                                incomingMsg = new Message(buffer);
                            } catch (Exception ex) {
                                _logger.AppendLine(ex.Message);
                            }
                            if (incomingMsg == Message.Empty()) {
                                continue;
                            }
                            if (incomingMsg.Type == MessageTypes.Disconnect) {
                                Task.Run(Initialize);
                            }
                            if (incomingMsg.Type < MessageTypes.Heartbeat) {
                                try {
                                    while (_messageLookup == null) {
                                        Task.Delay(100).Wait();
                                    }
                                    if (_messageLookup.ContainsKey(incomingMsg.Type))
                                        SendData(_messageLookup[incomingMsg.Type].ParseMessage(incomingMsg));

                                } catch (Exception e) {
                                    _logger.AppendLine($"TCPListener ParseMessage (MsgType: {incomingMsg.Type}) event caught error:\n{e.Message}\n{e.StackTrace}");
                                }
                            }
                        }
                         Task.Delay(500).Wait();
                    } catch (OutOfMemoryException) {
                        _logger.AppendLine("Out of memory exception thrown.");
                    } catch (ObjectDisposedException) {
                        _logger.AppendLine("Client was disposed!");
                    } catch (InvalidOperationException e) {
                        if (incomingMsg.Type != MessageTypes.ConsoleLogUpdate) {
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