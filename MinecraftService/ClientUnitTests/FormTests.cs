using BedrockService.Client.Forms;
using BedrockService.Client.Management;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ClientUnitTests
{
    [TestClass]
    public class FormTests
    {
        [TestMethod]
        public void TestClientConfigForm()
        {
            List<IClientSideServiceConfiguration> clientSideServiceConfigurations = new List<IClientSideServiceConfiguration>();
            clientSideServiceConfigurations.Add(new ClientSideServiceConfiguration("test1", "address1", "1234"));
            clientSideServiceConfigurations.Add(new ClientSideServiceConfiguration("test2", "4ddr3552", "3456"));
            clientSideServiceConfigurations.Add(new ClientSideServiceConfiguration("test3", "address3", "5678"));

            using (ClientConfigForm form = new ClientConfigForm(new ConfigManager(new ClientLogger(new ServiceProcessInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileName(Assembly.GetExecutingAssembly().Location), Process.GetCurrentProcess().Id, false, true)))))
            {
                form.Show();
                System.Timers.Timer timer = new System.Timers.Timer(6000);
                form.SimulateTests();
                Task.Delay(6000).Wait();
            }
        }
    }
}
