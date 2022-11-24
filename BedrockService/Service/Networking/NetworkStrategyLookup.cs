
using BedrockService.Service.Networking.Interfaces;
using BedrockService.Service.Networking.NetworkStrategies;

namespace BedrockService.Service.Networking {
    public class NetworkStrategyLookup {
        private readonly Dictionary<NetworkMessageTypes, IMessageParser> _standardMessageLookup;
        private readonly Dictionary<NetworkMessageTypes, IFlaggedMessageParser> _flaggedMessageLookup;

        public NetworkStrategyLookup(ITCPListener listener, IBedrockService service, IBedrockLogger logger, IConfigurator configurator, IServiceConfiguration serviceConfiguration, IProcessInfo processInfo, FileUtilities fileUtils) {
            _standardMessageLookup = new Dictionary<NetworkMessageTypes, IMessageParser>()
            {
                {NetworkMessageTypes.Connect, new Connect(serviceConfiguration) },
                {NetworkMessageTypes.AddNewServer, new AddNewServer(logger, processInfo, configurator, serviceConfiguration, service) },
                {NetworkMessageTypes.Command, new ServerCommand(service, logger) },
                {NetworkMessageTypes.Restart, new ServerRestart(service) },
                {NetworkMessageTypes.StartStop, new StartStopServer(service) },
                {NetworkMessageTypes.ServerStatusRequest, new StatusRequest(service, serviceConfiguration) },
                {NetworkMessageTypes.Backup, new ServerBackup(service) },
                {NetworkMessageTypes.BackupAll, new ServerBackupAll(service) },
                {NetworkMessageTypes.EnumBackups, new EnumBackups(configurator) },
                {NetworkMessageTypes.BackupRollback, new BackupRollback(service) },
                {NetworkMessageTypes.DelBackups, new DeleteBackups(configurator) },
                {NetworkMessageTypes.PropUpdate, new ServerPropUpdate(logger, configurator, serviceConfiguration, service) },
                {NetworkMessageTypes.StartCmdUpdate, new StartCmdUpdate(configurator, serviceConfiguration) },
                {NetworkMessageTypes.ConsoleLogUpdate, new ConsoleLogUpdate(logger, serviceConfiguration, service) },
                {NetworkMessageTypes.PackList, new PackList(processInfo, serviceConfiguration, logger) },
                {NetworkMessageTypes.PackFile, new PackFile(serviceConfiguration, logger, fileUtils) },
                {NetworkMessageTypes.RemovePack, new RemovePack(serviceConfiguration, logger) },
                {NetworkMessageTypes.CheckUpdates, new CheckUpdates(service) },
                {NetworkMessageTypes.PlayersRequest, new PlayerRequest(service) },
                {NetworkMessageTypes.PlayersUpdate, new PlayersUpdate(configurator, serviceConfiguration, service) },
                {NetworkMessageTypes.LevelEditRequest, new LevelEditRequest(serviceConfiguration) },
                {NetworkMessageTypes.LevelEditFile, new LevelEditFile(serviceConfiguration, service, logger) },
                {NetworkMessageTypes.ExportFile, new ExportFileRequest(configurator, serviceConfiguration) },
                {NetworkMessageTypes.LLPluginFile, new LiteLoaderPluginFile(serviceConfiguration) }
            };
            _flaggedMessageLookup = new Dictionary<NetworkMessageTypes, IFlaggedMessageParser>()
            {
                {NetworkMessageTypes.RemoveServer, new RemoveServer(configurator, serviceConfiguration, service) }
            };
            listener.SetStrategyDictionaries(_standardMessageLookup, _flaggedMessageLookup);
        }

        public IMessageParser GetStandardStrategy(NetworkMessageTypes messageType) => _standardMessageLookup[messageType];

        public IFlaggedMessageParser GetFlaggedStrategy(NetworkMessageTypes messageType) => _flaggedMessageLookup[messageType];
    }
}
