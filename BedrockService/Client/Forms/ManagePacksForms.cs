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

namespace BedrockService.Client.Forms
{
    public partial class ManagePacksForms : Form
    {
        private byte ServerIndex = 0x00;
        private ILogger Logger;
        private IProcessInfo ProcessInfo;
        private DirectoryInfo PackExtractDir;
        public ManagePacksForms(byte serverIndex, ILogger logger, IProcessInfo processInfo)
        {
            Logger = logger;
            PackExtractDir = new DirectoryInfo($@"{processInfo.GetDirectory()}\Temp");
            ProcessInfo = processInfo;
            ServerIndex = serverIndex;
            InitializeComponent();
        }

        public void PopulateServerPacks(List<MinecraftPackParser> packList)
        {
            foreach (MinecraftPackParser pack in packList)
                foreach (MinecraftPackContainer container in pack.FoundPacks)
                    serverListBox.Items.Add(container);
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox thisBox = (ListBox)sender;
            if (thisBox == serverListBox)
                parsedPacksListBox.SelectedIndex = -1;
            if (thisBox == parsedPacksListBox)
                serverListBox.SelectedIndex = -1;
            if (thisBox.SelectedIndex != -1)
            {
                MinecraftPackContainer selectedPack = (MinecraftPackContainer)thisBox.SelectedItem;
                if (selectedPack.IconBytes != null)
                    using (MemoryStream ms = new MemoryStream(selectedPack.IconBytes))
                    {
                        selectedPackIcon.Image = Image.FromStream(ms);
                    }
                if (selectedPack.JsonManifest != null)
                    textBox1.Text = $"{selectedPack.JsonManifest.header.name}\r\n{selectedPack.JsonManifest.header.description}\r\n{selectedPack.JsonManifest.header.uuid}\r\n{selectedPack.JsonManifest.header.version[0]}";
                else
                    textBox1.Text = selectedPack.PackContentLocation.Name;
            }
        }

        private void removePackBtn_Click(object sender, EventArgs e)
        {
            if (serverListBox.SelectedIndex != -1)
            {
                List<MinecraftPackContainer> temp = new List<MinecraftPackContainer>();
                temp.Add((MinecraftPackContainer)serverListBox.SelectedItem);
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                FormManager.GetTCPClient.SendData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(temp, Formatting.Indented, settings)), NetworkMessageSource.Client, NetworkMessageDestination.Server, ServerIndex, NetworkMessageTypes.RemovePack);
                serverListBox.Items.Remove(temp[0]);
            }

        }

        private void removeAllPacksBtn_Click(object sender, EventArgs e)
        {
            List<MinecraftPackContainer> temp = new List<MinecraftPackContainer>();
            foreach (object item in serverListBox.Items)
                temp.Add((MinecraftPackContainer)item);
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            FormManager.GetTCPClient.SendData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(temp, Formatting.Indented, settings)), NetworkMessageSource.Client, NetworkMessageDestination.Server, ServerIndex, NetworkMessageTypes.RemovePack);
            serverListBox.Items.Clear();
        }

        private void sendPacksBtn_Click(object sender, EventArgs e)
        {
            if (parsedPacksListBox.SelectedIndex != -1)
            {
                object[] items = new object[parsedPacksListBox.SelectedItems.Count];
                parsedPacksListBox.SelectedItems.CopyTo(items, 0);
                SendPacks(items);
            }
        }

        private void sendAllBtn_Click(object sender, EventArgs e)
        {
            object[] items = new object[parsedPacksListBox.Items.Count];
            parsedPacksListBox.Items.CopyTo(items, 0);
            SendPacks(items);
        }

        private void openFileBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "MC pack file (.MCWORLD, .MCPACK, .MCADDON, .Zip)|*.mcworld;*.mcpack;*.mcaddon;*.zip";
            openFileDialog.Title = "Select pack file(s)";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                MinecraftPackParser parser = new MinecraftPackParser(openFileDialog.FileNames, PackExtractDir, Logger, ProcessInfo);
                parsedPacksListBox.Items.Clear();
                foreach (MinecraftPackContainer container in parser.FoundPacks)
                    parsedPacksListBox.Items.Add(container);
            }
        }

        private void SendPacks(object[] packList)
        {
            foreach (MinecraftPackContainer container in packList)
            {
                Directory.CreateDirectory($@"{PackExtractDir.FullName}\ZipTemp");
                container.PackContentLocation.MoveTo($@"{PackExtractDir.FullName}\ZipTemp\{container.PackContentLocation.Name}");
                container.PackContentLocation = new DirectoryInfo($@"{PackExtractDir.FullName}\ZipTemp\{container.PackContentLocation.Name}");
            }
            ZipFile.CreateFromDirectory($@"{PackExtractDir.FullName}\ZipTemp", $@"{PackExtractDir.FullName}\SendZip.zip");
            FormManager.GetTCPClient.SendData(File.ReadAllBytes($@"{PackExtractDir.FullName}\SendZip.zip"), NetworkMessageSource.Client, NetworkMessageDestination.Server, ServerIndex, NetworkMessageTypes.PackFile);
            parsedPacksListBox.Items.Clear();
        }
    }
}
