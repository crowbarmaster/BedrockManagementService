using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftService.Shared.JsonModels.Service.FileProcessors {
    public class BedrockServerArchive(MmsLogger logger) : IFileProcessor {
        public void Process(byte[] fileBytes) {
            if (fileBytes == null) throw new ArgumentNullException(nameof(fileBytes));
            MemoryStream stream = new MemoryStream(fileBytes);
            string tempPath = SharedStringBase.GetNewTempDirectory("BedrockImport");
            Progress<ProgressModel> progress = new Progress<ProgressModel>((progress) => {
                string prog = string.Format("{0:N2}", progress.Progress);
                logger.AppendLine($"Extracting bedrock server contents... {prog}% completed.");
            });
            ZipUtilities.ExtractToDirectory(stream, tempPath, progress);

            List<string[]> serverProps = MinecraftFileUtilities.FilterLinesFromPropFile($"{tempPath}\\{SharedStringBase.GetServerFileName(SharedStringBase.ServerFileNameKeys.ServerProps)}");
        }
    }
}
