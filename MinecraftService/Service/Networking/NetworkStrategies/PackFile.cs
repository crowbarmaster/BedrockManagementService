using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.PackParser;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class PackFile(ServiceConfigurator serviceConfiguration, MmsLogger logger) : IMessageParser {

        public Message ParseMessage(Message message) {
            Progress<ProgressModel> progress = new Progress<ProgressModel>((progress) => {
                string prog = string.Format("{0:N2}", progress.Progress);
                logger.AppendLine($"Extracting pack contents... {prog}% completed.");
            });
            MinecraftPackParser archiveParser = new(logger, progress);
            archiveParser.ProcessServerData(message.Data, () => {
                foreach (MinecraftPackContainer container in archiveParser.FoundPacks) {
                    IServerConfiguration server = serviceConfiguration.GetServerInfoByIndex(message.ServerIndex);
                    string serverPath = server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString();
                    string levelName = server.GetProp(MmsDependServerPropKeys.LevelName).ToString();
                    string filePath;

                    if (container.ManifestType == MinecraftPackTypeStrings[MinecraftPackTypes.WorldPack]) {
                        FileUtilities.CopyFolderTree(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo(GetServerDirectory(ServerDirectoryKeys.ServerWorldDir_LevelName, serverPath, container.FolderName)));
                    }
                    if (container.ManifestType == MinecraftPackTypeStrings[MinecraftPackTypes.Behavior]) {
                        filePath = GetServerFilePath(ServerFileNameKeys.WorldBehaviorPacks, server, levelName);
                        if (MinecraftFileUtilities.UpdateWorldPackFile(filePath, container.JsonManifest)) {
                            FileUtilities.CopyFolderTree(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($"{GetServerDirectory(ServerDirectoryKeys.BehaviorPacksDir_LevelName, serverPath, levelName)}\\{container.FolderName}"));
                        }
                    }
                    if (container.ManifestType == MinecraftPackTypeStrings[MinecraftPackTypes.Resource]) {
                        filePath = GetServerFilePath(ServerFileNameKeys.WorldResourcePacks, server, levelName);
                        if (MinecraftFileUtilities.UpdateWorldPackFile(filePath, container.JsonManifest)) {
                            FileUtilities.CopyFolderTree(new DirectoryInfo(container.PackContentLocation), new DirectoryInfo($"{GetServerDirectory(ServerDirectoryKeys.ResourcePacksDir_LevelName, serverPath, levelName)}\\{container.FolderName}"));
                        }
                    }
                    logger.AppendLine($@"{container.GetFixedManifestType()} pack installed to server: {container.JsonManifest.header.name}.");
                }
            });
            return Message.EmptyUICallback;
        }
    }
}

