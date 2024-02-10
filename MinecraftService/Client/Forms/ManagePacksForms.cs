using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.LiteLoaderJsonModels;
using MinecraftService.Shared.PackParser;
using MinecraftService.Shared.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinecraftService.Client.Forms {
    public partial class ManagePacksForms : Form {
        private readonly byte _serverIndex;
        private readonly IServerLogger _logger;
        private readonly IProcessInfo _processInfo;
        private readonly DirectoryInfo _packExtractDir;
        public ManagePacksForms(byte serverIndex, IServerLogger logger, IProcessInfo processInfo) {
            _logger = logger;
            _packExtractDir = new DirectoryInfo($"{Path.GetTempPath()}{FileUtilities.GetRandomPrefix()}MMSTemp\\ExaminePacks");
            _processInfo = processInfo;
            _serverIndex = serverIndex;
            InitializeComponent();
        }

        public void PopulateServerData(List<MinecraftPackContainer> packList, LLServerPluginRegistry pluginReg = null) {
            foreach (MinecraftPackContainer container in packList) {
                serverListBox.Items.Add(container);
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

        private void removePacksBtn_Click(object sender, EventArgs e) {
            if (serverListBox.CheckedItems.Count > 0) {
                List<MinecraftPackContainer> temp = new();
                object[] items = new object[serverListBox.CheckedItems.Count];
                serverListBox.CheckedItems.CopyTo(items, 0);
                foreach (object item in items) {
                    temp.Add((MinecraftPackContainer)item);
                    serverListBox.Items.Remove(item);
                }
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                FormManager.TCPClient.SendData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(temp, Formatting.Indented, settings)), _serverIndex, NetworkMessageTypes.RemovePack);
            }
            DialogResult = DialogResult.OK;
        }

        private void sendPacksBtn_Click(object sender, EventArgs e) {
            if (parsedPacksListBox.CheckedItems.Count > 0) {
                object[] items = new object[parsedPacksListBox.CheckedItems.Count];
                parsedPacksListBox.CheckedItems.CopyTo(items, 0);
                SendPacks(items);
            }
        }

        private void openFileBtn_Click(object sender, EventArgs e) {
            OpenFileDialog openFileDialog = new();
            ProgressDialog progressDialog = new(null);
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "MC pack file (.MCWORLD, .MCPACK, .MCADDON, .Zip)|*.mcworld;*.mcpack;*.mcaddon;*.zip";
            openFileDialog.Title = "Select pack file(s)";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                progressDialog.SetCallback(Task.Run(() => {
                    MinecraftPackParser parser = new(_logger, progressDialog.GetDialogProgress(), _packExtractDir.FullName);
                    parser.ProcessClientFiles(openFileDialog.FileNames, () => {
                        Invoke(() => {
                            parsedPacksListBox.Items.Clear();
                            foreach (MinecraftPackContainer container in parser.FoundPacks) {
                                parsedPacksListBox.Items.Add(container);
                            }
                            progressDialog.EndProgress(null);
                        });
                    });
                }));
                progressDialog.Show(this);
            }
        }

        private void SendPacks(object[] packList) {
            FileUtilities.CreateInexistantDirectory($@"{_packExtractDir.FullName}\BuildZip");
            foreach (MinecraftPackContainer container in packList) {
                try {
                    DirectoryInfo directoryInfo = new(container.PackContentLocation);
                    string packDir = $@"{_packExtractDir.FullName}\BuildZip\{directoryInfo.Name}";
                    directoryInfo.MoveTo(packDir);
                    container.PackContentLocation = packDir;
                } catch (Exception ex) {
                    _logger.AppendLine($"Error adding {container.FolderName} to pack zip.\r\nError message: {ex.Message}");
                }
            }
            ZipFile.CreateFromDirectory($@"{_packExtractDir.FullName}\BuildZip", $@"{_packExtractDir.FullName}\SendZip.zip");
            FormManager.TCPClient.SendData(File.ReadAllBytes($@"{_packExtractDir.FullName}\SendZip.zip"), _serverIndex, NetworkMessageTypes.PackFile);
            DialogResult = DialogResult.OK;
        }

        private void checkAllServerButton_Click(object sender, EventArgs e) {
            for (int i = 0; i < serverListBox.Items.Count; i++) {
                serverListBox.SetItemChecked(i, true);
            }
        }

        private void checkAllLocalButton_Click(object sender, EventArgs e) {
            for (int i = 0; i < parsedPacksListBox.Items.Count; i++) {
                parsedPacksListBox.SetItemChecked(i, true);
            }
        }
       
        private void uncheckAllServerButton_Click(object sender, EventArgs e) {
            for (int i = 0; i < serverListBox.Items.Count; i++) {
                serverListBox.SetItemChecked(i, false);
            }
        }

        private void uncheckAllLocalButton_Click(object sender, EventArgs e) {
            for (int i = 0; i < parsedPacksListBox.Items.Count; i++) {
                parsedPacksListBox.SetItemChecked(i, false);
            }
        }
    }
}
