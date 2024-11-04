using MinecraftService.Shared.Classes.Networking;

namespace MinecraftService.Shared.SerializeModels
{
    public class ExportImportManifestModel {
        public string Filename;
        public FileTypeFlags FileType;
        public PackageFlags PackageFlags;

        public ExportImportManifestModel(ExportImportFileModel exportImportFile) {
            Filename = exportImportFile.Filename;
            FileType = exportImportFile.FileType;
            PackageFlags = exportImportFile.PackageFlags;
        }
    }
}
