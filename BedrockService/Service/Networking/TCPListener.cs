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

namespace BedrockService.Service.Networking
{
    public class TCPListener : ITCPListener, IMessageSender
    {
        private TcpClient client;
        private TcpListener inListener;
        private NetworkStream stream;
        private readonly IServiceConfiguration serviceConfiguration;
        private readonly ILogger logger;
        private IServiceThread tcpThread;
        private IServiceThread clientThread;
        private IServiceThread heartbeatThread;
        private bool heartbeatRecieved = false;
        private bool firstHeartbeatRecieved = false;
        private bool keepAlive = false;
        private int heartbeatFailTimeout;
        private readonly int heartbeatFailTimeoutLimit = 200;
        private Dictionary<NetworkMessageTypes, IMessageParser> StandardMessageLookup;
        private Dictionary<NetworkMessageTypes, IFlaggedMessageParser> FlaggedMessageLookup;
        private IPAddress addr = IPAddress.Parse("0.0.0.0");
        private System.Timers.Timer reconnectTimer = new System.Timers.Timer(500.0);

        public TCPListener(IServiceConfiguration serviceConfiguration, ILogger logger)
        {
            this.logger = logger;
            this.serviceConfiguration = serviceConfiguration;
            reconnectTimer.Elapsed += ReconnectTimer_Elapsed;
            tcpThread = new TCPThread(new ThreadStart(StartListening));
        }

        private void ReconnectTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            reconnectTimer.Stop();
            tcpThread = new TCPThread(new ThreadStart(StartListening));
        }

        public void SetStrategyDictionaries(Dictionary<NetworkMessageTypes, IMessageParser> standard, Dictionary<NetworkMessageTypes, IFlaggedMessageParser> flagged)
        {
            StandardMessageLookup = standard;
            FlaggedMessageLookup = flagged;
        }

        public void StartListening()
        {
            inListener = new TcpListener(addr, int.Parse(serviceConfiguration.GetProp("ClientPort").ToString()));
            try
            {
                inListener.Start();
                keepAlive = true;
            }
            catch(SocketException e)
            {
                logger.AppendLine($"Error! {e.Message}");
                Thread.Sleep(2000);
                //Environment.Exit(1);
            }

            while (true)
            {
                try
                {
                    client = inListener.AcceptTcpClient();
                    stream = client.GetStream();
                    clientThread = new ClientServiceThread(new ThreadStart(IncomingListener));
                    heartbeatThread = new HeartbeatThread(new ThreadStart(SendBackHeatbeatSignal));
                }
                catch (ThreadStateException) { }
                catch (NullReferenceException) { }
                catch (InvalidOperationException) { }
                catch (SocketException) { }
                catch (Exception e)
                {
                    logger.AppendLine(e.ToString());
                }
            }
        }

        public void ResetListener()
        {
            keepAlive = false;
            while (heartbeatThread.IsAlive())
            {
                Thread.Sleep(300);
            }
            stream.Close();
            stream.Dispose();
            client.Client.Blocking = false;
            inListener.Stop();
            tcpThread.CloseThread();
            tcpThread = null;
            StartListening();
        }

