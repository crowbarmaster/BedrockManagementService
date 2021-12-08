using BedrockService.Service.Core.Interfaces;
using BedrockService.Service.Core.Tasks;
using BedrockService.Service.Networking.MessageInterfaces;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;

namespace BedrockService.Service.Networking
{
    public class TCPListener : ITCPListener, IMessageSender
    {
        private TcpClient _client;
        private TcpListener _inListener;
        private NetworkStream _stream;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IBedrockLogger _logger;
        private IServiceTask _tcpTask;
        private IServiceTask _clientListenerTask;
        private IServiceTask _heartbeatTask;
        private int _heartbeatFailTimeout;
        private readonly int _heartbeatFailTimeoutLimit = 2;
        private Dictionary<NetworkMessageTypes, IMessageParser> _standardMessageLookup;
        private Dictionary<NetworkMessageTypes, IFlaggedMessageParser> _flaggedMessageLookup;
        private readonly IPAddress _ipAddress = IPAddress.Parse("0.0.0.0");
        private readonly System.Timers.Timer _reconnectTimer = new System.Timers.Timer(500.0);
        private CommsKeyContainer _keyContainer;
        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

        public TCPListener(IServiceConfiguration serviceConfiguration, IBedrockLogger logger)
        {
            _logger = logger;
            _serviceConfiguration = serviceConfiguration;
            _reconnectTimer.Elapsed += ReconnectTimer_Elapsed;
            _tcpTask = new TCPListenerTask(new Action<CancellationToken>(StartListening), _cancelTokenSource);
            _cancelTokenSource = new CancellationTokenSource();
        }

        private void ReconnectTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _reconnectTimer.Stop();
            _tcpTask = new TCPListenerTask(new Action<CancellationToken>(StartListening), _cancelTokenSource);
        }

        public void SetStrategyDictionaries(Dictionary<NetworkMessageTypes, IMessageParser> standard, Dictionary<NetworkMessageTypes, IFlaggedMessageParser> flagged)
        {
            _standardMessageLookup = standard;
            _flaggedMessageLookup = flagged;
        }

        public void StartListening(CancellationToken token)
        {
            _inListener = new TcpListener(_ipAddress, int.Parse(_serviceConfiguration.GetProp("ClientPort").ToString()));
            try
            {
                _inListener.Start();
            }
            catch (SocketException e)
            {
                _logger.AppendLine($"Error! {e.Message}");
                Thread.Sleep(2000);
                Environment.Exit(1);
            }

            while (true)
            {
                try
                {
                    if (_inListener.Pending())
                    {
                        _cancelTokenSource = new CancellationTokenSource();
                        _client = _inListener.AcceptTcpClient();
                        _stream = _client.GetStream();
                        _clientListenerTask = new ClientServiceTask(new Action<CancellationToken>(IncomingListener), _cancelTokenSource);
                        _heartbeatTask = new HeartbeatTask(new Action<CancellationToken>(HeartbeatSender), _cancelTokenSource);
                    }
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
            _logger.AppendLine("Resetting listener!");
            _cancelTokenSource.Cancel();
            _client.Client.Blocking = false;
            _stream.Close();
            _stream.Dispose();
            _client.Close();
            _inListener.Stop();
            _tcpTask = null;
            _tcpTask = new TCPListenerTask(new Action<CancellationToken>(StartListening), _cancelTokenSource);
            _clientListenerTask = null;
            _heartbeatTask = null;
            
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

            if (_clientListenerTask != null && _clientListenerTask.IsAlive())
            {
                try
                {
                    _stream.Write(byteHeader, 0, byteHeader.Length);
                    _stream.Flush();
                    _heartbeatFailTimeout = 0;
                }
                catch
                {
                    _logger.AppendLine("Error writing to network stream!");
                    _heartbeatFailTimeout++;
                    if (_heartbeatFailTimeout >= _heartbeatFailTimeoutLimit)
                    {
                        ResetListener();
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

        private void IncomingListener(CancellationToken token)
        {
            _logger.AppendLine("Packet listener thread started.");
            int AvailBytes = 0;
            int byteCount = 0;
            NetworkMessageSource msgSource = 0;
            NetworkMessageDestination msgDest = 0;
            byte serverIndex = 0xFF;
            NetworkMessageTypes msgType = 0;
            NetworkMessageFlags msgFlag = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    byte[] buffer = new byte[4];
                    while (_client.Client != null && _client.Client.Available != 0) // Recieve data from client.
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
                        if (msgType == NetworkMessageTypes.Disconnect)
                        {
                            ResetListener();
                        }
                        if (msgType < NetworkMessageTypes.Heartbeat)
                        {
                            try
                            {
                                if (_standardMessageLookup.ContainsKey(msgType))
                                    _standardMessageLookup[msgType].ParseMessage(buffer, serverIndex);
                                else
                                    _flaggedMessageLookup[msgType].ParseMessage(buffer, serverIndex, msgFlag);
                            }
                            catch
                            {
                                if(msgType == NetworkMessageTypes.Connect)
                                    ResetListener();
                            }
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
                    AvailBytes = _client.Client != null ? _client.Client.Available:0;
                }
                catch { }
            }
        }

        private void HeartbeatSender(CancellationToken token)
        {
            _logger.AppendLine("HeartBeatSender started.");
            while (!token.IsCancellationRequested)
            {

                SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Heartbeat);
                int timeWaited = 0;
                while (timeWaited < 3000)
                {
                    Thread.Sleep(100);
                    timeWaited += 100;
                    if (token.IsCancellationRequested)
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
            if (certificate != null)
            {
                byte[] decrypted;
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(_keyContainer.LocalPrivateKey.GetPrivateKey());

                }
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(_keyContainer.RemotePublicKey.GetPrivateKey());

                }
            }
            return true;
        }
    }
}