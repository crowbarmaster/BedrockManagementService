using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace MinecraftService.Shared.SerializeModels {
    public class BackupInfoModel {
        public DateTime Timestamp { get; set; }
        public string Filename { get; set; }
        public int SizeInKb { get; set; }
        private const string _backupStringTemplate = "Backup-yyyyMMdd_HHmmssff.zip";
        private const string _backupFilenamePrefix = "Backup-";
        private const string _timestampTemplate = "yyyyMMdd_HHmmssff";

        public BackupInfoModel(FileInfo backupFileInfo) {
            Filename = backupFileInfo.Name;
            if (backupFileInfo.Exists) {
                SizeInKb = (int)(backupFileInfo.Length / 1000);
            } else {
                SizeInKb = 0;
            }
            if (backupFileInfo.Name.Length != _backupStringTemplate.Length) {
                throw new ArgumentOutOfRangeException($"File name length of file {backupFileInfo.Name} was not as expected! Omitted from backup enumeration.");
            }
            string timeStampFromName = backupFileInfo.Name.Substring(_backupFilenamePrefix.Length, _timestampTemplate.Length);
            if (!DateTime.TryParseExact(timeStampFromName, "yyyyMMdd_HHmmssff", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime)) {
                throw new FormatException($"DateTime format of file {backupFileInfo.Name} did not match! Omitted from backup enumeration.");
            }
            Timestamp = parsedTime;
        }

        public BackupInfoModel() {
            Filename = "-----.zip";
            SizeInKb = 0;
            Timestamp = DateTime.MinValue;
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
