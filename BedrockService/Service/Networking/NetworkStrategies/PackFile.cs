using BedrockService.Service.Networking.Interfaces;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using BedrockService.Shared.PackParser;
using Newtonsoft.Json;
using System.Text;

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
            MinecraftPackParser archiveParser = new MinecraftPackParser(data, _serviceProcessInfo);
            foreach (MinecraftPackContainer container in archiveParser.FoundPacks) {
                string serverPath = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetSettingsProp("ServerPath").ToString();
                string levelName = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("level-name").ToString();
                string knownPacksFile = $@"{serverPath}\valid_known_packs.json";
                string filePath;
                if (container.ManifestType == "WorldPack") {
                    _fileUtils.CopyFolderTree(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($@"{serverPath}\worlds\{container.FolderName}"));
                }
                if (container.ManifestType == "data") {
                    filePath = $@"{serverPath}\worlds\{levelName}\world_behavior_packs.json";
                    if (MinecraftFileUtilities.UpdateWorldPackFile(filePath, container.JsonManifest) && MinecraftFileUtilities.UpdateKnownPackFile(knownPacksFile, container)) {
                        _fileUtils.CopyFolderTree(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($@"{serverPath}\development_behavior_packs\{container.FolderName}"));
                    }
                }
                if (container.ManifestType == "resources") {
                    filePath = $@"{serverPath}\worlds\{levelName}\world_resource_packs.json";
                    if (MinecraftFileUtilities.UpdateWorldPackFile(filePath, container.JsonManifest) && MinecraftFileUtilities.UpdateKnownPackFile(knownPacksFile, container)) {
                        _fileUtils.CopyFolderTree(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($@"{serverPath}\development_resource_packs\{container.FolderName}"));
                    }
                }
                _logger.AppendLine($@"{container.GetFixedManifestType()} pack installed to server: {container.JsonManifest.header.name}.");
            }
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}

