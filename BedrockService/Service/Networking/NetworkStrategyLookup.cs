using BedrockService.Service.Core.Interfaces;
using BedrockService.Service.Networking.MessageInterfaces;
using BedrockService.Service.Server;
using BedrockService.Shared.MinecraftJsonModels.FileModels;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using BedrockService.Shared.PackParser;
using BedrockService.Shared.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace BedrockService.Service.Networking {
    public class NetworkStrategyLookup {
        private readonly Dictionary<NetworkMessageTypes, IMessageParser> _standardMessageLookup;
        private readonly Dictionary<NetworkMessageTypes, IFlaggedMessageParser> _flaggedMessageLookup;

        public NetworkStrategyLookup(ITCPListener messageSender, IBedrockService service, IBedrockLogger logger, IConfigurator configurator, IServiceConfiguration serviceConfiguration, IProcessInfo processInfo, IUpdater updater) {
            _standardMessageLookup = new Dictionary<NetworkMessageTypes, IMessageParser>()
            {
                {NetworkMessageTypes.DelBackups, new DeleteBackups(configurator) },
                {NetworkMessageTypes.BackupAll, new ServerBackupAll(messageSender, service) },
                {NetworkMessageTypes.EnumBackups, new EnumBackups(configurator, messageSender) },
                {NetworkMessageTypes.Backup, new ServerBackup(messageSender, service) },
                {NetworkMessageTypes.PropUpdate, new ServerPropUpdate(configurator, serviceConfiguration, messageSender, service) },
                {NetworkMessageTypes.Restart, new ServerRestart(messageSender, service) },
                {NetworkMessageTypes.Command, new ServerCommand(messageSender, service, logger) },
                {NetworkMessageTypes.PackList, new PackList(messageSender, processInfo, serviceConfiguration, logger) },
                {NetworkMessageTypes.RemovePack, new RemovePack(messageSender, processInfo, serviceConfiguration, logger) },
                {NetworkMessageTypes.PackFile, new PackFile(messageSender, serviceConfiguration, processInfo, logger) },
                {NetworkMessageTypes.Connect, new Connect(messageSender, serviceConfiguration) },
                {NetworkMessageTypes.StartCmdUpdate, new StartCmdUpdate(configurator, serviceConfiguration) },
                {NetworkMessageTypes.CheckUpdates, new CheckUpdates(messageSender, updater) },
                {NetworkMessageTypes.BackupRollback, new BackupRollback(messageSender, service) },
                {NetworkMessageTypes.AddNewServer, new AddNewServer(processInfo, configurator, messageSender, serviceConfiguration, service) },
                {NetworkMessageTypes.ConsoleLogUpdate, new ConsoleLogUpdate(messageSender, logger, serviceConfiguration, service) },
                {NetworkMessageTypes.PlayersRequest, new PlayerRequest(messageSender, serviceConfiguration) },
                {NetworkMessageTypes.PlayersUpdate, new PlayersUpdate(configurator, messageSender, serviceConfiguration, service) },
                {NetworkMessageTypes.LevelEditRequest, new LevelEditRequest(messageSender, serviceConfiguration) },
                {NetworkMessageTypes.LevelEditFile, new LevelEditFile(serviceConfiguration, service) }
            };
            _flaggedMessageLookup = new Dictionary<NetworkMessageTypes, IFlaggedMessageParser>()
            {
                {NetworkMessageTypes.RemoveServer, new RemoveServer(configurator, messageSender, serviceConfiguration, service) }
            };
                messageSender.SetStrategyDictionaries(_standardMessageLookup, _flaggedMessageLookup);
        }

        class DeleteBackups : IMessageParser {
            private readonly IConfigurator _configurator;

            public DeleteBackups(IConfigurator configurator) {
                _configurator = configurator;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                List<string> backupFileNames = JsonConvert.DeserializeObject<List<string>>(stringData, settings);
                _configurator.DeleteBackupsForServer(serverIndex, backupFileNames);
            }
        }

        class ServerBackupAll : IMessageParser {
            private readonly IMessageSender _messageSender;
            private readonly IBedrockService _service;

            public ServerBackupAll(IMessageSender messageSender, IBedrockService service) {
                _messageSender = messageSender;
                _service = service;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                foreach (IBedrockServer server in _service.GetAllServers())
                    server.InitializeBackup();
                _messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class ServerBackup : IMessageParser {
            private readonly IMessageSender _messageSender;
            private readonly IBedrockService _service;
            public ServerBackup(IMessageSender messageSender, IBedrockService service) {
                _messageSender = messageSender;
                _service = service;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                _service.GetBedrockServerByIndex(serverIndex).InitializeBackup();
                _messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class EnumBackups : IMessageParser {
            private readonly IConfigurator _configurator;
            private readonly IMessageSender _messageSender;
            public EnumBackups(IConfigurator configurator, IMessageSender messageSender) {
                _configurator = configurator;
                _messageSender = messageSender;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                Formatting indented = Formatting.Indented;
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_configurator.EnumerateBackupsForServer(serverIndex), indented, settings));
                _messageSender.SendData(serializeToBytes, NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.EnumBackups);

            }
        }

        class ServerPropUpdate : IMessageParser {
            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IMessageSender _messageSender;
            private readonly IBedrockService _bedrockService;
            private readonly IConfigurator _configurator;

            public ServerPropUpdate(IConfigurator configurator, IServiceConfiguration serviceConfiguration, IMessageSender messageSender, IBedrockService bedrockService) {
                _configurator = configurator;
                _serviceConfiguration = serviceConfiguration;
                _messageSender = messageSender;
                _bedrockService = bedrockService;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                List<Property> propList = JsonConvert.DeserializeObject<List<Property>>(stringData, settings);
                Property prop = propList.FirstOrDefault(p => p.KeyName == "server-name");
                if (prop == null) {
                    _serviceConfiguration.SetAllProps(propList);
                    _configurator.SaveGlobalFile();
                    _bedrockService.RestartService();
                    return;
                }
                _serviceConfiguration.GetServerInfoByIndex(serverIndex).SetAllProps(propList);
                _configurator.SaveServerProps(_serviceConfiguration.GetServerInfoByIndex(serverIndex), true);
                _bedrockService.GetBedrockServerByIndex(serverIndex).SetServerStatus(BedrockServer.ServerStatus.Stopping);
                while (_bedrockService.GetBedrockServerByIndex(serverIndex).GetServerStatus() == BedrockServer.ServerStatus.Stopping) {
                    Thread.Sleep(100);
                }
                _bedrockService.GetBedrockServerByIndex(serverIndex).SetServerStatus(BedrockServer.ServerStatus.Starting);
                _messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class ServerRestart : IMessageParser {
            private readonly IMessageSender _messageSender;
            private readonly IBedrockService _service;
            public ServerRestart(IMessageSender messageSender, IBedrockService service) {
                _messageSender = messageSender;
                _service = service;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                _service.GetBedrockServerByIndex(serverIndex).RestartServer(false);
                _messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class ServerCommand : IMessageParser {
            private readonly IBedrockService _service;
            private readonly IBedrockLogger _logger;
            private readonly IMessageSender _messageSender;

            public ServerCommand(IMessageSender messageSender, IBedrockService service, IBedrockLogger logger) {
                _messageSender = messageSender;
                _service = service;
                _logger = logger;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                _service.GetBedrockServerByIndex(serverIndex).WriteToStandardIn(stringData);
                _logger.AppendLine($"Sent command {stringData} to stdInput stream");
                _messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class PackList : IMessageParser {
            private readonly IMessageSender _messageSender;
            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IProcessInfo _processInfo;
            private readonly IBedrockLogger _logger;

            public PackList(IMessageSender messageSender, IProcessInfo processInfo, IServiceConfiguration serviceConfiguration, IBedrockLogger logger) {
                _logger = logger;
                _messageSender = messageSender;
                _serviceConfiguration = serviceConfiguration;
                _processInfo = processInfo;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                MinecraftKnownPacksClass knownPacks = new MinecraftKnownPacksClass($@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\valid_known_packs.json", $@"{_processInfo.GetDirectory()}\Server\stock_packs.json");
                List<MinecraftPackContainer> list = new List<MinecraftPackContainer>();
                foreach (KnownPacksJsonModel pack in knownPacks.InstalledPacks.Contents) {
                    MinecraftPackParser currentParser = new MinecraftPackParser(_logger, _processInfo);
                    currentParser.ParseDirectory($@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\{pack.path.Replace(@"/", @"\")}");
                    list.AddRange(currentParser.FoundPacks);
                }
                string arrayString = JsonConvert.SerializeObject(list);
                _messageSender.SendData(Encoding.UTF8.GetBytes(arrayString), NetworkMessageSource.Server, NetworkMessageDestination.Client, NetworkMessageTypes.PackList);
            }
        }

        class RemovePack : IMessageParser {
            private readonly IMessageSender _messageSender;
            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IProcessInfo _processInfo;
            private readonly IBedrockLogger _logger;

            public RemovePack(IMessageSender messageSender, IProcessInfo processInfo, IServiceConfiguration serviceConfiguration, IBedrockLogger logger) {
                _messageSender = messageSender;
                _serviceConfiguration = serviceConfiguration;
                _processInfo = processInfo;
                _logger = logger;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                MinecraftKnownPacksClass knownPacks = new MinecraftKnownPacksClass($@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\valid_known_packs.json", $@"{_processInfo.GetDirectory()}\Server\stock_packs.json");
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                List<MinecraftPackContainer> container = JsonConvert.DeserializeObject<List<MinecraftPackContainer>>(stringData, settings);
                foreach (MinecraftPackContainer content in container)
                    knownPacks.RemovePackFromServer(_serviceConfiguration.GetServerInfoByIndex(serverIndex), content);
                _messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class LevelEditRequest : IMessageParser {
            private readonly IMessageSender _messageSender;
            private readonly IServiceConfiguration _serviceConfiguration;

            public LevelEditRequest(IMessageSender messageSender, IServiceConfiguration serviceConfiguration) {
                _messageSender = messageSender;
                _serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
                string pathToLevelDat = $@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\worlds\{server.GetProp("level-name")}\level.dat";
                byte[] levelDatToBytes = File.ReadAllBytes(pathToLevelDat);
                _messageSender.SendData(levelDatToBytes, NetworkMessageSource.Server, NetworkMessageDestination.Client, NetworkMessageTypes.LevelEditFile);
            }
        }

        class PlayersUpdate : IMessageParser {
            private readonly IMessageSender _messageSender;
            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IConfigurator _configurator;
            private readonly IBedrockService _service;

            public PlayersUpdate(IConfigurator configurator, IMessageSender messageSender, IServiceConfiguration serviceConfiguration, IBedrockService service) {
                _service = service;
                _configurator = configurator;
                _messageSender = messageSender;
                _serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                List<IPlayer> fetchedPlayers = JsonConvert.DeserializeObject<List<IPlayer>>(stringData, settings);
                foreach (IPlayer player in fetchedPlayers) {
                    try {
                        _serviceConfiguration.GetServerInfoByIndex(serverIndex).AddUpdatePlayer(player);
                    }
                    catch (Exception) {
                    }
                }
                _configurator.SaveKnownPlayerDatabase(_serviceConfiguration.GetServerInfoByIndex(serverIndex));
                _configurator.WriteJSONFiles(_serviceConfiguration.GetServerInfoByIndex(serverIndex));
                Task.Delay(500).Wait();
                _service.GetBedrockServerByIndex(serverIndex).WriteToStandardIn("ops reload");
                _service.GetBedrockServerByIndex(serverIndex).WriteToStandardIn("whitelist reload");
                _messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class PackFile : IMessageParser {
            private readonly IMessageSender _messageSender;
            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IProcessInfo _serviceProcessInfo;
            private readonly IBedrockLogger _logger;

            public PackFile(IMessageSender messageSender, IServiceConfiguration serviceConfiguration, IProcessInfo serviceProcessInfo, IBedrockLogger logger) {
                _messageSender = messageSender;
                _logger = logger;
                _serviceProcessInfo = serviceProcessInfo;
                _serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                MinecraftPackParser archiveParser = new MinecraftPackParser(data, _logger, _serviceProcessInfo);
                foreach (MinecraftPackContainer container in archiveParser.FoundPacks) {
                    string serverPath = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath").ToString();
                    string levelName = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("level-name").ToString();
                    string filePath = "";
                    FileUtils fileUtils = new FileUtils(_serviceProcessInfo.GetDirectory());
                    if (container.ManifestType == "WorldPack") {
                        fileUtils.CopyFilesRecursively(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($@"{serverPath}\worlds\{container.FolderName}"));
                    }
                    if (container.ManifestType == "data") {
                        filePath = $@"{serverPath}\worlds\{levelName}\world_behavior_packs.json";
                        if(VerifyAddToJson(filePath, container.JsonManifest)) {
                            fileUtils.CopyFilesRecursively(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($@"{serverPath}\behavior_packs\{container.FolderName}"));
                        }
                    }
                    if (container.ManifestType == "resources") {
                        filePath = $@"{serverPath}\worlds\{levelName}\world_resource_packs.json";
                        if (VerifyAddToJson(filePath, container.JsonManifest)) {
                            fileUtils.CopyFilesRecursively(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($@"{serverPath}\resource_packs\{container.FolderName}"));
                        }
                    }
                }
            }

            private bool VerifyAddToJson(string filePath, PackManifestJsonModel manifest) {
                WorldPackFileModel worldPackFile = new WorldPackFileModel(filePath);
                if (worldPackFile.Contents.Where(x => x.pack_id == manifest.header.uuid).Count() > 0) {
                    return false;
                }
                worldPackFile.Contents.Add(new WorldKnownPackEntryJsonModel(manifest.header.uuid, manifest.header.version));
                worldPackFile.SaveFile();
                _messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
                return true;
            }
        }

        class LevelEditFile : IMessageParser {
            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IBedrockService _bedrockService;

            public LevelEditFile(IServiceConfiguration serviceConfiguration, IBedrockService bedrockService) {
                _bedrockService = bedrockService;
                _serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                byte[] stripHeaderFromBuffer = new byte[data.Length - 5];
                Buffer.BlockCopy(data, 5, stripHeaderFromBuffer, 0, stripHeaderFromBuffer.Length);
                IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
                string pathToLevelDat = $@"{_serviceConfiguration.GetProp("ServersPath")}\{server.GetProp("server-name")}\worlds\{server.GetProp("level-name")}\level.dat";
                _bedrockService.GetBedrockServerByIndex(serverIndex).StopServer(false).Wait();
                File.WriteAllBytes(pathToLevelDat, stripHeaderFromBuffer);
                _bedrockService.GetBedrockServerByIndex(serverIndex).SetServerStatus(BedrockServer.ServerStatus.Starting);
            }
        }

        class Connect : IMessageParser {
            private readonly IMessageSender _iTCPListener;
            private readonly IServiceConfiguration _serviceConfiguration;

            public Connect(ITCPListener iTCPListener, IServiceConfiguration serviceConfiguration) {
                _iTCPListener = iTCPListener;
                _serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                Formatting indented = Formatting.Indented;
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                string jsonString = JsonConvert.SerializeObject(_serviceConfiguration, indented, settings);
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(jsonString);
                _iTCPListener.SendData(serializeToBytes, NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Connect);
            }
        }

        class StartCmdUpdate : IMessageParser {
            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IConfigurator _configurator;

            public StartCmdUpdate(IConfigurator configurator, IServiceConfiguration serviceConfiguration) {
                _configurator = configurator;
                _serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                List<StartCmdEntry> entries = JsonConvert.DeserializeObject<List<StartCmdEntry>>(Encoding.UTF8.GetString(data, 5, data.Length - 5), settings);
                _serviceConfiguration.GetServerInfoByIndex(serverIndex).SetStartCommands(entries);
                _configurator.SaveServerProps(_serviceConfiguration.GetServerInfoByIndex(serverIndex), true);
            }
        }

        class CheckUpdates : IMessageParser {
            private readonly IMessageSender _messageSender;
            private readonly IUpdater _updater;

            public CheckUpdates(IMessageSender messageSender, IUpdater updater) {
                _updater = updater;
                _messageSender = messageSender;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                _updater.CheckUpdates().Wait();
                if (_updater.CheckVersionChanged()) {
                    _messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.CheckUpdates);
                }

                _messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class BackupRollback : IMessageParser {
            private readonly IMessageSender _messageSender;
            private readonly IBedrockService _service;

            public BackupRollback(IMessageSender messageSender, IBedrockService service) {
                _service = service;
                _messageSender = messageSender;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                _service.GetBedrockServerByIndex(serverIndex).RollbackToBackup(serverIndex, stringData);
                _messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);
            }
        }

        class AddNewServer : IMessageParser {
            private readonly IProcessInfo _processInfo;
            private readonly IMessageSender _messageSender;
            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IConfigurator _configurator;
            private readonly IBedrockService _bedrockService;

            public AddNewServer(IProcessInfo processInfo, IConfigurator configurator, IMessageSender messageSender, IServiceConfiguration serviceConfiguration, IBedrockService bedrockService) {
                _processInfo = processInfo;    
                _bedrockService = bedrockService;
                _configurator = configurator;
                _messageSender = messageSender;
                _serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                List<Property> propList = JsonConvert.DeserializeObject<List<Property>>(stringData, settings);
                Property serverNameProp = propList.First(p => p.KeyName == "server-name");
                ServerInfo newServer = new ServerInfo(_serviceConfiguration.GetProp("ServersPath").ToString(), _serviceConfiguration.GetServerDefaultPropList()) {
                    ServerName = serverNameProp.ToString(),
                    ServerPropList = propList,
                    ServerPath = new Property("ServerPath", "") {
                        Value = $@"{_serviceConfiguration.GetProp("ServersPath")}\{serverNameProp}"
                    },
                    ServerExeName = new Property("ServerExeName", "") {
                        Value = $"BedrockService.{serverNameProp}.exe"
                    },
                    FileName = $@"{serverNameProp}.conf"
                };
                _configurator.SaveServerProps(newServer, true);
                _bedrockService.InitializeNewServer(newServer);

                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_serviceConfiguration, Formatting.Indented, settings));
                _messageSender.SendData(serializeToBytes, NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Connect);
                _messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);

            }
        }

        class RemoveServer : IFlaggedMessageParser {
            private readonly IMessageSender _messageSender;
            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IConfigurator _configurator;
            private readonly IBedrockService _bedrockService;

            public RemoveServer(IConfigurator configurator, IMessageSender messageSender, IServiceConfiguration serviceConfiguration, IBedrockService bedrockService) {
                this._bedrockService = bedrockService;
                _configurator = configurator;
                _messageSender = messageSender;
                _serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex, NetworkMessageFlags flag) {
                _bedrockService.GetBedrockServerByIndex(serverIndex).StopServer(true).Wait();
                _configurator.RemoveServerConfigs(_serviceConfiguration.GetServerInfoByIndex(serverIndex), flag);
                _bedrockService.RemoveBedrockServerByIndex(serverIndex);
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_serviceConfiguration, Formatting.Indented, settings));
                _messageSender.SendData(serializeToBytes, NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.Connect);
                _messageSender.SendData(NetworkMessageSource.Service, NetworkMessageDestination.Client, NetworkMessageTypes.UICallback);

            }
        }

        class ConsoleLogUpdate : IMessageParser {
            private readonly IMessageSender _messageSender;
            private readonly IBedrockLogger _logger;
            private readonly IBedrockService _service;
            private readonly IServiceConfiguration _serviceConfiguration;

            public ConsoleLogUpdate(IMessageSender messageSender, IBedrockLogger logger, IServiceConfiguration serviceConfiguration, IBedrockService service) {
                _service = service;
                _logger = logger;
                _messageSender = messageSender;
                _serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                StringBuilder srvString = new StringBuilder();
                string[] split = stringData.Split('|');
                for (int i = 0; i < split.Length; i++) {
                    string[] dataSplit = split[i].Split(';');
                    string srvName = dataSplit[0];
                    int srvTextLen;
                    int clientCurLen;
                    int loop;
                    IBedrockLogger srvText;
                    if (srvName != "Service") {
                        try {
                            srvText = _service.GetBedrockServerByName(srvName).GetLogger();
                        }
                        catch (NullReferenceException) {
                            break;
                        }
                        srvTextLen = srvText.Count();
                        clientCurLen = int.Parse(dataSplit[1]);
                        loop = clientCurLen;
                        while (loop < srvTextLen) {
                            srvString.Append($"{srvName};{srvText.FromIndex(loop)};{loop}|");
                            loop++;
                        }

                    }
                    else {
                        srvTextLen = _serviceConfiguration.GetLog().Count;
                        clientCurLen = int.Parse(dataSplit[1]);
                        loop = clientCurLen;
                        while (loop < srvTextLen) {
                            srvString.Append($"{srvName};{_logger.FromIndex(loop)};{loop}|");
                            loop++;
                        }
                    }
                }
                if (srvString.Length > 1) {
                    srvString.Remove(srvString.Length - 1, 1);
                    _messageSender.SendData(Encoding.UTF8.GetBytes(srvString.ToString()), NetworkMessageSource.Server, NetworkMessageDestination.Client, NetworkMessageTypes.ConsoleLogUpdate);
                }
            }
        }

        class PlayerRequest : IMessageParser {
            private readonly IMessageSender _messageSender;
            private readonly IServiceConfiguration _serviceConfiguration;

            public PlayerRequest(IMessageSender messageSender, IServiceConfiguration serviceConfiguration) {
                _messageSender = messageSender;
                _serviceConfiguration = serviceConfiguration;
            }

            public void ParseMessage(byte[] data, byte serverIndex) {
                IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(server.GetPlayerList(), Formatting.Indented, settings));
                _messageSender.SendData(serializeToBytes, NetworkMessageSource.Server, NetworkMessageDestination.Client, serverIndex, NetworkMessageTypes.PlayersRequest);
            }
        }

    }
}
