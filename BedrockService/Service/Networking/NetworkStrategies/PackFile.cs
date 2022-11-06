using BedrockService.Service.Networking.Interfaces;
using BedrockService.Shared.MinecraftFileModels.JsonModels;
using BedrockService.Shared.PackParser;
using Newtonsoft.Json;
using System.Text;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class PackFile : IMessageParser {

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
            MinecraftPackParser archiveParser = new MinecraftPackParser(data);
            foreach (MinecraftPackContainer container in archiveParser.FoundPacks) {
                string serverPath = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetSettingsProp(ServerPropertyKeys.ServerPath).ToString();
                string levelName = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp(BmsDependServerPropKeys.LevelName).ToString();
                string knownPacksFile = GetServerFilePath(BdsFileNameKeys.ValidKnownPacks, serverPath);
                string filePath;
                if (container.ManifestType == "WorldPack") {
                    _fileUtils.CopyFolderTree(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo(GetServerDirectory(BdsDirectoryKeys.ServerWorldDir_LevelName, serverPath, container.FolderName)));
                }
                if (container.ManifestType == "data") {
                    filePath = GetServerFilePath(BdsFileNameKeys.WorldBehaviorPacks, serverPath);
                    if (MinecraftFileUtilities.UpdateWorldPackFile(filePath, container.JsonManifest) && MinecraftFileUtilities.UpdateKnownPackFile(knownPacksFile, container)) {
                        _fileUtils.CopyFolderTree(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($@"{filePath}\{container.FolderName}"));
                    }
                }
                if (container.ManifestType == "resources") {
                    filePath = GetServerFilePath(BdsFileNameKeys.WorldResourcePacks, serverPath);
                    if (MinecraftFileUtilities.UpdateWorldPackFile(filePath, container.JsonManifest) && MinecraftFileUtilities.UpdateKnownPackFile(knownPacksFile, container)) {
                        _fileUtils.CopyFolderTree(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($@"{filePath}\{container.FolderName}"));
                    }
                }
                _logger.AppendLine($@"{container.GetFixedManifestType()} pack installed to server: {container.JsonManifest.header.name}.");
            }
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}

