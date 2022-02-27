using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Shared.SerializeModels {
    public class BackupInfoModel {
        public DateTime Timestamp { get; set; }
        public string Filename { get; set; }
        public int SizeInKb { get; set; }

        public BackupInfoModel(FileInfo backupFileInfo) {
            Filename = backupFileInfo.Name;
            if (backupFileInfo.Exists) {
                SizeInKb = (int)(backupFileInfo.Length / 1000);
            } else {
                SizeInKb = 0;
            }
            try {
                string timeStampFromName = backupFileInfo.Name.Substring(7, backupFileInfo.Name.Length - 7).Substring(0, 17);
                Timestamp = DateTime.ParseExact(timeStampFromName, "yyyyMMdd_HHmmssff", CultureInfo.InvariantCulture);
            } catch {
                Timestamp = DateTime.MinValue;
            }
        }

        [JsonConstructor]
        public BackupInfoModel(string filename, int size, DateTime stamp) {
            Filename = filename;
            SizeInKb = size;
            Timestamp = stamp;
        }

        public string[] GetBackupInfo() {
            return new string[5] { $"Filename: {Filename}", $"Created on: {Timestamp.ToString("G")}", $"Size of backup: {SizeInKb / 1000f} MB", "", "-----------------------------------------------------------" };
        }

        public override string ToString() {
            return Filename;
        }
    }
}
