using BedrockService.Service.Networking.Interfaces;
using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using System.IO.Compression;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class LiteLoaderPluginFile : IMessageParser {

        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly int _messageHeaderLength = 5;

        public LiteLoaderPluginFile(IServiceConfiguration serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            using MemoryStream memoryStream = new(data, _messageHeaderLength, data.Length - _messageHeaderLength);
            using ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Read);
            PluginVersionInfo pluginVersionInfo = new();
            zipArchive.ExtractToDirectory(GetServerDirectory(BdsDirectoryKeys.LLPlugins, _serviceConfiguration.GetServerInfoByIndex(serverIndex)), true);

            PluginVersionInfo versionInfo = new() {
                BedrockVersion = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetSettingsProp(ServerPropertyKeys.DeployedVersion).StringValue,
                LiteLoaderVersion = _serviceConfiguration.GetSettingsProp(ServerPropertyKeys.DeployedLiteLoaderVersion).StringValue,
                PluginFileName = zipArchive.Entries[0].Name
            };
            _serviceConfiguration.SetServerPluginInfo(serverIndex, versionInfo);
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}