using BedrockService.Client.Forms;
using BedrockService.Client.Networking;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace BedrockService.Client.Management {
    public sealed class FormManager {
        private static readonly IProcessInfo processInfo = new ServiceProcessInfo("Client", Path.GetDirectoryName(Application.ExecutablePath), Process.GetCurrentProcess().Id, false, true);
        public static readonly IServiceConfiguration ClientLogContainer;
        private static readonly IBedrockLogger _logger;
        private static MainWindow main;
        private static TCPClient client;

        static FormManager() {
            ClientLogContainer = new ServiceConfigurator(processInfo);
            ClientLogContainer.InitializeDefaults();
            ClientLogContainer.SetProp(new Property("LogApplicationOutput", "true") { Value = "true" });
            _logger = new BedrockLogger(processInfo, ClientLogContainer);
            _logger.AppendLine($"Bedrock Client version {Application.ProductVersion} has started.");
            _logger.AppendLine($"Working directory: {processInfo.GetDirectory()}");
        }

        public static MainWindow MainWindow {
            get {
                if (main == null || main.IsDisposed) {
                    main = new MainWindow(processInfo, _logger);
                }
                return main;
            }
        }

        public static TCPClient TCPClient {
            get {
                if (client == null) {
                    client = new TCPClient(_logger);
                }
                return client;
            }
        }
    }
}
