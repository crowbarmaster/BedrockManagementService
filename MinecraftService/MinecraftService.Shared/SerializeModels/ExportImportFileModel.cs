using MinecraftService.Shared.Classes;

namespace MinecraftService.Shared.SerializeModels {
    public class ExportImportFileModel {
        public FileTypeFlags FileType;
        public string Filename;
        public byte[] Data;
        public PackageFlags PackageFlags;
    }
}
