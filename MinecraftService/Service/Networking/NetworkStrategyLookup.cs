
using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Service.Networking.NetworkStrategies;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;

namespace MinecraftService.Service.Networking
{
    public class NetworkStrategyLookup {
        private readonly Dictionary<MessageTypes, IMessageParser> _standardMessageLookup;
        private readonly Dictionary<MessageTypes, IFlaggedMessageParser> _flaggedMessageLookup;

        public NetworkStrategyLookup(ITCPObject listener, MmsService service, MmsLogger logger, UserConfigManager configurator, ServiceConfigurator serviceConfiguration, ProcessInfo processInfo, FileUtilities fileUtils) {
            _standardMessageLookup = new Dictionary<MessageTypes, IMessageParser>()
            {
                {MessageTypes.Connect, new Connect(serviceConfiguration) },
                {MessageTypes.AddNewServer, new AddNewServer(logger, processInfo, configurator, serviceConfiguration, service) },
                {MessageTypes.Command, new ServerCommand(service, logger) },
                {MessageTypes.Restart, new ServerRestart(service) },
                {MessageTypes.StartStop, new StartStopServer(service) },
                {MessageTypes.ServerStatusRequest, new ServerStatusRequest(service) },
                {MessageTypes.Backup, new ServerBackup(service) },
                {MessageTypes.BackupAll, new ServerBackupAll(service) },
                {MessageTypes.EnumBackups, new EnumBackups(configurator) },
                {MessageTypes.BackupRollback, new BackupRollback(service) },
                {MessageTypes.DelBackups, new DeleteBackups(logger, serviceConfiguration) },
                {MessageTypes.PropUpdate, new ServerPropUpdate(logger, configurator, serviceConfiguration, service) },
                {MessageTypes.StartCmdUpdate, new StartCmdUpdate(configurator, serviceConfiguration) },
                {MessageTypes.ConsoleLogUpdate, new ConsoleLogUpdate(logger, serviceConfiguration, service) },
                {MessageTypes.VersionListRequest, new VersionListRequest(logger, serviceConfiguration) },
                {MessageTypes.PackList, new PackList(processInfo, serviceConfiguration, logger) },
                {MessageTypes.PackFile, new PackFile(serviceConfiguration, logger) },
                {MessageTypes.RemovePack, new RemovePack(serviceConfiguration, logger) },
                {MessageTypes.CheckUpdates, new CheckUpdates(service) },
                {MessageTypes.PlayersRequest, new PlayerRequest(service) },
                {MessageTypes.PlayersUpdate, new PlayersUpdate(configurator, serviceConfiguration, service) },
                {MessageTypes.LevelEditRequest, new LevelEditRequest(serviceConfiguration) },
                {MessageTypes.LevelEditFile, new LevelEditFile(serviceConfiguration, service, logger) },
                {MessageTypes.ExportFile, new ExportFileRequest(configurator, service, serviceConfiguration, logger) },
            };
            listener.SetStrategies(_standardMessageLookup);
        }
    }
}
