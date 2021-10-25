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
        private IServiceThread clientThread;
        private IServiceThread heartbeatThread;
        private bool heartbeatRecieved = false;
        private bool firstHeartbeatRecieved = false;
        private bool keepAlive = false;
        private int heartbeatFailTimeout;
        private readonly int heartbeatFailTimeoutLimit = 200;
        private Dictionary<NetworkMessageTypes, IMessageParser> StandardMessageLookup;
        private Dictionary<NetworkMessageTypes, IFlaggedMessageParser> FlaggedMessageLookup;

        public TCPListener(IServiceConfiguration serviceConfiguration, ILogger logger)
        {
            this.logger = logger;
            this.serviceConfiguration = serviceConfiguration;
        }

        public void SetStrategyDictionaries(Dictionary<NetworkMessageTypes, IMessageParser> standard, Dictionary<NetworkMessageTypes, IFlaggedMessageParser> flagged)
        {
            StandardMessageLookup = standard;
            FlaggedMessageLookup = flagged;
        }

        public void StartListening()
        {
            IPAddress addr = IPAddress.Parse("0.0.0.0");
            inListener = new TcpListener(addr, int.Parse(serviceConfiguration.GetProp("ClientPort").ToString()));
            try
            {
                inListener.Start();
                keepAlive = true;
            }
            catch
            {
                logger.AppendLine("Error! Port is occupied and cannot be opened... Program will be killed!");
                Thread.Sleep(2000);
                Environment.Exit(1);
            }

            while (keepAlive)
            {
                try
                {
                    client = inListener.AcceptTcpClient();
                    stream = client.GetStream();
                    clientThread = new ClientServiceThread(new ThreadStart(IncomingListener));
                    heartbeatThread = new HeartbeatThread(new ThreadStart(SendBackHeatbeatSignal));
                }
                catch (ThreadStateException) { }
                catch (Exception e)
                {
                    logger.AppendLine(e.ToString());
                }
            }
            inListener.Stop();
        }

        public void StopListening()
        {
            clientThread.CloseThread();
            stream.Close();
            client.Close();
            inListener.Stop();
            inListener = null;
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
                    AvailBytes = client.Client.Available;
                    while (AvailBytes != 0) // Recieve data from client.
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
                            StopListening();
                        }
                        if (msgType < NetworkMessageTypes.Heartbeat)
                        {
                            if (StandardMessageLookup.ContainsKey(msgType))
                                StandardMessageLookup[msgType].ParseMessage(buffer, serverIndex);
                            else
                                FlaggedMessageLookup[msgType].ParseMessage(buffer, serverIndex, msgFlag);
                        }
                        AvailBytes = client.Client.Available;
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
                AvailBytes = client.Client.Available;
                if (!clientThread.IsAlive())
                    clientThread.CloseThread();
            }
        }

        private void SendBackHeatbeatSignal()
        {
            logger.AppendLine("HeartBeatSender started.");
            while (heartbeatThread.IsAlive())
            {
                heartbeatRecieved = false;
                while (!heartbeatRecieved)
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
                Thread.Sleep(3000);
            }
            logger.AppendLine("HeartBeatSender exited.");
        }
    }
}