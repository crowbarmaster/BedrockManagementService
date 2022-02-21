using BedrockService.Client.Forms;
using BedrockService.Client.Networking;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using BedrockService.Shared.Utilities;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace BedrockService.Client.Management {
    public sealed class FormManager {
        public static readonly IProcessInfo processInfo = new ServiceProcessInfo("Client", Path.GetDirectoryName(Application.ExecutablePath), Process.GetCurrentProcess().Id, false, true);
        public static readonly IServiceConfiguration ClientLogContainer;
        public static readonly IBedrockLogger Logger;
        private static MainWindow main;
        private static TCPClient client;

        static FormManager() {
            ClientLogContainer = new ServiceConfigurator(processInfo);
            ClientLogContainer.InitializeDefaults();
            Logger = new BedrockLogger(processInfo, ClientLogContainer);
            Logger.AppendLine($"Bedrock Client version {Application.ProductVersion} has started.");
            Logger.AppendLine($"Working directory: {processInfo.GetDirectory()}");
            if (UpgradeAssistant_26RC2.IsClientUpgradeRequired(processInfo.GetDirectory())) {
                UpgradeAssistant_26RC2.PerformClientUpgrade(processInfo.GetDirectory());
            }
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
