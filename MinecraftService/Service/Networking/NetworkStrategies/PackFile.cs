using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.PackParser;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class PackFile : IMessageParser {

        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IServerLogger _logger;

        public PackFile(IServiceConfiguration serviceConfiguration, IServerLogger logger) {
            _logger = logger;
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            Progress<ProgressModel> progress = new Progress<ProgressModel>((progress) => {
                string prog = string.Format("{0:N2}", progress.Progress);
                _logger.AppendLine($"Extracting pack contents... {prog}% completed.");
            });
            MinecraftPackParser archiveParser = new(_logger, progress);
            archiveParser.ProcessServerData(data, () => {
                foreach (MinecraftPackContainer container in archiveParser.FoundPacks) {
                    IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
                    string serverPath = server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString();
                    string levelName = server.GetProp(MmsDependServerPropKeys.LevelName).ToString();
                    string filePath;

                    if (container.ManifestType == MinecraftPackTypeStrings[MinecraftPackTypes.WorldPack]) {
                        FileUtilities.CopyFolderTree(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo(GetServerDirectory(ServerDirectoryKeys.ServerWorldDir_LevelName, serverPath, container.FolderName)));
                    }
                    if (container.ManifestType == MinecraftPackTypeStrings[MinecraftPackTypes.Behavior]) {
                        filePath = GetServerFilePath(ServerFileNameKeys.WorldBehaviorPacks, server, levelName);
                        if (MinecraftFileUtilities.UpdateWorldPackFile(filePath, container.JsonManifest)) {
                            FileUtilities.CopyFolderTree(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($"{GetServerDirectory(ServerDirectoryKeys.BehaviorPacksDir, serverPath, levelName)}\\{container.FolderName}"));
                        }
                    }
                    if (container.ManifestType == MinecraftPackTypeStrings[MinecraftPackTypes.Resource]) {
                        filePath = GetServerFilePath(ServerFileNameKeys.WorldResourcePacks, server, levelName);
                        if (MinecraftFileUtilities.UpdateWorldPackFile(filePath, container.JsonManifest)) {
                            FileUtilities.CopyFolderTree(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($"{GetServerDirectory(ServerDirectoryKeys.ResourcePacksDir, serverPath, levelName)}\\{container.FolderName}"));
                        }
                    }
                    _logger.AppendLine($@"{container.GetFixedManifestType()} pack installed to server: {container.JsonManifest.header.name}.");
                }
            });
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}

