using BedrockService.Client.Management;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
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
        private readonly byte ServerIndex = 0x00;
        private readonly IBedrockLogger Logger;
        private readonly IProcessInfo ProcessInfo;
        private readonly DirectoryInfo PackExtractDir;
        public ManagePacksForms(byte serverIndex, IBedrockLogger logger, IProcessInfo processInfo) {
            Logger = logger;
            PackExtractDir = new DirectoryInfo($@"{processInfo.GetDirectory()}\Temp");
            ProcessInfo = processInfo;
            ServerIndex = serverIndex;
            InitializeComponent();
        }

        public void PopulateServerPacks(List<MinecraftPackContainer> packList) {
            foreach (MinecraftPackContainer container in packList)
                serverListBox.Items.Add(container);
            FormManager.TCPClient.RecievedPacks = null;
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
                    using (MemoryStream ms = new MemoryStream(selectedPack.IconBytes)) {
                        selectedPackIcon.Image = Image.FromStream(ms);
                    }
                if (selectedPack.JsonManifest != null)
                    textBox1.Text = $"{selectedPack.JsonManifest.header.name}\r\n{selectedPack.JsonManifest.header.description}\r\n{selectedPack.JsonManifest.header.uuid}\r\n{selectedPack.JsonManifest.header.version[0]}";
                else
                    textBox1.Text = new DirectoryInfo(selectedPack.PackContentLocation).Name;
            }
        }

        private void removePackBtn_Click(object sender, EventArgs e) {
            if (serverListBox.SelectedIndex != -1) {
                List<MinecraftPackContainer> temp = new List<MinecraftPackContainer>();
                object[] items = new object[serverListBox.SelectedItems.Count];
                serverListBox.SelectedItems.CopyTo(items, 0);
                foreach (object item in items) {
                    temp.Add((MinecraftPackContainer)item);
                    serverListBox.Items.Remove(item);
                }
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                FormManager.TCPClient.SendData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(temp, Formatting.Indented, settings)), NetworkMessageSource.Client, NetworkMessageDestination.Server, ServerIndex, NetworkMessageTypes.RemovePack);
            }
            DialogResult = DialogResult.OK;
        }

        private void removeAllPacksBtn_Click(object sender, EventArgs e) {
            List<MinecraftPackContainer> temp = new List<MinecraftPackContainer>();
            foreach (object item in serverListBox.Items)
                temp.Add((MinecraftPackContainer)item);
            JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
            FormManager.TCPClient.SendData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(temp, Formatting.Indented, settings)), NetworkMessageSource.Client, NetworkMessageDestination.Server, ServerIndex, NetworkMessageTypes.RemovePack);
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
            if (parsedPacksListBox.Items.Count < 1) { return; }
            object[] items = new object[parsedPacksListBox.Items.Count];
            parsedPacksListBox.Items.CopyTo(items, 0);
            SendPacks(items);
        }

        private void openFileBtn_Click(object sender, EventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "MC pack file (.MCWORLD, .MCPACK, .MCADDON, .Zip)|*.mcworld;*.mcpack;*.mcaddon;*.zip";
            openFileDialog.Title = "Select pack file(s)";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                MinecraftPackParser parser = new MinecraftPackParser(openFileDialog.FileNames, PackExtractDir.FullName, ProcessInfo);
                parsedPacksListBox.Items.Clear();
                foreach (MinecraftPackContainer container in parser.FoundPacks)
                    parsedPacksListBox.Items.Add(container);
            }
        }

        private void SendPacks(object[] packList) {
            foreach (MinecraftPackContainer container in packList) {
                Directory.CreateDirectory($@"{PackExtractDir.FullName}\ZipTemp");
                DirectoryInfo directoryInfo = new DirectoryInfo(container.PackContentLocation);
                directoryInfo.MoveTo($@"{PackExtractDir.FullName}\ZipTemp\{directoryInfo.Name}");
                container.PackContentLocation = $@"{PackExtractDir.FullName}\ZipTemp\{directoryInfo.Name}";
            }
            ZipFile.CreateFromDirectory($@"{PackExtractDir.FullName}\ZipTemp", $@"{PackExtractDir.FullName}\SendZip.zip");
            FormManager.TCPClient.SendData(File.ReadAllBytes($@"{PackExtractDir.FullName}\SendZip.zip"), NetworkMessageSource.Client, NetworkMessageDestination.Server, ServerIndex, NetworkMessageTypes.PackFile);
            DialogResult = DialogResult.OK;
        }
    }
}
