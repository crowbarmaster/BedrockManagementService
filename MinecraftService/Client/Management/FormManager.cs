using MinecraftService.Client.Forms;
using MinecraftService.Client.Networking;
using MinecraftService.Shared.Classes;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.Utilities;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace MinecraftService.Client.Management {
    public sealed class FormManager {
        public static readonly IProcessInfo processInfo = new ServiceProcessInfo("Client", Path.GetDirectoryName(Application.ExecutablePath), Process.GetCurrentProcess().Id, false, true);
        public static readonly IServiceConfiguration ClientLogContainer;
        public static readonly IServerLogger Logger;
        private static MainWindow main;
        private static TCPClient client;

        static FormManager() {
            ClientLogContainer = new ServiceConfigurator(processInfo);
            ClientLogContainer.InitializeDefaults();
            Logger = new MinecraftServerLogger(processInfo, ClientLogContainer);
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
