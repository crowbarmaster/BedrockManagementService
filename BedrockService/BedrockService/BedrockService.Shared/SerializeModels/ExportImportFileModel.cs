using BedrockService.Shared.Classes;

namespace BedrockService.Shared.SerializeModels {
    public class ExportImportFileModel {
        public FileTypeFlags FileType;
        public string Filename;
        public byte[] Data;
        public PackageFlags PackageFlags;
    }
}
