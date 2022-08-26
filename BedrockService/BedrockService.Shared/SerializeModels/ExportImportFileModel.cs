using BedrockService.Shared.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Shared.SerializeModels {
    public class ExportImportFileModel {
        public FileTypeFlags FileType;
        public string Filename;
        public byte[] Data;
        public PackageFlags PackageFlags;
    }
}
