using BedrockService.Service.Core.Interfaces;
using BedrockService.Service.Core.Threads;
using BedrockService.Service.Networking.NetworkMessageClasses;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace BedrockService.Service.Networking
{
    public class TCPListener : ITCPListener, IMessageSender
    {
        private TcpClient _client;
        private TcpListener _inListener;
        private NetworkStream _stream;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly ILogger _logger;
        private IServiceThread _tcpThread;
        private IServiceThread _clientThread;
        private IServiceThread _heartbeatThread;
        private bool _heartbeatRecieved = false;
        private bool _firstHeartbeatRecieved = false;
        private bool _keepAlive = false;
        private int _heartbeatFailTimeout;
        private readonly int _heartbeatFailTimeoutLimit = 200;
        private Dictionary<NetworkMessageTypes, IMessageParser> _standardMessageLookup;
        private Dictionary<NetworkMessageTypes, IFlaggedMessageParser> _flaggedMessageLookup;
        private readonly IPAddress _ipAddress = IPAddress.Parse("0.0.0.0");
        private readonly System.Timers.Timer _reconnectTimer = new System.Timers.Timer(500.0);
        private CommsKeyContainer _keyContainer;

        public TCPListener(IServiceConfiguration serviceConfiguration, ILogger logger)
        {
            _logger = logger;
            _serviceConfiguration = serviceConfiguration;
            _reconnectTimer.Elapsed += ReconnectTimer_Elapsed;
            _tcpThread = new TCPThread(new ThreadStart(StartListening));
        }

        private void ReconnectTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _reconnectTimer.Stop();
            _tcpThread = new TCPThread(new ThreadStart(StartListening));
        }

        public void SetStrategyDictionaries(Dictionary<NetworkMessageTypes, IMessageParser> standard, Dictionary<NetworkMessageTypes, IFlaggedMessageParser> flagged)
        {
            _standardMessageLookup = standard;
            _flaggedMessageLookup = flagged;
        }

        public void StartListening()
        {
            _inListener = new TcpListener(_ipAddress, int.Parse(_serviceConfiguration.GetProp("ClientPort").ToString()));
            try
            {
                _inListener.Start();
                _keepAlive = true;
            }
            catch(SocketException e)
            {
                _logger.AppendLine($"Error! {e.Message}");
                Thread.Sleep(2000);
                //Environment.Exit(1);
            }

            while (true)
            {
                try
                {
                    _client = _inListener.AcceptTcpClient();
                    _stream = _client.GetStream();
                    _clientThread = new ClientServiceThread(new ThreadStart(IncomingListener));
                    _heartbeatThread = new HeartbeatThread(new ThreadStart(SendBackHeatbeatSignal));
                }
                catch (ThreadStateException) { }
                catch (NullReferenceException) { }
                catch (InvalidOperationException) { }
                catch (SocketException) { }
                catch (Exception e)
                {
                    _logger.AppendLine(e.ToString());
                }
            }
        }

        public void ResetListener()
        {
            _keepAlive = false;
            while (_heartbeatThread.IsAlive())
            {
                Thread.Sleep(300);
            }
            _stream.Close();
            _stream.Dispose();
            _client.Client.Blocking = false;
            _inListener.Stop();
            _tcpThread.CloseThread();
            _tcpThread = null;
            StartListening();
        }

        public void SendData(byte[] bytesToSend, NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type, NetworkMessageFlags status)
        {
            byte[] byteHeader = new byte[9 + bytesToSend.Length];
            byte[] len = BitConverter.GetBytes(5 + bytesToSend.Length);
            Buffer.BlockCopy(len, 0, byteHeader, 0, 4);
            byteHeader[4] = (byte)source;
            byteHeader[5] = (byte)destination;
            byteHeader[6] = serverIndex;
            byteHeader[7] = (byte)type;
            byteHeader[8] = (byte)status;
            Buffer.BlockCopy(bytesToSend, 0, byteHeader, 9, bytesToSend.Length);
            
            if (_clientThread.IsAlive())
            {
                try
                {
                    _stream.Write(byteHeader, 0, byteHeader.Length);
                    _stream.Flush();
                }
                catch
                {
                    _logger.AppendLine("Error writing to network stream!");
                }
            }
        }

        public void SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type) => SendData(bytes, source, destination, serverIndex, type, NetworkMessageFlags.None);

        public void SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type) => SendData(bytes, source, destination, 0xFF, type, NetworkMessageFlags.None);

        public void SendData(NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type) => SendData(new byte[0], source, destination, 0xFF, type, NetworkMessageFlags.None);

        public void SendData(NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type) => SendData(new byte[0], source, destination, serverIndex, type, NetworkMessageFlags.None);

        public void SendData(NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type, NetworkMessageFlags status) => SendData(new byte[0], source, destination, 0xFF, type, status);

        private void IncomingListener()
        {
            _keepAlive = true;
            _logger.AppendLine("Packet listener thread started.");
            int AvailBytes = 0;
            int byteCount = 0;
            NetworkMessageSource msgSource = 0;
            NetworkMessageDestination msgDest = 0;
            byte serverIndex = 0xFF;
            NetworkMessageTypes msgType = 0;
            NetworkMessageFlags msgFlag = 0;
            while (_keepAlive)
            {
                try
                {
                    byte[] buffer = new byte[4];
                    while (_client.Client.Available != 0) // Recieve data from client.
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
                        if (msgType == NetworkMessageTypes.Heartbeat)
                        {
                            if (!_firstHeartbeatRecieved)
                                _firstHeartbeatRecieved = true;
                            _heartbeatRecieved = true;
                        }
                        if (msgType == NetworkMessageTypes.Disconnect)
                        {
                            ResetListener();
                        }
                        if (msgType < NetworkMessageTypes.Heartbeat)
                        {
                            if (_standardMessageLookup.ContainsKey(msgType))
                                _standardMessageLookup[msgType].ParseMessage(buffer, serverIndex);
                            else
                                _flaggedMessageLookup[msgType].ParseMessage(buffer, serverIndex, msgFlag);
                        }
                    }
                    Thread.Sleep(200);
                }
                catch (OutOfMemoryException)
                {
                    _logger.AppendLine("Out of memory exception thrown.");
                }
                catch (ObjectDisposedException)
                {
                    _logger.AppendLine("Client was disposed! Killing thread...");
                    break;
                }
                catch (InvalidOperationException e)
                {
                    if (msgType != NetworkMessageTypes.ConsoleLogUpdate)
                    {
                        _logger.AppendLine(e.Message);
                        _logger.AppendLine(e.StackTrace);
                    }
                }
                catch (ThreadAbortException)
                {
                    _logger.AppendLine("ListenerThread aborted!");
                }
                catch (Exception e)
                {
                    _logger.AppendLine($"Error: {e.Message} {e.StackTrace}");
                }
                try
                {
                    AvailBytes = _client.Client.Available;
                }
                catch { }
                if (!_clientThread.IsAlive())
                    _clientThread.CloseThread();
            }
        }

        private void SendBackHeatbeatSignal()
        {
            _logger.AppendLine("HeartBeatSender started.");
            while (_keepAlive)
            {
                _heartbeatRecieved = false;
                while (!_heartbeatRecieved && _keepAlive)
                {
                    Thread.Sleep(100);
                    _heartbeatFailTimeout++;
                    if (_heartbeatFailTimeout > _heartbeatFailTimeoutLimit)
                    {
                        if (!_firstHeartbeatRecieved)
                        {
                            try
                            {
                                SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Heartbeat);
                                _heartbeatFailTimeout = 0;
                            }
                            catch (Exception e)
                            {
                                _logger.AppendLine($"HeartBeatSender exited with error: {e.Message}");
                                return;
                            }
                        }
                    }
                }
                _heartbeatRecieved = false;
                _heartbeatFailTimeout = 0;
                SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Heartbeat);
                int timeWaited = 0;
                while (timeWaited < 3000)
                {
                    Thread.Sleep(100);
                    timeWaited += 100;
                    if (!_keepAlive)
                    {
                        _logger.AppendLine("HeartBeatSender exited.");
                        return;
                    }
                }
            }
            _logger.AppendLine("HeartBeatSender exited.");
        }

        public void SetKeyContainer(CommsKeyContainer keyContainer)
        {
            _keyContainer = keyContainer;
        }

        public bool VerifyClientData(byte[] certificate)
        {
            if(certificate != null)
            {
                byte[] decrypted;
                using(RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(_keyContainer.LocalPrivateKey.GetPrivateKey());
                    
                }
                using(RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(_keyContainer.RemotePublicKey.GetPrivateKey());

                }
            }
            return true;
        }
    }
}