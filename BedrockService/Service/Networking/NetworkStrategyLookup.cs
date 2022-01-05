using BedrockService.Service.Core.Interfaces;
using BedrockService.Service.Networking.MessageInterfaces;
using BedrockService.Service.Server;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using BedrockService.Shared.PackParser;
using BedrockService.Shared.Utilities;
using Newtonsoft.Json;
using System.Text;

namespace BedrockService.Service.Networking {
    public class NetworkStrategyLookup {
        private readonly Dictionary<NetworkMessageTypes, IMessageParser> _standardMessageLookup;
        private readonly Dictionary<NetworkMessageTypes, IFlaggedMessageParser> _flaggedMessageLookup;

        public NetworkStrategyLookup(ITCPListener listener, IBedrockService service, IBedrockLogger logger, IConfigurator configurator, IServiceConfiguration serviceConfiguration, IProcessInfo processInfo, IUpdater updater, FileUtilities fileUtils) {
            _standardMessageLookup = new Dictionary<NetworkMessageTypes, IMessageParser>()
            {
                {NetworkMessageTypes.DelBackups, new DeleteBackups(configurator) },
                {NetworkMessageTypes.BackupAll, new ServerBackupAll(service) },
                {NetworkMessageTypes.EnumBackups, new EnumBackups(configurator) },
                {NetworkMessageTypes.Backup, new ServerBackup(service) },
                {NetworkMessageTypes.PropUpdate, new ServerPropUpdate(configurator, serviceConfiguration, service) },
                {NetworkMessageTypes.Restart, new ServerRestart(service) },
                {NetworkMessageTypes.Command, new ServerCommand(service, logger) },
                {NetworkMessageTypes.PackList, new PackList(processInfo, serviceConfiguration, logger) },
                {NetworkMessageTypes.RemovePack, new RemovePack(processInfo, serviceConfiguration, logger) },
                {NetworkMessageTypes.PackFile, new PackFile(serviceConfiguration, processInfo, logger, fileUtils) },
                {NetworkMessageTypes.Connect, new Connect(serviceConfiguration) },
                {NetworkMessageTypes.StartCmdUpdate, new StartCmdUpdate(configurator, serviceConfiguration) },
                {NetworkMessageTypes.CheckUpdates, new CheckUpdates(updater) },
                {NetworkMessageTypes.BackupRollback, new BackupRollback(service) },
                {NetworkMessageTypes.AddNewServer, new AddNewServer(processInfo, configurator, serviceConfiguration, service) },
                {NetworkMessageTypes.ConsoleLogUpdate, new ConsoleLogUpdate(logger, serviceConfiguration, service) },
                {NetworkMessageTypes.PlayersRequest, new PlayerRequest(serviceConfiguration) },
                {NetworkMessageTypes.PlayersUpdate, new PlayersUpdate(configurator, serviceConfiguration, service) },
                {NetworkMessageTypes.LevelEditRequest, new LevelEditRequest(serviceConfiguration) },
                {NetworkMessageTypes.LevelEditFile, new LevelEditFile(serviceConfiguration, service, logger) }
            };
            _flaggedMessageLookup = new Dictionary<NetworkMessageTypes, IFlaggedMessageParser>()
            {
                {NetworkMessageTypes.RemoveServer, new RemoveServer(configurator, serviceConfiguration, service) }
            };
            listener.SetStrategyDictionaries(_standardMessageLookup, _flaggedMessageLookup);
        }

        class DeleteBackups : IMessageParser {
            private readonly IConfigurator _configurator;

            public DeleteBackups(IConfigurator configurator) {
                _configurator = configurator;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                List<string> backupFileNames = JsonConvert.DeserializeObject<List<string>>(stringData, settings);
                _configurator.DeleteBackupsForServer(serverIndex, backupFileNames);
                return (Array.Empty<byte>(), 0, 0);
            }
        }

        class ServerBackupAll : IMessageParser {

            private readonly IBedrockService _service;

            public ServerBackupAll(IBedrockService service) {

                _service = service;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                foreach (IBedrockServer server in _service.GetAllServers())
                    server.InitializeBackup();
                return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
            }
        }

        class ServerBackup : IMessageParser {

            private readonly IBedrockService _service;
            public ServerBackup(IBedrockService service) {

                _service = service;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                _service.GetBedrockServerByIndex(serverIndex).InitializeBackup();
                return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
            }
        }

        class EnumBackups : IMessageParser {
            private readonly IConfigurator _configurator;

