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
            MinecraftPackParser archiveParser = new(data);
            foreach (MinecraftPackContainer container in archiveParser.FoundPacks) {
                string serverPath = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetSettingsProp(ServerPropertyKeys.ServerPath).ToString();
                string levelName = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp(MmsDependServerPropKeys.LevelName).ToString();
                string knownPacksFile = GetServerFilePath(ServerFileNameKeys.ValidKnownPacks, serverPath);
                string filePath;
                if (container.ManifestType == MinecraftPackTypeStrings[MinecraftPackTypes.WorldPack]) {
                    FileUtilities.CopyFolderTree(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo(GetServerDirectory(ServerDirectoryKeys.ServerWorldDir_LevelName, serverPath, container.FolderName)));
                }
                if (container.ManifestType == MinecraftPackTypeStrings[MinecraftPackTypes.Behavior]) {
                    filePath = GetServerFilePath(ServerFileNameKeys.WorldBehaviorPacks, serverPath);
                    if (MinecraftFileUtilities.UpdateWorldPackFile(filePath, container.JsonManifest) && MinecraftFileUtilities.UpdateKnownPackFile(knownPacksFile, container)) {
                        FileUtilities.CopyFolderTree(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($@"{filePath}\{container.FolderName}"));
                    }
                }
                if (container.ManifestType == MinecraftPackTypeStrings[MinecraftPackTypes.Resource]) {
                    filePath = GetServerFilePath(ServerFileNameKeys.WorldResourcePacks, serverPath);
                    if (MinecraftFileUtilities.UpdateWorldPackFile(filePath, container.JsonManifest) && MinecraftFileUtilities.UpdateKnownPackFile(knownPacksFile, container)) {
                        FileUtilities.CopyFolderTree(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($@"{filePath}\{container.FolderName}"));
                    }
                }
                _logger.AppendLine($@"{container.GetFixedManifestType()} pack installed to server: {container.JsonManifest.header.name}.");
            }
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}

