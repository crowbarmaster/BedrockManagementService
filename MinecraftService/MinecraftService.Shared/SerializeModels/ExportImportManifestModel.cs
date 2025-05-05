using MinecraftService.Shared.Classes.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.LayoutRenderers.Wrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace MinecraftService.Shared.SerializeModels {

    [Flags]
    public enum FileTypes {
        Undefined = 0,
        ServerConfig = 1,
        ServiceConfig = 2,
        ServerPacks = 4,
        PlayerDB = 8,
        WorldBackup = 16,
        BedrockServer = 32,
        BedrockPlayerData = 64,
        BedrockWorld = 128
    }

    public struct ExportImportManifestModel : IEquatable<ExportImportManifestModel> {
        public string Filename;
        public FileTypes FileType;

        public ExportImportManifestModel() { }

        public static readonly ExportImportManifestModel Empty = new();

        public override bool Equals(object obj) {
            return obj is ExportImportManifestModel model && Equals(model);
        }

        public bool Equals(ExportImportManifestModel other) {
            return Filename == other.Filename &&
                   FileType == other.FileType;
        }

        public override int GetHashCode() {
            return HashCode.Combine(Filename, FileType);
        }

        public static bool operator ==(ExportImportManifestModel left, ExportImportManifestModel right) {
            return left.Equals(right);
        }

        public static bool operator !=(ExportImportManifestModel left, ExportImportManifestModel right) {
            return !(left == right);
        }
    }

    public struct ExportImportFileModel : IEquatable<ExportImportFileModel> {
        public byte[] Data;
        public ExportImportManifestModel Manifest;
        private readonly string _manifestFileName = "MMS_Manifest.json";

        public ExportImportFileModel(byte[] zippedData) {
            Data = zippedData;
            bool isZipFile = Data[0] == 0x50 && Data[1] == 0x4B && Data[2] == 0x03 && Data[3] == 0x04;
            byte[] headerBytes = new byte[4];
            Buffer.BlockCopy(zippedData, 0, headerBytes, 0, headerBytes.Length);
            if (!isZipFile) throw new Exception($"Export/Import model only supports zip file data. Header: {Convert.ToHexString(headerBytes)}");
            MemoryStream stream = new(Data);
            ZipArchive archive = new(stream);
            foreach (ZipArchiveEntry entry in archive.Entries) {

                if (entry.Name == _manifestFileName) {
                    byte[] entryBytes = new byte[entry.Length];
                    using Stream entryStream = entry.Open();
                    entryStream.Read(entryBytes, 0, entryBytes.Length);
                    Manifest = JsonConvert.DeserializeObject<ExportImportManifestModel>(Encoding.UTF8.GetString(entryBytes));
                }
            }
            if (Manifest == ExportImportManifestModel.Empty) {
                Manifest.FileType = FileTypes.Undefined;
                if (archive.Entries.Where(x => x.Name.Equals("server.properties")).Any()) {
                    Manifest.FileType |= FileTypes.BedrockServer;
                }
                if (archive.Entries.Where(x => x.Name.Contains(".preg")).Any()) {
                    Manifest.FileType |= FileTypes.PlayerDB;
                }
                if(archive.Entries.Where(x => x.Name.Equals("Service.conf")).Any()) {
                    Manifest.FileType |= FileTypes.ServiceConfig;
                }
                if (archive.Entries.Where(x => x.Name.Contains(".conf")).Any()) {
                    Manifest.FileType |= FileTypes.ServiceConfig;
                }
                if (archive.Entries.Where(x => x.Name.Equals("permissions.json")).Any() || archive.Entries.Where(x => x.Name.Equals("allowlist.json")).Any() || archive.Entries.Where(x => x.Name.Equals("permissions.json")).Any()) {
                    Manifest.FileType |= FileTypes.BedrockPlayerData;
                }
                if (Manifest.FileType == FileTypes.Undefined) {
                    throw new InvalidOperationException("Could not locate anything to process in this archive!");
                }
            }
        }

        public override bool Equals(object obj) {
            return obj is ExportImportFileModel model && Equals(model);
        }

        public bool Equals(ExportImportFileModel other) {
            return EqualityComparer<byte[]>.Default.Equals(Data, other.Data) &&
                   Manifest.Equals(other.Manifest);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Data, Manifest);
        }

        public static bool operator ==(ExportImportFileModel left, ExportImportFileModel right) {
            return left.Equals(right);
        }

        public static bool operator !=(ExportImportFileModel left, ExportImportFileModel right) {
            return !(left == right);
        }
    }
}
