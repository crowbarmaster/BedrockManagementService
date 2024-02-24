// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Threading.Tasks;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class ServerStatusRequest : INetworkMessage {

        public Task<bool> ProcessMessage(byte[] messageData) => Task.Run(() => {
            string data = Encoding.UTF8.GetString(messageData, 5, messageData.Length - 5);
            StatusUpdateModel status = JsonConvert.DeserializeObject<StatusUpdateModel>(data, SharedStringBase.GlobalJsonSerialierSettings);
            if (status != null && status.ServerStatusModel != null && status.ServerStatusModel.ServerIndex != 255) {
                ServerStatusModel formerServerStatus = FormManager.MainWindow.selectedServer.GetStatus();
                if (!status.ServerStatusModel.Equals(formerServerStatus)) {
                    FormManager.MainWindow.connectedHost.GetServerInfoByIndex(status.ServerStatusModel.ServerIndex).SetStatus(status.ServerStatusModel);
                    FormManager.MainWindow.Invoke(() => FormManager.MainWindow.RefreshAllCompenentStates());
                    FormManager.TCPClient.SendData(messageData[2], NetworkMessageTypes.EnumBackups);
                }
                FormManager.MainWindow.ServiceStatus = status.ServiceStatusModel;
            }
            return true;
        });
    }
}
