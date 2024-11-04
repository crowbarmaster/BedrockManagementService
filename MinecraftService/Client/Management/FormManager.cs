// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using MinecraftService.Client.Forms;
using MinecraftService.Client.Networking;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;

namespace MinecraftService.Client.Management {
    public sealed class FormManager {
        public static readonly ProcessInfo processInfo = new ProcessInfo("Client", Path.GetDirectoryName(Application.ExecutablePath), Process.GetCurrentProcess().Id, false, true);
        public static readonly ServiceConfigurator ClientLogContainer;
        public static readonly MmsLogger Logger;
        private static MainWindow main;
        private static TCPClient client;

        static FormManager() {
            ClientLogContainer = new ServiceConfigurator(processInfo, new());
            ClientLogContainer.InitializeDefaults();
            Logger = new MmsLogger(processInfo, ClientLogContainer);
            Logger.AppendLine($"Bedrock Client version {Application.ProductVersion} has started.");
            Logger.AppendLine($"Working directory: {processInfo.GetDirectory()}");
            SharedStringBase.SetWorkingDirectory(processInfo);
            client = new TCPClient(Logger);
            main = new MainWindow(processInfo, Logger);
        }

        public static MainWindow MainWindow {
            get {
                if (main == null || main.IsDisposed) {
                    main = new MainWindow(processInfo, Logger);
                }
                return main;
            }
        }

        public static TCPClient TCPClient {
            get {
                if (client == null) {
                    client = new TCPClient(Logger);
                }
                return client;
            }
        }
    }
}
