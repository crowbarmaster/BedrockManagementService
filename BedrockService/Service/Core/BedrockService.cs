using BedrockService.Service.Core.Interfaces;
using BedrockService.Service.Management;
using BedrockService.Service.Networking;
using BedrockService.Service.Server;
using BedrockService.Shared.Interfaces;
using BedrockService.Service.Core.Threads;
using NCrontab;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using Topshelf;

namespace BedrockService.Service.Core
{
    public class BedrockService : ServiceControl, IBedrockService
    {
        private enum ServiceStatus
        {
            Stopped,
            Starting,
            Started,
            Stopping
        }
        private readonly IServiceConfiguration ServiceConfiguration;
        private readonly ILogger Logger;
        private readonly IProcessInfo ProcessInfo;
        private readonly IConfigurator Configurator;
        private readonly IUpdater Updater;
        private readonly ITCPListener tCPListener;
        private IServiceThread tcpThread;
        private readonly CrontabSchedule shed;
        private readonly CrontabSchedule updaterCron;
        private HostControl hostControl;
        private List<IBedrockServer> bedrockServers = new List<IBedrockServer>();
        private System.Timers.Timer updaterTimer;
        private System.Timers.Timer cronTimer;

        public BedrockService(IConfigurator configurator, IUpdater updater, ILogger logger, IServiceConfiguration serviceConfiguration, IProcessInfo serviceProcessInfo, ITCPListener tCPListener)
        {
            this.tCPListener = tCPListener;
            Configurator = configurator;
            ServiceConfiguration = serviceConfiguration;
            ProcessInfo = serviceProcessInfo;
            Updater = updater;
            Logger = logger;
            tcpThread = new TCPThread(new ThreadStart(tCPListener.StartListening));
            shed = CrontabSchedule.TryParse(serviceConfiguration.GetProp("BackupCron").ToString());
            updaterCron = CrontabSchedule.TryParse(serviceConfiguration.GetProp("UpdateCron").ToString());
            Initialize();
        }

