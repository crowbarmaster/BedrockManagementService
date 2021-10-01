using BedrockService.Client.Management;
using BedrockService.Service.Networking;
using BedrockService.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BedrockService.Service.Server.PackParser;
using System.IO.Compression;

namespace BedrockService.Client.Forms
{
    public partial class ManagePacksForms : Form
    {
        private byte ServerIndex = 0x00;
        private DirectoryInfo PackExtractDir = new DirectoryInfo(@"C:\");
        public ManagePacksForms(byte serverIndex)
        {
            ServerIndex = serverIndex;
            InitializeComponent();
            //FormManager.GetTCPClient.SendData(File.ReadAllBytes($@"E:\testRB.zip"), NetworkMessageSource.Client, NetworkMessageDestination.Server, ServerIndex, NetworkMessageTypes.PackFile);
        }

        public void PopulateServerPacks (List<MinecraftPackParser> packList)
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
            if(thisBox.SelectedIndex != -1)
            {
                MinecraftPackContainer selectedPack = (MinecraftPackContainer)thisBox.SelectedItem;
                if(selectedPack.IconBytes != null)
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
                FormManager.GetTCPClient.SendData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(temp)), NetworkMessageSource.Client, NetworkMessageDestination.Server, ServerIndex, NetworkMessageTypes.RemovePack);
                serverListBox.Items.Remove(temp[0]);
            }

        }

        private void removeAllPacksBtn_Click(object sender, EventArgs e)
        {
            List<MinecraftPackContainer> temp = new List<MinecraftPackContainer>();
            foreach(object item in serverListBox.Items)
                temp.Add((MinecraftPackContainer)item);
            FormManager.GetTCPClient.SendData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(temp)), NetworkMessageSource.Client, NetworkMessageDestination.Server, ServerIndex, NetworkMessageTypes.RemovePack);
            serverListBox.Items.Clear();
        }

        private void sendPacksBtn_Click(object sender, EventArgs e)
        {
            if (parsedPacksListBox.SelectedIndex != -1)
            {
                foreach (object pack in parsedPacksListBox.SelectedItems)
                {
                    Directory.CreateDirectory($@"{PackExtractDir.FullName}\ZipTemp");
                    MinecraftPackContainer container = (MinecraftPackContainer)pack;
                    container.PackContentLocation.Parent.MoveTo($@"{PackExtractDir.FullName}\ZipTemp\{container.PackContentLocation.Name}");
                    container.PackContentLocation = new DirectoryInfo($@"{PackExtractDir.FullName}\ZipTemp\{container.PackContentLocation.Name}");
                }
                ZipFile.CreateFromDirectory($@"{PackExtractDir.FullName}\ZipTemp", $@"{PackExtractDir.FullName}\SendZip.zip");
                FormManager.GetTCPClient.SendData(File.ReadAllBytes($@"{PackExtractDir.FullName}\SendZip.zip"), NetworkMessageSource.Client, NetworkMessageDestination.Server, ServerIndex, NetworkMessageTypes.PackFile);
                parsedPacksListBox.Items.Clear();
            }
        }

        private void sendAllBtn_Click(object sender, EventArgs e)
        {
            foreach (MinecraftPackContainer container in parsedPacksListBox.Items)
            {
                Directory.CreateDirectory($@"{PackExtractDir.FullName}\ZipTemp");
                container.PackContentLocation.MoveTo($@"{PackExtractDir.FullName}\ZipTemp\{container.PackContentLocation.Name}");
                container.PackContentLocation = new DirectoryInfo($@"{PackExtractDir.FullName}\ZipTemp\{container.PackContentLocation.Name}");
            }
            ZipFile.CreateFromDirectory($@"{PackExtractDir.FullName}\ZipTemp", $@"{PackExtractDir.FullName}\SendZip.zip");
            FormManager.GetTCPClient.SendData(File.ReadAllBytes($@"{PackExtractDir.FullName}\SendZip.zip"), NetworkMessageSource.Client, NetworkMessageDestination.Server, ServerIndex, NetworkMessageTypes.PackFile);
            parsedPacksListBox.Items.Clear();

        }

        private void openFileBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "MC pack file (.MCWORLD, .MCPACK, .MCADDON, .Zip)|*.mcworld;*.mcpack;*.mcaddon;*.zip";
            openFileDialog.Title = "Select pack file(s)";
            openFileDialog.Multiselect = true;
            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                MinecraftPackParser parser = new MinecraftPackParser(openFileDialog.FileNames);
                PackExtractDir = parser.PackExtractDirectory;
                parsedPacksListBox.Items.Clear();
                foreach (MinecraftPackContainer container in parser.FoundPacks)
                    parsedPacksListBox.Items.Add(container);
            }
        }
    }
}
