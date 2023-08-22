using BedrockService.Client.Management;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using BedrockService.Shared.PackParser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Forms;

namespace BedrockService.Client.Forms {
    public partial class ManagePacksForms : Form {
        private readonly byte _serverIndex;
        private readonly IServerLogger _logger;
        private readonly IProcessInfo _processInfo;
        private readonly DirectoryInfo _packExtractDir;
        public ManagePacksForms(byte serverIndex, IServerLogger logger, IProcessInfo processInfo) {
            _logger = logger;
            _packExtractDir = new DirectoryInfo($"{Path.GetTempPath()}\\BMSTemp");
            _processInfo = processInfo;
            _serverIndex = serverIndex;
            InitializeComponent();
        }

        public void PopulateServerData(List<MinecraftPackContainer> packList, LLServerPluginRegistry pluginReg = null) {
            foreach (MinecraftPackContainer container in packList) {
                serverListBox.Items.Add(container);
            }
            if (pluginReg != null) {
                BmsServerPluginDatabase db = pluginReg.ServerPluginList[_serverIndex];
                foreach (PluginVersionInfo info in db.InstalledPlugins) {

                }
            }
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e) {
            ListBox thisBox = (ListBox)sender;
            if (thisBox == serverListBox)
                parsedPacksListBox.SelectedIndex = -1;
            if (thisBox == parsedPacksListBox)
                serverListBox.SelectedIndex = -1;
            if (thisBox.SelectedIndex != -1) {
                MinecraftPackContainer selectedPack = (MinecraftPackContainer)thisBox.SelectedItem;
                if (selectedPack.IconBytes != null)
                    using (MemoryStream ms = new(selectedPack.IconBytes)) {
                        selectedPackIcon.Image = Image.FromStream(ms);
                    }
                if (selectedPack.JsonManifest != null) {
                    textBox1.Text = $"{selectedPack.JsonManifest.header.name}\r\n{selectedPack.JsonManifest.header.description}\r\n{selectedPack.JsonManifest.header.uuid}\r\n{selectedPack.JsonManifest.header.version[0]}";
                } else {
                    textBox1.Text = new DirectoryInfo(selectedPack.PackContentLocation).Name;
                }
            }
        }

        private void removePackBtn_Click(object sender, EventArgs e) {
            if (serverListBox.SelectedIndex != -1) {
                List<MinecraftPackContainer> temp = new();
                object[] items = new object[serverListBox.SelectedItems.Count];
                serverListBox.SelectedItems.CopyTo(items, 0);
                foreach (object item in items) {
                    temp.Add((MinecraftPackContainer)item);
                    serverListBox.Items.Remove(item);
                }
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                FormManager.TCPClient.SendData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(temp, Formatting.Indented, settings)), _serverIndex, NetworkMessageTypes.RemovePack);
            }
            DialogResult = DialogResult.OK;
        }

        private void removeAllPacksBtn_Click(object sender, EventArgs e) {
            List<MinecraftPackContainer> temp = new();
            foreach (object item in serverListBox.Items) {
                temp.Add((MinecraftPackContainer)item);
            }
            JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
            FormManager.TCPClient.SendData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(temp, Formatting.Indented, settings)), _serverIndex, NetworkMessageTypes.RemovePack);
            DialogResult = DialogResult.OK;
        }

        private void sendPacksBtn_Click(object sender, EventArgs e) {
            if (parsedPacksListBox.SelectedIndex != -1) {
                object[] items = new object[parsedPacksListBox.SelectedItems.Count];
                parsedPacksListBox.SelectedItems.CopyTo(items, 0);
                SendPacks(items);
            }
        }

        private void sendAllBtn_Click(object sender, EventArgs e) {
            if (parsedPacksListBox.Items.Count < 1) {
                return;
            }
            object[] items = new object[parsedPacksListBox.Items.Count];
            parsedPacksListBox.Items.CopyTo(items, 0);
            SendPacks(items);
        }

        private void openFileBtn_Click(object sender, EventArgs e) {
            OpenFileDialog openFileDialog = new();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "MC pack file (.MCWORLD, .MCPACK, .MCADDON, .Zip)|*.mcworld;*.mcpack;*.mcaddon;*.zip";
            openFileDialog.Title = "Select pack file(s)";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                MinecraftPackParser parser = new(openFileDialog.FileNames, _packExtractDir.FullName);
                parsedPacksListBox.Items.Clear();
                foreach (MinecraftPackContainer container in parser.FoundPacks)
                    parsedPacksListBox.Items.Add(container);
            }
        }

        private void SendPacks(object[] packList) {
            foreach (MinecraftPackContainer container in packList) {
                Directory.CreateDirectory($@"{_packExtractDir.FullName}\ZipTemp");
                DirectoryInfo directoryInfo = new(container.PackContentLocation);
                directoryInfo.MoveTo($@"{_packExtractDir.FullName}\ZipTemp\{directoryInfo.Name}");
                container.PackContentLocation = $@"{_packExtractDir.FullName}\ZipTemp\{directoryInfo.Name}";
            }
            ZipFile.CreateFromDirectory($@"{_packExtractDir.FullName}\ZipTemp", $@"{_packExtractDir.FullName}\SendZip.zip");
            FormManager.TCPClient.SendData(File.ReadAllBytes($@"{_packExtractDir.FullName}\SendZip.zip"), _serverIndex, NetworkMessageTypes.PackFile);
            DialogResult = DialogResult.OK;
        }
    }
}