            public EnumBackups(IConfigurator configurator) {
                _configurator = configurator;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                Formatting indented = Formatting.Indented;
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_configurator.EnumerateBackupsForServer(serverIndex), indented, settings));
                return (serializeToBytes, 0, NetworkMessageTypes.EnumBackups);

            }
        }

        class ServerPropUpdate : IMessageParser {
            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IBedrockService _bedrockService;
            private readonly IConfigurator _configurator;

            public ServerPropUpdate(IConfigurator configurator, IServiceConfiguration serviceConfiguration, IBedrockService bedrockService) {
                _configurator = configurator;
                _serviceConfiguration = serviceConfiguration;
                _bedrockService = bedrockService;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                List<Property> propList = JsonConvert.DeserializeObject<List<Property>>(stringData, settings);
                Property prop = propList.FirstOrDefault(p => p.KeyName == "server-name");
                if (prop == null) {
                    _serviceConfiguration.SetAllProps(propList);
                    _configurator.SaveGlobalFile();
                    _bedrockService.RestartService();
                    return (Array.Empty<byte>(), 0, 0);
                }
                _serviceConfiguration.GetServerInfoByIndex(serverIndex).SetAllProps(propList);
                _configurator.SaveServerConfiguration(_serviceConfiguration.GetServerInfoByIndex(serverIndex));
                _bedrockService.GetBedrockServerByIndex(serverIndex).RestartServer().Wait();
                return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
            }
        }

        class ServerRestart : IMessageParser {
            private readonly IBedrockService _service;

            public ServerRestart(IBedrockService service) {
                _service = service;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                _service.GetBedrockServerByIndex(serverIndex).RestartServer().Wait();
                return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
            }
        }

        class ServerCommand : IMessageParser {
            private readonly IBedrockService _service;
            private readonly IBedrockLogger _logger;


            public ServerCommand(IBedrockService service, IBedrockLogger logger) {
                _service = service;
                _logger = logger;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                _service.GetBedrockServerByIndex(serverIndex).WriteToStandardIn(stringData);
                _logger.AppendLine($"Sent command {stringData} to stdInput stream");
                return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
            }
        }

        class PackList : IMessageParser {

            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IProcessInfo _processInfo;
            private readonly IBedrockLogger _logger;

            public PackList(IProcessInfo processInfo, IServiceConfiguration serviceConfiguration, IBedrockLogger logger) {
                _logger = logger;
                _serviceConfiguration = serviceConfiguration;
                _processInfo = processInfo;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                MinecraftKnownPacksClass knownPacks = new MinecraftKnownPacksClass($@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\valid_known_packs.json", $@"{_processInfo.GetDirectory()}\Server\stock_packs.json");
                List<MinecraftPackContainer> list = new List<MinecraftPackContainer>();
                foreach (KnownPacksJsonModel pack in knownPacks.InstalledPacks.Contents) {
                    MinecraftPackParser currentParser = new MinecraftPackParser(_processInfo);
                    currentParser.ParseDirectory($@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\{pack.path.Replace(@"/", @"\")}");
                    list.AddRange(currentParser.FoundPacks);
                }
                string arrayString = JsonConvert.SerializeObject(list);
                return (Encoding.UTF8.GetBytes(arrayString), 0, NetworkMessageTypes.PackList);
            }
        }

        class RemovePack : IMessageParser {

            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IProcessInfo _processInfo;
            private readonly IBedrockLogger _logger;

            public RemovePack(IProcessInfo processInfo, IServiceConfiguration serviceConfiguration, IBedrockLogger logger) {
                _serviceConfiguration = serviceConfiguration;
                _processInfo = processInfo;
                _logger = logger;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                MinecraftKnownPacksClass knownPacks = new MinecraftKnownPacksClass($@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\valid_known_packs.json", $@"{_processInfo.GetDirectory()}\Server\stock_packs.json");
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                List<MinecraftPackContainer>? container = JsonConvert.DeserializeObject<List<MinecraftPackContainer>>(stringData, settings);
                foreach (MinecraftPackContainer content in container) {
                    knownPacks.RemovePackFromServer(_serviceConfiguration.GetServerInfoByIndex(serverIndex), content);
                    _logger.AppendLine($@"{content.JsonManifest.header.name} removed from server.");
                }
                return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
            }
        }

        class LevelEditRequest : IMessageParser {

            private readonly IServiceConfiguration _serviceConfiguration;

            public LevelEditRequest(IServiceConfiguration serviceConfiguration) {
                _serviceConfiguration = serviceConfiguration;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
                string pathToLevelDat = $@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\worlds\{server.GetProp("level-name")}\level.dat";
                byte[] levelDatToBytes = File.ReadAllBytes(pathToLevelDat);
                return (levelDatToBytes, 0, NetworkMessageTypes.LevelEditFile);
            }
        }

        class PlayersUpdate : IMessageParser {

            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IConfigurator _configurator;
            private readonly IBedrockService _service;

            public PlayersUpdate(IConfigurator configurator, IServiceConfiguration serviceConfiguration, IBedrockService service) {
                _service = service;
                _configurator = configurator;
                _serviceConfiguration = serviceConfiguration;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
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
                return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
            }
        }

        class PackFile : IMessageParser {

            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IProcessInfo _serviceProcessInfo;
            private readonly IBedrockLogger _logger;
            private readonly FileUtilities _fileUtils;

            public PackFile(IServiceConfiguration serviceConfiguration, IProcessInfo serviceProcessInfo, IBedrockLogger logger, FileUtilities fileUtils) {
                _fileUtils = fileUtils;
                _logger = logger;
                _serviceProcessInfo = serviceProcessInfo;
                _serviceConfiguration = serviceConfiguration;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                MinecraftPackParser archiveParser = new MinecraftPackParser(data, _serviceProcessInfo);
                foreach (MinecraftPackContainer container in archiveParser.FoundPacks) {
                    string serverPath = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath").ToString();
                    string levelName = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("level-name").ToString();
                    string knownPacksFile = $@"{serverPath}\valid_known_packs.json";
                    string filePath;
                    if (container.ManifestType == "WorldPack") {
                        _fileUtils.CopyFilesRecursively(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($@"{serverPath}\worlds\{container.FolderName}"));
                    }
                    if (container.ManifestType == "data") {
                        filePath = $@"{serverPath}\worlds\{levelName}\world_behavior_packs.json";
                        if(MinecraftFileUtilities.UpdateWorldPackFile(filePath, container.JsonManifest) && MinecraftFileUtilities.UpdateKnownPackFile(knownPacksFile, container)) {
                            _fileUtils.CopyFilesRecursively(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($@"{serverPath}\behavior_packs\{container.FolderName}"));
                        }
                    }
                    if (container.ManifestType == "resources") {
                        filePath = $@"{serverPath}\worlds\{levelName}\world_resource_packs.json";
                        if (MinecraftFileUtilities.UpdateWorldPackFile(filePath, container.JsonManifest) && MinecraftFileUtilities.UpdateKnownPackFile(knownPacksFile, container)) {
                            _fileUtils.CopyFilesRecursively(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($@"{serverPath}\resource_packs\{container.FolderName}"));
                        }
                    }
                    _logger.AppendLine($@"{container.GetFixedManifestType()} pack installed to server: {container.JsonManifest.header.name}.");
                }
                return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
            }
        }

        class LevelEditFile : IMessageParser {
            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IBedrockService _bedrockService;
            private readonly IBedrockLogger _logger;

            public LevelEditFile(IServiceConfiguration serviceConfiguration, IBedrockService bedrockService, IBedrockLogger logger) {
                _logger = logger;
                _bedrockService = bedrockService;
                _serviceConfiguration = serviceConfiguration;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                byte[] stripHeaderFromBuffer = new byte[data.Length - 5];
                Buffer.BlockCopy(data, 5, stripHeaderFromBuffer, 0, stripHeaderFromBuffer.Length);
                IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
                string pathToLevelDat = $@"{_serviceConfiguration.GetProp("ServersPath")}\{server.GetProp("server-name")}\worlds\{server.GetProp("level-name")}\level.dat";
                _bedrockService.GetBedrockServerByIndex(serverIndex).AwaitableServerStop(false).Wait();
                File.WriteAllBytes(pathToLevelDat, stripHeaderFromBuffer);
                _logger.AppendLine($"level.dat writen to server {server.GetServerName()}");
                _bedrockService.GetBedrockServerByIndex(serverIndex).AwaitableServerStart().Wait();
                return (Array.Empty<byte>(), 0, 0);
            }
        }

        class Connect : IMessageParser {
            private readonly IServiceConfiguration _serviceConfiguration;

            public Connect(IServiceConfiguration serviceConfiguration) {
                _serviceConfiguration = serviceConfiguration;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                Formatting indented = Formatting.Indented;
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                string jsonString = JsonConvert.SerializeObject(_serviceConfiguration, indented, settings);
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(jsonString);
                return (serializeToBytes, 0, NetworkMessageTypes.Connect);
            }
        }

        class StartCmdUpdate : IMessageParser {
            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IConfigurator _configurator;

            public StartCmdUpdate(IConfigurator configurator, IServiceConfiguration serviceConfiguration) {
                _configurator = configurator;
                _serviceConfiguration = serviceConfiguration;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                List<StartCmdEntry> entries = JsonConvert.DeserializeObject<List<StartCmdEntry>>(Encoding.UTF8.GetString(data, 5, data.Length - 5), settings);
                _serviceConfiguration.GetServerInfoByIndex(serverIndex).SetStartCommands(entries);
                _configurator.SaveServerConfiguration(_serviceConfiguration.GetServerInfoByIndex(serverIndex));
                return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
            }
        }

        class CheckUpdates : IMessageParser {

            private readonly IUpdater _updater;

            public CheckUpdates(IUpdater updater) {
                _updater = updater;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                _updater.CheckUpdates().Wait();
                if (_updater.CheckVersionChanged()) {
                    return (Array.Empty<byte>(), 0, NetworkMessageTypes.CheckUpdates);
                }
                return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
            }
        }

        class BackupRollback : IMessageParser {

            private readonly IBedrockService _service;

            public BackupRollback(IBedrockService service) {
                _service = service;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
                _service.GetBedrockServerByIndex(serverIndex).RollbackToBackup(serverIndex, stringData);
                return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
            }
        }

        class AddNewServer : IMessageParser {
            private readonly IProcessInfo _processInfo;

            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IConfigurator _configurator;
            private readonly IBedrockService _bedrockService;

            public AddNewServer(IProcessInfo processInfo, IConfigurator configurator, IServiceConfiguration serviceConfiguration, IBedrockService bedrockService) {
                _processInfo = processInfo;    
                _bedrockService = bedrockService;
                _configurator = configurator;

                _serviceConfiguration = serviceConfiguration;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
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
                _configurator.SaveServerConfiguration(newServer);
                _bedrockService.InitializeNewServer(newServer);

                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_serviceConfiguration, Formatting.Indented, settings));
                return (serializeToBytes, 0, NetworkMessageTypes.Connect);

            }
        }

        class RemoveServer : IFlaggedMessageParser {

            private readonly IServiceConfiguration _serviceConfiguration;
            private readonly IConfigurator _configurator;
            private readonly IBedrockService _bedrockService;

            public RemoveServer(IConfigurator configurator, IServiceConfiguration serviceConfiguration, IBedrockService bedrockService) {
                this._bedrockService = bedrockService;
                _configurator = configurator;

                _serviceConfiguration = serviceConfiguration;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex, NetworkMessageFlags flag) {
                _bedrockService.GetBedrockServerByIndex(serverIndex).AwaitableServerStop(true).Wait();
                _configurator.RemoveServerConfigs(_serviceConfiguration.GetServerInfoByIndex(serverIndex), flag);
                _bedrockService.RemoveBedrockServerByIndex(serverIndex);
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_serviceConfiguration, Formatting.Indented, settings));
                return (serializeToBytes, 0, NetworkMessageTypes.Connect);

            }
        }

        class ConsoleLogUpdate : IMessageParser {

            private readonly IBedrockLogger _logger;
            private readonly IBedrockService _service;
            private readonly IServiceConfiguration _serviceConfiguration;

            public ConsoleLogUpdate(IBedrockLogger logger, IServiceConfiguration serviceConfiguration, IBedrockService service) {
                _service = service;
                _logger = logger;

                _serviceConfiguration = serviceConfiguration;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
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
                        } catch (NullReferenceException) {
                            break;
                        }
                        srvTextLen = srvText.Count();
                        clientCurLen = int.Parse(dataSplit[1]);
                        loop = clientCurLen;
                        while (loop < srvTextLen) {
                            srvString.Append($"{srvName};{srvText.FromIndex(loop)};{loop}|");
                            loop++;
                        }

                    } else {
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
                    return (Encoding.UTF8.GetBytes(srvString.ToString()), 0, NetworkMessageTypes.ConsoleLogUpdate);
                }
                return (Array.Empty<byte>(), 0, 0);
            }
        }

        class PlayerRequest : IMessageParser {

            private readonly IServiceConfiguration _serviceConfiguration;

            public PlayerRequest(IServiceConfiguration serviceConfiguration) {

                _serviceConfiguration = serviceConfiguration;
            }

            public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
                IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(server.GetPlayerList(), Formatting.Indented, settings));
                return (serializeToBytes, serverIndex, NetworkMessageTypes.PlayersRequest);
            }
        }

        public IMessageParser GetStandardStrategy(NetworkMessageTypes messageType) => _standardMessageLookup[messageType];

        public IFlaggedMessageParser GetFlaggedStrategy(NetworkMessageTypes messageType) => _flaggedMessageLookup[messageType];
    }
}
