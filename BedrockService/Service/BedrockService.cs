using BedrockService.Service.Networking;
using BedrockService.Service.Server;
using BedrockService.Service.Server.HostInfoClasses;
using NCrontab;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Topshelf;

namespace BedrockService.Service
{
    public class BedrockService : ServiceControl
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
        private readonly Thread TCPServerThread;
        private readonly CrontabSchedule updater;
        private readonly TCPListener ClientHost = new TCPListener();

        public BedrockService()
        {
            if (LoadInit())
            {
                TCPServerThread = new Thread(new ThreadStart(ClientHostThread));
                shed = CrontabSchedule.TryParse(InstanceProvider.HostInfo.GetGlobalValue("BackupCron"));
                updater = CrontabSchedule.TryParse(InstanceProvider.HostInfo.GetGlobalValue("UpdateCron"));
                if (InstanceProvider.HostInfo.GetGlobalValue("BackupEnabled") == "true" && shed != null)
                {
                    cronTimer = new System.Timers.Timer((shed.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds);
                    cronTimer.Elapsed += CronTimer_Elapsed;
                    cronTimer.Start();
                }

                if (InstanceProvider.HostInfo.GetGlobalValue("CheckUpdates") == "true" && updater != null)
                {
                    updaterTimer = new System.Timers.Timer((updater.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds);
                    updaterTimer.Elapsed += UpdateTimer_Elapsed;
                    InstanceProvider.ServiceLogger.AppendLine($"Updates Enabled, will be checked in: {((float)updaterTimer.Interval / 1000)} seconds.");
                    updaterTimer.Start();
                }
                Initialize();
            }
        }

        private bool LoadInit()
        {
            if (InstanceProvider.ConfigManager.LoadConfigs())
            {
                if (!Updater.CheckUpdates().Result)
                {
                    InstanceProvider.ServiceLogger.AppendLine("Checking for updates at init failed.");
                    if (File.Exists($@"{Program.ServiceDirectory}\Server\MCSFiles\bedrock_ver.ini") && !File.Exists($@"{Program.ServiceDirectory}\Server\MCSFiles\Update_{File.ReadAllText($@"{Program.ServiceDirectory}\Server\MCSFiles\bedrock_ver.ini")}.zip"))
                        InstanceProvider.ServiceLogger.AppendLine("An update package was not found. Execution may fail if this is first run!");
                }
                return true;
            }
            return false;
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
                    while (brs.CurrentServerStatus == BedrockServer.ServerStatus.Stopping && !Program.IsExiting)
                        Thread.Sleep(100);
                }
                return true;
            }
            catch (Exception e)
            {
                InstanceProvider.ServiceLogger.AppendLine($"Error Stopping BedrockServiceWrapper {e.StackTrace}");
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
                }
                return true;
            }
            catch (Exception e)
            {
                InstanceProvider.ServiceLogger.AppendLine($"Error Starting BedrockServiceWrapper {e.StackTrace}");
                return false;
            }
        }

        private void Initialize()
        {
            CurrentServiceStatus = ServiceStatus.Starting;
            bedrockServers = new List<BedrockServer>();

            if (!TCPServerThread.IsAlive)
                TCPServerThread.Start();
            try
            {
                foreach (ServerInfo server in InstanceProvider.HostInfo.GetServerInfos())
                {
                    bedrockServers.Add(new BedrockServer(server));
                }
                CurrentServiceStatus = ServiceStatus.Started;
            }
            catch (Exception e)
            {
                InstanceProvider.ServiceLogger.AppendLine($"Error Instantiating BedrockServiceWrapper: {e.StackTrace}");
            }
        }

        public void InitializeNewServer(ServerInfo server)
        {
            BedrockServer brs = new BedrockServer(server);
            bedrockServers.Add(brs);
            InstanceProvider.HostInfo.Servers.Add(server);
            if (ValidSettingsCheck())
            {
                brs.CurrentServerStatus = BedrockServer.ServerStatus.Starting;
                brs.StartWatchdog(_hostControl);
            }
        }

        private void ClientHostThread()
        {
            try
            {
                ClientHost.StartListening(int.Parse((string)InstanceProvider.HostInfo.GetGlobalValue("ClientPort")));
                InstanceProvider.ServiceLogger.AppendLine("Stop socket service");
                ClientHost.client.Close();
                GC.Collect();
            }
            catch (ThreadAbortException abort)
            {
                InstanceProvider.ServiceLogger.AppendLine($"WCF Thread reports {abort.Message}");
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
                if ((string)InstanceProvider.HostInfo.GetGlobalValue("BackupEnabled") == "true" && shed != null)
                {
                    Backup();

                    cronTimer = new System.Timers.Timer((shed.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds);
                    cronTimer.Elapsed += CronTimer_Elapsed;
                    cronTimer.Start();
                }
            }
            catch (Exception ex)
            {
                InstanceProvider.ServiceLogger.AppendLine($"Error in BackupTimer_Elapsed {ex}");
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
                if (InstanceProvider.HostInfo.GetGlobalValue("CheckUpdates") == "true" && updater != null && task.Result)
                {
                    if (Updater.VersionChanged)
                    {
                        InstanceProvider.ServiceLogger.AppendLine("Version change detected! Restarting server(s) to apply update...");
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
                InstanceProvider.ServiceLogger.AppendLine($"Error in UpdateTimer_Elapsed {ex}");
            }
        }

        private void Backup()
        {
            InstanceProvider.ServiceLogger.AppendLine("Service started backup manager.");
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

                brs.CurrentServerStatus = BedrockServer.ServerStatus.Starting;

            }
            InstanceProvider.ServiceLogger.AppendLine("Backups have been completed.");
        }

        private bool ValidSettingsCheck()
        {
            bool validating = true;
            bool dupedSettingsFound = false;
            while (validating)
            {
                if (InstanceProvider.HostInfo.GetServerInfos().Count() < 1)
                {
                    throw new Exception("No Servers Configured");
                }
                else
                {
                    foreach (ServerInfo server in InstanceProvider.HostInfo.GetServerInfos())
                    {
                        foreach (ServerInfo compareServer in InstanceProvider.HostInfo.GetServerInfos())
                        {
                            if (server != compareServer)
                            {
                                if (server.ServerExeName.Value.Equals(compareServer.ServerExeName) || server.ServerPath.Equals(compareServer.ServerPath) || server.GetServerProp("server-port").Value.Equals(compareServer.GetServerProp("server-port").Value) || server.GetServerProp("server-portv6").Value.Equals(compareServer.GetServerProp("server-portv6").Value) || server.GetServerProp("server-name").Value.Equals(compareServer.GetServerProp("server-name").Value))
                                {
                                    InstanceProvider.ServiceLogger.AppendLine($"Duplicate server settings between servers {server.ServerName} and {compareServer}.");
                                    dupedSettingsFound = true;
                                }
                            }
                        }
                    }
                    if (dupedSettingsFound)
                    {
                        throw new Exception("Duplicate settings found! Check logs.");
                    }
                    foreach (var server in InstanceProvider.HostInfo.GetServerInfos())
                    {
                        if (Updater.VersionChanged || !File.Exists(server.ServerPath + "\\bedrock_server.exe"))
                        {
                            if (!ReplaceBuild(server).Wait(60000))
                                InstanceProvider.ServiceLogger.AppendLine("Error! Timeout to replace build exceeded.");
                        }
                        if (server.ServerExeName.Value != "bedrock_server.exe" && File.Exists(server.ServerPath + "\\bedrock_server.exe") && !File.Exists(server.ServerPath + "\\" + server.ServerExeName.Value))
                        {
                            File.Copy(server.ServerPath + "\\bedrock_server.exe", server.ServerPath + "\\" + server.ServerExeName.Value);
                        }
                    }
                    if (Updater.VersionChanged)
                        Updater.VersionChanged = false;
                    else
                    {
                        validating = false;
                    }
                }
            }
            return true;
        }

        public async Task ReplaceBuild(ServerInfo server)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (!Directory.Exists(server.ServerPath.Value))
                        Directory.CreateDirectory(server.ServerPath.Value);
                    else if (File.Exists($@"{Program.ServiceDirectory}\Server\MCSFiles\stock_filelist.ini"))
                        DeleteFilelist(File.ReadAllLines($@"{Program.ServiceDirectory}\Server\MCSFiles\stock_filelist.ini"), server.ServerPath.Value);
                    else
                        DeleteFilesRecursively(new DirectoryInfo(server.ServerPath.Value));

                    while (InstanceProvider.HostInfo.ServerVersion == null || InstanceProvider.HostInfo.ServerVersion == "None")
                    {
                        Thread.Sleep(150);
                    }

                    ZipFile.ExtractToDirectory($@"{Program.ServiceDirectory}\Server\MCSFiles\Update_{InstanceProvider.HostInfo.ServerVersion}.zip", server.ServerPath.Value);
                    File.Copy(server.ServerPath.Value + "\\bedrock_server.exe", server.ServerPath.Value + "\\" + server.ServerExeName.Value, true);
                    File.Delete(server.ServerPath.Value + "\\bedrock_server.exe");
                }
                catch (Exception e)
                {
                    InstanceProvider.ServiceLogger.AppendLine($"ERROR: Got an exception deleting entire directory! {e.Message}");
                }


            });
        }

        private void DeleteFilesRecursively(DirectoryInfo source)
        {
            try
            {
                source.Delete(true);
            }
            catch (Exception e)
            {
                InstanceProvider.ServiceLogger.AppendLine($@"Error Deleting Dir {source.Name}: {e.StackTrace}");
            }
        }

        private void DeleteFilelist(string[] fileList, string serverPath)
        {
            foreach (string file in fileList)
                try
                {
                    File.Delete($@"{serverPath}\{file}");
                }
                catch { }
            List<string> exesInPath = Directory.EnumerateFiles(serverPath, "*.exe", SearchOption.AllDirectories).ToList();
            foreach (string exe in exesInPath)
                File.Delete(exe);
            foreach (string dir in Directory.GetDirectories(serverPath))
                if (Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Count() == 0)
                    Directory.Delete(dir, true);
        }
    }
}
