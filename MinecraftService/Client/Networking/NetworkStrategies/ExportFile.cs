// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Threading.Tasks;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class ExportFile : INetworkMessage {

        public Task<bool> ProcessMessage(byte[] messageData) => Task.Run(() => {
            string data = Encoding.UTF8.GetString(messageData, 5, messageData.Length - 5);
            ExportImportFileModel exportModel = JsonConvert.DeserializeObject<ExportImportFileModel>(data, SharedStringBase.GlobalJsonSerialierSettings);
            if (exportModel != null) {
                FormManager.MainWindow.RecieveExportData(exportModel).Wait();
            }
            return true;
        });
    }
}
