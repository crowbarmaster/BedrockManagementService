// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Threading.Tasks;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class ServerStatusRequest : INetworkMessage {

        public Task<bool> ProcessMessage(Message message) => Task.Run(() => {
            StatusUpdateModel status = JsonConvert.DeserializeObject<StatusUpdateModel>(Encoding.UTF8.GetString(message.Data), SharedStringBase.GlobalJsonSerialierSettings);
            if (status != null && status.ServerStatusModel != null && status.ServerStatusModel.ServerIndex != 255 && FormManager.MainWindow.SelectedServer != null) {
                ServerStatusModel formerServerStatus = FormManager.MainWindow.SelectedServer.GetStatus();
                if (!status.ServerStatusModel.Equals(formerServerStatus)) {
                    FormManager.MainWindow.connectedHost.GetServerInfoByIndex(status.ServerStatusModel.ServerIndex).SetStatus(status.ServerStatusModel);
                    FormManager.MainWindow.Invoke(() => FormManager.MainWindow.RefreshAllCompenentStates());
                    FormManager.TCPClient.SendData(new() {
                        ServerIndex = message.ServerIndex,
                        Type = MessageTypes.EnumBackups
                    });
                }
                FormManager.MainWindow.ServiceStatus = status.ServiceStatusModel;
            }
            return true;
        });
    }
}
