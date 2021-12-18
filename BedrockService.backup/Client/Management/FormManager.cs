using BedrockService.Client.Forms;
using BedrockService.Client.Networking;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace BedrockService.Client.Management
{
    public sealed class FormManager
    {
        private static readonly IProcessInfo processInfo = new ServiceProcessInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileName(Assembly.GetExecutingAssembly().Location), Process.GetCurrentProcess().Id, false, true);
        private static readonly ILogger Logger = new ClientLogger(processInfo);
        private static MainWindow main;
        private static TCPClient client;
        public static MainWindow MainWindow
        {
            get
            {
                if (main == null || main.IsDisposed)
                {
                    main = new MainWindow(processInfo, Logger);
                }
                return main;
            }
        }

        public static TCPClient TCPClient
        {
            get
            {
                if (client == null)
                {
                    client = new TCPClient(Logger);
                }
                return client;
            }
        }
    }
}