        public bool Start(HostControl hostControl)
        {
            this.hostControl = hostControl;
            try
            {
                ValidSettingsCheck();

                foreach (var brs in bedrockServers)
                {
                    if(hostControl != null)
                        this.hostControl.RequestAdditionalTime(TimeSpan.FromSeconds(30));
                    brs.SetServerStatus(BedrockServer.ServerStatus.Starting);
                    brs.StartWatchdog(this.hostControl);
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.AppendLine($"Error Starting BedrockServiceWrapper {e.StackTrace}");
                return false;
            }
        }

        public bool Stop(HostControl hostControl)
        {
            this.hostControl = hostControl;
            try
            {
                foreach (var brs in bedrockServers)
                {
                    brs.SetServerStatus(BedrockServer.ServerStatus.Stopping);
                    while (brs.GetServerStatus() == BedrockServer.ServerStatus.Stopping && !Program.IsExiting)
                        Thread.Sleep(100);
                }
                tcpThread.CloseThread();
                tcpThread = null;
                return true;
            }
            catch (Exception e)
            {
                Logger.AppendLine($"Error Stopping BedrockServiceWrapper {e.StackTrace}");
                return false;
            }
        }

        public void RestartService()
        {
            try
            {
                foreach (var brs in bedrockServers)
                {
                    brs.SetServerStatus(BedrockServer.ServerStatus.Stopping);
                    while (brs.GetServerStatus() == BedrockServer.ServerStatus.Stopping && !Program.IsExiting)
                        Thread.Sleep(100);
                }
                try
                {
                    tCPListener.ResetListener();
                }
                catch (ThreadAbortException) { }
                //tcpThread.CloseThread();
                Configurator.LoadAllConfigurations().Wait();
                tcpThread = new TCPThread(new ThreadStart(tCPListener.StartListening));
                Initialize();
                foreach (var brs in bedrockServers)
                {
                    brs.SetServerStatus(BedrockServer.ServerStatus.Starting);
                }
            }
            catch (Exception e)
            {
                Logger.AppendLine($"Error Stopping BedrockServiceWrapper {e.StackTrace}");
            }
        }

        public IBedrockServer GetBedrockServerByIndex(int serverIndex)
        {
            return bedrockServers[serverIndex];
        }

        public IBedrockServer GetBedrockServerByName(string name)
        {
            return bedrockServers.FirstOrDefault(brs => brs.GetServerName() == name);
        }

        public List<IBedrockServer> GetAllServers() => bedrockServers;

        public void InitializeNewServer(IServerConfiguration server)
        {
            IBedrockServer bedrockServer = new BedrockServer(server, Configurator, Logger, ServiceConfiguration, ProcessInfo);
            bedrockServers.Add(bedrockServer);
            ServiceConfiguration.AddNewServerInfo(server);
            if (ValidSettingsCheck())
            {
                bedrockServer.SetServerStatus(BedrockServer.ServerStatus.Starting);
                bedrockServer.StartWatchdog(hostControl);
            }
        }

        private void Initialize()
        {
            bedrockServers = new List<IBedrockServer>();
            if (ServiceConfiguration.GetProp("BackupEnabled").ToString() == "true" && shed != null)
            {
                cronTimer = new System.Timers.Timer((shed.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds);
                cronTimer.Elapsed += CronTimer_Elapsed;
                cronTimer.Start();
            }
            if (ServiceConfiguration.GetProp("CheckUpdates").ToString() == "true" && updaterCron != null)
            {
                updaterTimer = new System.Timers.Timer((updaterCron.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds);
                updaterTimer.Elapsed += UpdateTimer_Elapsed;
                Logger.AppendLine($"Updates Enabled, will be checked in: {((float)updaterTimer.Interval / 1000)} seconds.");
                updaterTimer.Start();
            }
            try
            {
                foreach (IServerConfiguration server in ServiceConfiguration.GetAllServerInfos())
                {
                    IBedrockServer bedrockServer = new BedrockServer(server, Configurator, Logger, ServiceConfiguration, ProcessInfo);
                    bedrockServers.Add(bedrockServer);
                }
            }
            catch (Exception e)
            {
                Logger.AppendLine($"Error Instantiating BedrockServiceWrapper: {e.StackTrace}");
            }
        }

        private void CronTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (cronTimer != null)
                {
                    cronTimer.Stop();
                    cronTimer = null;
                }
                if (ServiceConfiguration.GetProp("BackupEnabled").ToString() == "true" && shed != null)
                {
                    Backup();

                    cronTimer = new System.Timers.Timer((shed.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds);
                    cronTimer.Elapsed += CronTimer_Elapsed;
                    cronTimer.Start();
                }
            }
            catch (Exception ex)
            {
                Logger.AppendLine($"Error in BackupTimer_Elapsed {ex}");
            }
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (updaterTimer != null)
                {
                    updaterTimer.Stop();
                    updaterTimer = null;
                }
                Updater.CheckUpdates().Wait();
                if (ServiceConfiguration.GetProp("CheckUpdates").ToString() == "true" && Updater != null)
                {
                    if (Updater.CheckVersionChanged())
                    {
                        Logger.AppendLine("Version change detected! Restarting server(s) to apply update...");
                        if (Stop(hostControl))
                        {
                            Start(hostControl);
                        }
                    }

                    updaterTimer = new System.Timers.Timer((updaterCron.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds);
                    updaterTimer.Elapsed += UpdateTimer_Elapsed;
                    updaterTimer.Start();
                }
            }
            catch (Exception ex)
            {
                Logger.AppendLine($"Error in UpdateTimer_Elapsed {ex}");
            }
        }

        private void Backup()
        {
            Logger.AppendLine("Service started backup manager.");
            foreach (var brs in bedrockServers)
            {
                brs.RestartServer(true);
            }
            Logger.AppendLine("Backups have been completed.");
        }

        private bool ValidSettingsCheck()
        {
            bool validating = true;
            bool dupedSettingsFound = false;
            while (validating)
            {
                if (ServiceConfiguration.GetAllServerInfos().Count() < 1)
                {
                    throw new Exception("No Servers Configured");
                }
                else
                {
                    foreach (IServerConfiguration server in ServiceConfiguration.GetAllServerInfos())
                    {
                        foreach (IServerConfiguration compareServer in ServiceConfiguration.GetAllServerInfos())
                        {
                            if (server != compareServer)
                            {
                                if (server.GetProp("server-port").Equals(compareServer.GetProp("server-port")) ||
                                    server.GetProp("server-portv6").Equals(compareServer.GetProp("server-portv6")) ||
                                    server.GetProp("server-name").Equals(compareServer.GetProp("server-name")))
                                {
                                    Logger.AppendLine($"Duplicate server settings between servers {server.GetFileName()} and {compareServer.GetFileName()}.");
                                    dupedSettingsFound = true;
                                }
                            }
                        }
                    }
                    if (dupedSettingsFound)
                    {
                        throw new Exception("Duplicate settings found! Check logs.");
                    }
                    foreach (var server in ServiceConfiguration.GetAllServerInfos())
                    {
                        if (Updater.CheckVersionChanged() || !File.Exists(server.GetProp("ServerPath") + "\\bedrock_server.exe"))
                        {
                            Configurator.ReplaceServerBuild(server).Wait();
                        }
                        if (server.GetProp("ServerExeName").ToString() != "bedrock_server.exe" && File.Exists(server.GetProp("ServerPath") + "\\bedrock_server.exe") && !File.Exists(server.GetProp("ServerPath") + "\\" + server.GetProp("ServerExeName")))
                        {
                            File.Copy(server.GetProp("ServerPath") + "\\bedrock_server.exe", server.GetProp("ServerPath") + "\\" + server.GetProp("ServerExeName"));
                        }
                    }
                    if (Updater.CheckVersionChanged())
                        Updater.MarkUpToDate();
                    else
                    {
                        validating = false;
                    }
                }
            }
            return true;
        }

        public void RemoveBedrockServerByIndex(int serverIndex)
        {
            bedrockServers.RemoveAt(serverIndex);
        }
    }
}
