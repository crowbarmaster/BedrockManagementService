using BedrockService.Service.Core;
using BedrockService.Service.Management;
using BedrockService.Service.Networking.NetworkMessageClasses;
using BedrockService.Service.Server;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using BedrockService.Shared.PackParser;
using BedrockService.Shared.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace BedrockService.Service.Networking
{
    public class NetworkStrategyLookup
    {
        public Dictionary<NetworkMessageTypes, IMessageParser> StandardMessageLookup;
        public Dictionary<NetworkMessageTypes, IFlaggedMessageParser> FlaggedMessageLookup;

        public NetworkStrategyLookup(ITCPListener messageSender, IBedrockService service, ILogger logger, IConfigurator configurator, IServiceConfiguration serviceConfiguration, IProcessInfo processInfo, IUpdater updater)
        {
            StandardMessageLookup = new Dictionary<NetworkMessageTypes, IMessageParser>()
            {
                {NetworkMessageTypes.DelBackups, new DeleteBackups(configurator) },
                {NetworkMessageTypes.BackupAll, new ServerBackupAll(messageSender, service) },
                {NetworkMessageTypes.EnumBackups, new EnumBackups(configurator, messageSender) },
                {NetworkMessageTypes.Backup, new ServerBackup(messageSender, service) },
                {NetworkMessageTypes.PropUpdate, new ServerPropUpdate(serviceConfiguration, messageSender, service) },
                {NetworkMessageTypes.Restart, new ServerRestart(messageSender, service) },
                {NetworkMessageTypes.Command, new ServerCommand(messageSender, service, logger) },
                {NetworkMessageTypes.PackList, new PackList(messageSender, processInfo, serviceConfiguration) },
                {NetworkMessageTypes.RemovePack, new RemovePack(messageSender, processInfo, serviceConfiguration) },
                {NetworkMessageTypes.PlayersUpdate, new PlayersUpdate(configurator, messageSender, serviceConfiguration) },
                {NetworkMessageTypes.PackFile, new PackFile(configurator, messageSender, serviceConfiguration, processInfo) },
                {NetworkMessageTypes.Connect, new Connect(messageSender, serviceConfiguration) },
                {NetworkMessageTypes.StartCmdUpdate, new StartCmdUpdate(configurator, messageSender, serviceConfiguration) },
                {NetworkMessageTypes.CheckUpdates, new CheckUpdates(configurator, messageSender, serviceConfiguration, updater) },
                {NetworkMessageTypes.BackupRollback, new BackupRollback(configurator, messageSender) },
                {NetworkMessageTypes.AddNewServer, new AddNewServer(configurator, messageSender, processInfo, serviceConfiguration, logger, service) },
                {NetworkMessageTypes.ConsoleLogUpdate, new ConsoleLogUpdate(messageSender, serviceConfiguration, service) },
                {NetworkMessageTypes.PlayersRequest, new PlayerRequest(messageSender, serviceConfiguration) }
            };
            FlaggedMessageLookup = new Dictionary<NetworkMessageTypes, IFlaggedMessageParser>()
            {
                {NetworkMessageTypes.RemoveServer, new RemoveServer(configurator, messageSender, serviceConfiguration, logger, service) },
            };
        }

        class DeleteBackups : IMessageParser
        {
            private IConfigurator configurator;

            public DeleteBackups(IConfigurator configurator)
            {
                this.configurator = configurator;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                List<string> backupFileNames = JsonConvert.DeserializeObject<List<string>>(stringData, settings);
                configurator.DeleteBackupsForServer(serverIndex, backupFileNames);
            }
        }

        class ServerBackupAll : IMessageParser
        {
            private IMessageSender messageSender;
            private IBedrockService service;

            public ServerBackupAll(IMessageSender messageSender, IBedrockService service)
            {
                this.messageSender = messageSender;
                this.service = service;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                service.GetBedrockServerByIndex(serverIndex).RestartServer(true);
                messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class ServerBackup : IMessageParser
        {
            private IMessageSender messageSender;
            private IBedrockService service;
            public ServerBackup(IMessageSender messageSender, IBedrockService service)
            {
                this.messageSender = messageSender;
                this.service = service;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                service.GetBedrockServerByIndex(serverIndex).RestartServer(true);
                messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class EnumBackups : IMessageParser
        {
            private readonly IConfigurator configurator;
            private readonly IMessageSender messageSender;
            public EnumBackups(IConfigurator configurator, IMessageSender messageSender)
            {
                this.configurator = configurator;
                this.messageSender = messageSender;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                Formatting indented = Formatting.Indented;
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(configurator.EnumerateBackupsForServer(serverIndex), indented, settings));
                messageSender.SendData(serializeToBytes, NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.EnumBackups);

            }
        }

        class ServerPropUpdate : IMessageParser
        {
            IServiceConfiguration serviceConfiguration;
            IMessageSender messageSender;
            IBedrockService bedrockService;

            public ServerPropUpdate(IServiceConfiguration serviceConfiguration, IMessageSender messageSender, IBedrockService bedrockService)
            {
                this.serviceConfiguration = serviceConfiguration;
                this.messageSender = messageSender;
                this.bedrockService = bedrockService;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                List<Property> propList = JsonConvert.DeserializeObject<List<Property>>(stringData, settings);
                Property prop = propList.First(p => p.KeyName == "server-name");
                serviceConfiguration.GetServerInfoByIndex(serverIndex).SetAllProps(propList);
                bedrockService.GetBedrockServerByIndex(serverIndex).SetServerStatus(BedrockServer.ServerStatus.Stopping);
                while (bedrockService.GetBedrockServerByIndex(serverIndex).GetServerStatus() == BedrockServer.ServerStatus.Stopping)
                {
                    Thread.Sleep(100);
                }
                bedrockService.GetBedrockServerByIndex(serverIndex).SetServerStatus(BedrockServer.ServerStatus.Starting);
                messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class ServerRestart : IMessageParser
        {
            private IMessageSender messageSender;
            private IBedrockService service;
            public ServerRestart(IMessageSender messageSender, IBedrockService service)
            {
                this.messageSender = messageSender;
                this.service = service;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                service.GetBedrockServerByIndex(serverIndex).RestartServer(false);
                messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class ServerCommand : IMessageParser
        {
            IBedrockService service;
            ILogger logger;
            IMessageSender messageSender;

            public ServerCommand(IMessageSender messageSender, IBedrockService service, ILogger logger)
            {
                this.messageSender = messageSender;
                this.service = service;
                this.logger = logger;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                service.GetBedrockServerByIndex(serverIndex).WriteToStandardIn(stringData);
                logger.AppendLine($"Sent command {stringData} to stdInput stream");
                messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class PackList : IMessageParser
        {
            IMessageSender messageSender;
            IServiceConfiguration serviceConfiguration;
            IProcessInfo processInfo;

            public PackList(IMessageSender messageSender, IProcessInfo processInfo, IServiceConfiguration serviceConfiguration)
            {
                this.messageSender = messageSender;
                this.serviceConfiguration = serviceConfiguration;
                this.processInfo = processInfo;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                if (!File.Exists($@"{processInfo.GetDirectory()}\Server\stock_packs.json"))
                    File.Copy($@"{serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\valid_known_packs.json", $@"{processInfo.GetDirectory()}\Server\stock_packs.json");
                MinecraftKnownPacksClass knownPacks = new MinecraftKnownPacksClass($@"{serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\valid_known_packs.json", $@"{processInfo.GetDirectory()}\Server\stock_packs.json");
                List<MinecraftPackParser> list = new List<MinecraftPackParser>();
                foreach (MinecraftKnownPacksClass.KnownPack pack in knownPacks.KnownPacks)
                    list.Add(new MinecraftPackParser($@"{serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\{pack.path.Replace(@"/", @"\")}"));
                messageSender.SendData(Encoding.UTF8.GetBytes(JArray.FromObject(list).ToString()), NetworkMessageSource.Server, NetworkMessageDestination.Client, NetworkMessageTypes.PackList);
            }
        }

        class RemovePack : IMessageParser
        {
            IMessageSender messageSender;
            IServiceConfiguration serviceConfiguration;
            IProcessInfo processInfo;

            public RemovePack(IMessageSender messageSender, IProcessInfo processInfo, IServiceConfiguration serviceConfiguration)
            {
                this.messageSender = messageSender;
                this.serviceConfiguration = serviceConfiguration;
                this.processInfo = processInfo;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                MinecraftKnownPacksClass knownPacks = new MinecraftKnownPacksClass($@"{serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\valid_known_packs.json", $@"{processInfo.GetDirectory()}\Server\stock_packs.json");
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                List<MinecraftPackContainer> MCContainer = JsonConvert.DeserializeObject<List<MinecraftPackContainer>>(stringData, settings);
                foreach (MinecraftPackContainer cont in MCContainer)
                    knownPacks.RemovePackFromServer(serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath").ToString(), cont);
                messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class LevelEditRequest : IMessageParser
        {
            IMessageSender messageSender;
            IServiceConfiguration serviceConfiguration;

            public LevelEditRequest(IMessageSender messageSender, IServiceConfiguration serviceConfiguration)
            {
                this.messageSender = messageSender;
                this.serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                IServerConfiguration server = serviceConfiguration.GetServerInfoByIndex(serverIndex);
                string pathToLevelDat = $@"{serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\{server.GetProp("server-name")}\worlds\{server.GetProp("level-name")}\level.dat";
                byte[] levelDatToBytes = File.ReadAllBytes(pathToLevelDat);
                messageSender.SendData(levelDatToBytes, NetworkMessageSource.Server, NetworkMessageDestination.Client, NetworkMessageTypes.LevelEditFile);
            }
        }

        class PlayersUpdate : IMessageParser
        {
            IMessageSender messageSender;
            IServiceConfiguration serviceConfiguration;
            IConfigurator configurator;

            public PlayersUpdate(IConfigurator configurator, IMessageSender messageSender, IServiceConfiguration serviceConfiguration)
            {
                this.configurator = configurator;
                this.messageSender = messageSender;
                this.serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                List<Player> fetchedPlayers = JsonConvert.DeserializeObject<List<Player>>(stringData, settings);
                foreach (Player player in fetchedPlayers)
                {
                    try
                    {
                        serviceConfiguration.GetServerInfoByIndex(serverIndex).AddUpdatePlayer(player);
                    }
                    catch (Exception)
                    {
                    }
                }
                configurator.SaveKnownPlayerDatabase(serviceConfiguration.GetServerInfoByIndex(serverIndex));
                configurator.LoadAllConfigurations().Wait();
                messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);

            }
        }

        class PackFile : IMessageParser
        {
            IMessageSender messageSender;
            IServiceConfiguration serviceConfiguration;
            IConfigurator configurator;
            IProcessInfo serviceProcessInfo;

            public PackFile(IConfigurator configurator, IMessageSender messageSender, IServiceConfiguration serviceConfiguration, IProcessInfo serviceProcessInfo)
            {
                this.serviceProcessInfo = serviceProcessInfo;
                this.configurator = configurator;
                this.messageSender = messageSender;
                this.serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                MinecraftPackParser archiveParser = new MinecraftPackParser(Encoding.UTF8.GetString(data, 5, data.Length - 5));
                foreach (MinecraftPackContainer container in archiveParser.FoundPacks)
                {
                    string serverPath = serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath").ToString();
                    if (container.ManifestType == "WorldPack")
                        new FileUtils(serviceProcessInfo.GetDirectory()).CopyFilesRecursively(container.PackContentLocation, new DirectoryInfo($@"{serverPath}\worlds\{container.FolderName}"));
                    if (container.ManifestType == "data")
                        new FileUtils(serviceProcessInfo.GetDirectory()).CopyFilesRecursively(container.PackContentLocation, new DirectoryInfo($@"{serverPath}\behavior_packs\{container.FolderName}"));
                    if (container.ManifestType == "resources")
                        new FileUtils(serviceProcessInfo.GetDirectory()).CopyFilesRecursively(container.PackContentLocation, new DirectoryInfo($@"{serverPath}\resource_packs\{container.FolderName}"));
                }
            }
        }

        class LevelEditFile : IMessageParser
        {
            IMessageSender messageSender;
            IServiceConfiguration serviceConfiguration;
            IConfigurator configurator;
            IBedrockService bedrockService;

            public LevelEditFile(IConfigurator configurator, IMessageSender messageSender, IServiceConfiguration serviceConfiguration, IBedrockService bedrockService)
            {
                this.bedrockService = bedrockService;
                this.configurator = configurator;
                this.messageSender = messageSender;
                this.serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                byte[] stripHeaderFromBuffer = new byte[data.Length - 5];
                Buffer.BlockCopy(data, 5, stripHeaderFromBuffer, 0, stripHeaderFromBuffer.Length);
                IServerConfiguration server = serviceConfiguration.GetServerInfoByIndex(serverIndex);
                string pathToLevelDat = $@"{serviceConfiguration.GetProp("ServersPath")}\{server.GetProp("server-name")}\worlds\{server.GetProp("level-name")}\level.dat";
                bedrockService.GetBedrockServerByIndex(serverIndex).StopServer().Wait();
                File.WriteAllBytes(pathToLevelDat, stripHeaderFromBuffer);
                bedrockService.GetBedrockServerByIndex(serverIndex).SetServerStatus(BedrockServer.ServerStatus.Starting);
            }
        }

        class Connect : IMessageParser
        {
            IMessageSender messageSender;
            IServiceConfiguration serviceConfiguration;

            public Connect(IMessageSender messageSender, IServiceConfiguration serviceConfiguration)
            {
                this.messageSender = messageSender;
                this.serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                Formatting indented = Formatting.Indented;
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                string jsonString = JsonConvert.SerializeObject(serviceConfiguration, indented, settings);
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(jsonString);
                messageSender.SendData(serializeToBytes, NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Connect);
            }
        }

        class StartCmdUpdate : IMessageParser
        {
            IMessageSender messageSender;
            IServiceConfiguration serviceConfiguration;
            IConfigurator configurator;

            public StartCmdUpdate(IConfigurator configurator, IMessageSender messageSender, IServiceConfiguration serviceConfiguration)
            {
                this.configurator = configurator;
                this.messageSender = messageSender;
                this.serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                serviceConfiguration.GetServerInfoByIndex(serverIndex).SetStartCommands(JsonConvert.DeserializeObject<List<StartCmdEntry>>(Encoding.UTF8.GetString(data, 5, data.Length - 5), settings));
                configurator.SaveServerProps(serviceConfiguration.GetServerInfoByIndex(serverIndex), true);
            }
        }

        class CheckUpdates : IMessageParser
        {
            IMessageSender messageSender;
            IServiceConfiguration serviceConfiguration;
            IConfigurator configurator;
            IUpdater updater;

            public CheckUpdates(IConfigurator configurator, IMessageSender messageSender, IServiceConfiguration serviceConfiguration, IUpdater updater)
            {
                this.updater = updater;
                this.configurator = configurator;
                this.messageSender = messageSender;
                this.serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                updater.CheckUpdates().Wait();
                if (updater.CheckVersionChanged())
                {
                    messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.CheckUpdates);
                }

                messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class BackupRollback : IMessageParser
        {
            IMessageSender messageSender;
            IConfigurator configurator;

            public BackupRollback(IConfigurator configurator, IMessageSender messageSender)
            {
                this.configurator = configurator;
                this.messageSender = messageSender;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                configurator.RollbackToBackup(serverIndex, stringData);
                messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class AddNewServer : IMessageParser
        {
            IMessageSender messageSender;
            IServiceConfiguration serviceConfiguration;
            IConfigurator configurator;
            ILogger logger;
            IBedrockService bedrockService;
            IProcessInfo processInfo;

            public AddNewServer(IConfigurator configurator, IMessageSender messageSender, IProcessInfo processInfo, IServiceConfiguration serviceConfiguration, ILogger logger, IBedrockService bedrockService)
            {
                this.processInfo = processInfo;
                this.bedrockService = bedrockService;
                this.logger = logger;
                this.configurator = configurator;
                this.messageSender = messageSender;
                this.serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                List<Property> propList = JsonConvert.DeserializeObject<List<Property>>(stringData, settings);
                Property serverNameProp = propList.First(p => p.KeyName == "server-name");
                ServerInfo newServer = new ServerInfo(null, serviceConfiguration.GetProp("ServersPath").ToString())
                {
                    ServerName = serverNameProp.ToString(),
                    ServerPropList = propList,
                    ServerPath = new Property("ServerPath", "")
                    {
                        Value = $@"{serviceConfiguration.GetProp("ServersPath")}\{serverNameProp}"
                    },
                    ServerExeName = new Property("ServerExeName", "")
                    {
                        Value = $"BDS_{serverNameProp}.exe"
                    },
                    FileName = $@"{serverNameProp}.conf"
                };
                configurator.SaveServerProps(newServer, true);
                bedrockService.InitializeNewServer(newServer);

                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(serviceConfiguration, Formatting.Indented, settings));
                messageSender.SendData(serializeToBytes, NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Connect);
                messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);

            }
        }

        class RemoveServer : IFlaggedMessageParser
        {
            IMessageSender messageSender;
            IServiceConfiguration serviceConfiguration;
            IConfigurator configurator;
            ILogger logger;
            IBedrockService bedrockService;

            public RemoveServer(IConfigurator configurator, IMessageSender messageSender, IServiceConfiguration serviceConfiguration, ILogger logger, IBedrockService bedrockService)
            {
                this.bedrockService = bedrockService;
                this.logger = logger;
                this.configurator = configurator;
                this.messageSender = messageSender;
                this.serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex, NetworkMessageFlags flag)
            {
                bedrockService.GetBedrockServerByIndex(serverIndex).StopServer().Wait();
                configurator.RemoveServerConfigs(serviceConfiguration.GetServerInfoByIndex(serverIndex), flag);
                bedrockService.RemoveBedrockServerByIndex(serverIndex);
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(serviceConfiguration, Formatting.Indented, settings));
                messageSender.SendData(serializeToBytes, NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Connect);
                messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);

            }
        }

        class ConsoleLogUpdate : IMessageParser
        {
            readonly IMessageSender messageSender;
            readonly IBedrockService service;
            readonly IServiceConfiguration serviceConfiguration;

            public ConsoleLogUpdate(IMessageSender messageSender, IServiceConfiguration serviceConfiguration, IBedrockService service)
            {
                this.service = service;
                this.messageSender = messageSender;
                this.serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                StringBuilder srvString = new StringBuilder();
                string[] split = stringData.Split('|');
                for (int i = 0; i < split.Length; i++)
                {
                    string[] dataSplit = split[i].Split(';');
                    string srvName = dataSplit[0];
                    int srvTextLen;
                    int clientCurLen;
                    int loop;
                    if (srvName != "Service")
                    {
                        ILogger srvText = service.GetBedrockServerByName(srvName).GetLogger();
                        srvTextLen = srvText.Count();
                        clientCurLen = int.Parse(dataSplit[1]);
                        loop = clientCurLen;
                        while (loop < srvTextLen)
                        {
                            srvString.Append($"{srvName};{srvText.FromIndex(loop)};{loop}|");
                            loop++;
                        }

                    }
                    else
                    {
                        srvTextLen = serviceConfiguration.GetLog().Count;
                        clientCurLen = int.Parse(dataSplit[1]);
                        loop = clientCurLen;
                        while (loop < srvTextLen)
                        {
                            srvString.Append($"{srvName};{serviceConfiguration.GetLog()[loop]};{loop}|");
                            loop++;
                        }
                    }
                }
                if (srvString.Length > 1)
                {
                    srvString.Remove(srvString.Length - 1, 1);
                    messageSender.SendData(Encoding.UTF8.GetBytes(srvString.ToString()), NetworkMessageSource.Server, NetworkMessageDestination.Client, NetworkMessageTypes.ConsoleLogUpdate);
                }
            }
        }

        class PlayerRequest : IMessageParser
        {
            IMessageSender messageSender;
            IServiceConfiguration serviceConfiguration;

            public PlayerRequest(IMessageSender messageSender, IServiceConfiguration serviceConfiguration)
            {
                this.messageSender = messageSender;
                this.serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex)
            {
                IServerConfiguration server = serviceConfiguration.GetServerInfoByIndex(serverIndex);
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(server.GetPlayerList(), Formatting.Indented, settings));
                messageSender.SendData(serializeToBytes, NetworkMessageSource.Server, NetworkMessageDestination.Client, serverIndex, NetworkMessageTypes.PlayersRequest);
            }
        }

    }
}
