
using BedrockService.Client.Forms;
using BedrockService.Client.Networking;

namespace BedrockService.Client.Management
{
    public sealed class FormManager
    {
        private static MainWindow main;
        private static TCPClient client;
        public static MainWindow GetMainWindow
        {
            get
            {
                if (main == null || main.IsDisposed)
                {
                    main = new MainWindow();
                }
                return main;
            }
        }

        public static TCPClient GetTCPClient
        {
            get
            {
                if (client == null)
                {
                    client = new TCPClient();
                }
                return client;
            }
        }
    }
}