        public void SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type, NetworkMessageFlags status)
        {
            byte[] compiled = new byte[9 + bytes.Length];
            byte[] len = BitConverter.GetBytes(5 + bytes.Length);
            Buffer.BlockCopy(len, 0, compiled, 0, 4);
            compiled[4] = (byte)source;
            compiled[5] = (byte)destination;
            compiled[6] = serverIndex;
            compiled[7] = (byte)type;
            compiled[8] = (byte)status;
            Buffer.BlockCopy(bytes, 0, compiled, 9, bytes.Length);
            if (clientThread.IsAlive())
            {
                try
                {
                    stream.Write(compiled, 0, compiled.Length);
                    stream.Flush();
                }
                catch
                {
                    logger.AppendLine("Error writing to network stream!");
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
            keepAlive = true;
            logger.AppendLine("Packet listener thread started.");
            int AvailBytes = 0;
            int byteCount = 0;
            NetworkMessageSource msgSource = 0;
            NetworkMessageDestination msgDest = 0;
            byte serverIndex = 0xFF;
            NetworkMessageTypes msgType = 0;
            NetworkMessageFlags msgFlag = 0;
            while (keepAlive)
            {
                try
                {
                    byte[] buffer = new byte[4];
                    while (client.Client.Available != 0) // Recieve data from client.
                    {
                        byteCount = stream.Read(buffer, 0, 4);
                        int expectedLen = BitConverter.ToInt32(buffer, 0);
                        buffer = new byte[expectedLen];
                        byteCount = stream.Read(buffer, 0, expectedLen);
                        msgSource = (NetworkMessageSource)buffer[0];
                        msgDest = (NetworkMessageDestination)buffer[1];
                        serverIndex = buffer[2];
                        msgType = (NetworkMessageTypes)buffer[3];
                        msgFlag = (NetworkMessageFlags)buffer[4];
                        if (msgType == NetworkMessageTypes.Heartbeat)
                        {
                            if (!firstHeartbeatRecieved)
                                firstHeartbeatRecieved = true;
                            heartbeatRecieved = true;
                        }
                        if (msgType == NetworkMessageTypes.Disconnect)
                        {
                            ResetListener();
                        }
                        if (msgType < NetworkMessageTypes.Heartbeat)
                        {
                            if (StandardMessageLookup.ContainsKey(msgType))
                                StandardMessageLookup[msgType].ParseMessage(buffer, serverIndex);
                            else
                                FlaggedMessageLookup[msgType].ParseMessage(buffer, serverIndex, msgFlag);
                        }
                    }
                    Thread.Sleep(200);
                }
                catch (OutOfMemoryException)
                {
                    logger.AppendLine("Out of memory exception thrown.");
                }
                catch (ObjectDisposedException)
                {
                    logger.AppendLine("Client was disposed! Killing thread...");
                    break;
                }
                catch (InvalidOperationException e)
                {
                    if (msgType != NetworkMessageTypes.ConsoleLogUpdate)
                    {
                        logger.AppendLine(e.Message);
                        logger.AppendLine(e.StackTrace);
                    }
                }
                catch (ThreadAbortException)
                {
                    logger.AppendLine("ListenerThread aborted!");
                }
                catch (Exception e)
                {
                    logger.AppendLine($"Error: {e.Message} {e.StackTrace}");
                }
                try
                {
                    AvailBytes = client.Client.Available;
                }
                catch { }
                if (!clientThread.IsAlive())
                    clientThread.CloseThread();
            }
        }

        private void SendBackHeatbeatSignal()
        {
            logger.AppendLine("HeartBeatSender started.");
            while (keepAlive)
            {
                heartbeatRecieved = false;
                while (!heartbeatRecieved && keepAlive)
                {
                    Thread.Sleep(100);
                    heartbeatFailTimeout++;
                    if (heartbeatFailTimeout > heartbeatFailTimeoutLimit)
                    {
                        if (!firstHeartbeatRecieved)
                        {
                            try
                            {
                                SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Heartbeat);
                                heartbeatFailTimeout = 0;
                            }
                            catch (Exception e)
                            {
                                logger.AppendLine($"HeartBeatSender exited with error: {e.Message}");
                                return;
                            }
                        }
                    }
                }
                heartbeatRecieved = false;
                heartbeatFailTimeout = 0;
                SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Heartbeat);
                int timeWaited = 0;
                while (timeWaited < 3000)
                {
                    Thread.Sleep(100);
                    timeWaited += 100;
                    if (!keepAlive)
                    {
                        logger.AppendLine("HeartBeatSender exited.");
                        return;
                    }
                }
            }
            logger.AppendLine("HeartBeatSender exited.");
        }
    }
}