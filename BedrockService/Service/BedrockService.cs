using BedrockService.Service.Networking;
using BedrockService.Service.Server;
using BedrockService.Service.Server.HostInfoClasses;
using NCrontab;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Topshelf;
using Topshelf.Logging;

namespace BedrockService.Service
{
    public class BedrockService : HostInfo, ServiceControl
    {
        public enum ServiceStatus
        {
            Stopped,
            Starting,
            Started,
            Stopping
        }
        public List<BedrockServer> bedrockServers = new List<BedrockServer>();
        public HostControl _hostControl;
        public ServiceStatus CurrentServiceStatus;
        private System.Timers.Timer updaterTimer;
        private System.Timers.Timer cronTimer;
        private readonly CrontabSchedule shed;
        private Thread TCPServerThread;
        private readonly CrontabSchedule updater;
        private readonly TCPListener ClientHost = new TCPListener();

        public BedrockService()
        {
            CurrentServiceStatus = ServiceStatus.Starting;
            if (LoadInit())
            {
                TCPServerThread = new Thread(new ThreadStart(ClientHostThread));
                TCPServerThread.Start();
                try
                {
                    foreach (ServerInfo Server in InstanceProvider.GetHostInfo().GetServerInfos())
                    {
                        BedrockServer srv = new BedrockServer(Server);
                        shed = CrontabSchedule.TryParse(InstanceProvider.GetHostInfo().GetGlobalValue("BackupCron"));
                        if (InstanceProvider.GetHostInfo().GetGlobalValue("BackupEnabled") == "true" && shed != null)
                        {
                            cronTimer = new System.Timers.Timer((shed.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds);
                            cronTimer.Elapsed += CronTimer_Elapsed;
                            cronTimer.Start();
                        }

                        updater = CrontabSchedule.TryParse(InstanceProvider.GetHostInfo().GetGlobalValue("UpdateCron"));
                        if (InstanceProvider.GetHostInfo().GetGlobalValue("CheckUpdates") == "true" && updater != null)
                        {
                            updaterTimer = new System.Timers.Timer((updater.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds);
                            updaterTimer.Elapsed += UpdateTimer_Elapsed;
                            InstanceProvider.GetServiceLogger().AppendLine($"Updates Enabled, will be checked in: {((float)updaterTimer.Interval / 1000)} seconds.");
                            updaterTimer.Start();
                        }
                        bedrockServers.Add(srv);
                    }
                    CurrentServiceStatus = ServiceStatus.Started;
                }
                catch (Exception e)
                {
                    InstanceProvider.GetServiceLogger().AppendLine($"Error Instantiating BedrockServiceWrapper: {e.StackTrace}");
                }
            }
        }
        private void ClientHostThread()
        {
            try
            {
                ClientHost.StartListening(int.Parse((string)InstanceProvider.GetHostInfo().GetGlobalValue("ClientPort")));
                InstanceProvider.GetServiceLogger().AppendLine("Stop socket service");
                ClientHost.client.Close();
                GC.Collect();
            }
            catch (ThreadAbortException abort)
            {
                InstanceProvider.GetServiceLogger().AppendLine($"WCF Thread reports {abort.Message}");
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
                if ((string)InstanceProvider.GetHostInfo().GetGlobalValue("BackupEnabled") == "true" && shed != null)
                {
                    Backup();

                    cronTimer = new System.Timers.Timer((shed.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds);
                    cronTimer.Elapsed += CronTimer_Elapsed;
                    cronTimer.Start();
                }
            }
            catch (Exception ex)
            {
                InstanceProvider.GetServiceLogger().AppendLine($"Error in BackupTimer_Elapsed {ex}");
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
                Task<bool> task = Updater.CheckUpdates();
                task.Wait();
                if (InstanceProvider.GetHostInfo().GetGlobalValue("CheckUpdates") == "true" && updater != null && task.Result)
                {
                    if (Updater.VersionChanged)
                    {
                        InstanceProvider.GetServiceLogger().AppendLine("Version change detected! Restarting server(s) to apply update...");
                        if (Stop(_hostControl))
                        {
                            Start(_hostControl);
                        }
                    }

                    updaterTimer = new System.Timers.Timer((updater.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds);
                    updaterTimer.Elapsed += UpdateTimer_Elapsed;
                    updaterTimer.Start();
                }
            }
            catch (Exception ex)
            {
                InstanceProvider.GetServiceLogger().AppendLine($"Error in UpdateTimer_Elapsed {ex}");
            }
        }

        private void Backup()
        {
            InstanceProvider.GetServiceLogger().AppendLine("Service started backup manager.");
            foreach (var brs in bedrockServers.OrderByDescending(t => t.serverInfo.Primary).ToList())
            {
                brs.CurrentServerStatus = BedrockServer.ServerStatus.Stopping;
                while (brs.CurrentServerStatus == BedrockServer.ServerStatus.Stopping)
                {
                    Thread.Sleep(100);
                }
            }

            foreach (var brs in bedrockServers.OrderByDescending(t => t.serverInfo.Primary).ToList())
            {
                if (brs.CurrentServerStatus == BedrockServer.ServerStatus.Stopped) brs.Backup();

            }
            foreach (var brs in bedrockServers.OrderByDescending(t => t.serverInfo.Primary).ToList())
            {

                brs.StartControl(_hostControl);
                Thread.Sleep(2000);

            }
            InstanceProvider.GetServiceLogger().AppendLine("Backups have been completed.");
        }

        public bool Stop(HostControl hostControl)
        {

            CurrentServiceStatus = ServiceStatus.Stopping;
            _hostControl = hostControl;
            try
            {
                foreach (var brs in bedrockServers)
                {
                    brs.CurrentServerStatus = BedrockServer.ServerStatus.Stopping;
                    while (brs.CurrentServerStatus == BedrockServer.ServerStatus.Stopping)
                    {
                        Thread.Sleep(100);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                InstanceProvider.GetServiceLogger().AppendLine($"Error Stopping BedrockServiceWrapper {e.StackTrace}");
                return false;
            }
        }

        public BedrockServer GetBedrockServer(string name)
        {
            return bedrockServers.FirstOrDefault(brs => brs.serverInfo.ServerName == name);
        }

        public bool Start(HostControl hostControl)
        {
            _hostControl = hostControl;
            try
            {
                ValidSettingsCheck();

                foreach (var brs in bedrockServers.OrderByDescending(t => t.serverInfo.Primary).ToList())
                {
                    _hostControl.RequestAdditionalTime(TimeSpan.FromSeconds(30));
                    brs.CurrentServerStatus = BedrockServer.ServerStatus.Starting;
                    brs.StartWatchdog(_hostControl);
                    Thread.Sleep(2000);
                }
                return true;
            }
            catch (Exception e)
            {
                InstanceProvider.GetServiceLogger().AppendLine($"Error Starting BedrockServiceWrapper {e.StackTrace}");
                return false;
            }
        }

        private void ValidSettingsCheck()
        {
            bool validating = true;
            bool dupedSettingsFound = false;
            while (validating)
            {
                if (InstanceProvider.GetHostInfo().GetServerInfos().Count() < 1)
                {
                    throw new Exception("No Servers Configured");
                }
                else
                {
                    foreach (ServerInfo server in InstanceProvider.GetHostInfo().GetServerInfos())
                    {
                        foreach (ServerInfo compareServer in InstanceProvider.GetHostInfo().GetServerInfos())
                        {
                            if (server != compareServer)
                            {
                                if (server.ServerExeName.Equals(compareServer.ServerExeName) || server.ServerPath.Equals(compareServer.ServerPath) || server.GetServerProp("server-port").Value.Equals(compareServer.GetServerProp("server-port").Value) || server.GetServerProp("server-portv6").Value.Equals(compareServer.GetServerProp("server-portv6").Value) || server.GetServerProp("server-name").Value.Equals(compareServer.GetServerProp("server-name").Value))
                                {
                                    InstanceProvider.GetServiceLogger().AppendLine($"Duplicate server settings between servers {server.ServerName} and {compareServer}.");
                                    dupedSettingsFound = true;
                                }
                            }
                        }
                    }
                    if (dupedSettingsFound)
                        throw new Exception("Duplicate settings found! Check logs.");

                    foreach (var server in InstanceProvider.GetHostInfo().GetServerInfos())
                    {
                        if (Updater.VersionChanged)
                        {
                            ReplaceBuild(server);
                        }
                        if (server.ServerExeName.Value != "bedrock_server.exe" && File.Exists(server.ServerPath.Value + "\\bedrock_server.exe") && !File.Exists(server.ServerPath.Value + "\\" + server.ServerExeName.Value))
                        {
                            File.Copy(server.ServerPath.Value + "\\bedrock_server.exe", server.ServerPath.Value + "\\" + server.ServerExeName.Value);
                        }
                    }
                    if (Updater.VersionChanged)
                        Updater.VersionChanged = false;
                    else
                        validating = false;
                }
            }

        }

        private static void ReplaceBuild(ServerInfo server)
        {
            try
            {
                if (Directory.Exists(server.ServerPath.Value))
                    DeleteFilesRecursively(new DirectoryInfo(server.ServerPath.Value));
                ZipFile.ExtractToDirectory($@"{Program.ServiceDirectory}\Server\MCSFiles\Update.zip", server.ServerPath.Value);
                if (server.ServerExeName.Value != "bedrock_server.exe")
                    File.Copy(server.ServerPath.Value + "\\bedrock_server.exe", server.ServerPath.Value + "\\" + server.ServerExeName.Value);
            }
            catch (Exception e)
            {
                InstanceProvider.GetServiceLogger().AppendLine($"ERROR: Got an exception deleting entire directory! {e.Message}");
            }
        }

        private static void DeleteFilesRecursively(DirectoryInfo source)
        {
            try
            {
                source.Delete(true);
            }
            catch (Exception e)
            {
                InstanceProvider.GetServiceLogger().AppendLine($@"Error Deleting Dir: {e.StackTrace}");
            }
        }

        private bool LoadInit()
        {
            if (InstanceProvider.GetConfigManager().LoadConfigs())
            {
                if (!Updater.CheckUpdates().Result)
                {
                    InstanceProvider.GetServiceLogger().AppendLine("Checking for updates at init failed.");
                    if(!File.Exists($@"{Program.ServiceDirectory}\Server\MCSFiles\Update.zip"))
                        InstanceProvider.GetServiceLogger().AppendLine("An update package was not found. Execution may fail if this is first run!");
                }
                return true;
            }
            return false;
        }
    }
}
