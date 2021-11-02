using Microsoft.VisualStudio.TestTools.UnitTesting;
using BedrockService.Client.Forms;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BedrockService.Client.Management;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Windows.Forms;

namespace ClientUnitTests
{
    [TestClass]
    public class FormTests
    {
        [TestMethod]
        public void TestClientConfigForm()
        {
            List<IClientSideServiceConfiguration> clientSideServiceConfigurations = new List<IClientSideServiceConfiguration>();
            clientSideServiceConfigurations.Add(new ClientSideServiceConfiguration("test1", "address1", "1234", "Test Display Test"));
            clientSideServiceConfigurations.Add(new ClientSideServiceConfiguration("test2", "4ddr3552", "3456", "Test Display Test"));
            clientSideServiceConfigurations.Add(new ClientSideServiceConfiguration("test3", "address3", "5678", "Test Display Test"));

            using (ClientConfigForm form = new ClientConfigForm(clientSideServiceConfigurations, new BedrockService.Client.Management.ConfigManager(new ClientLogger(new ServiceProcessInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileName(Assembly.GetExecutingAssembly().Location), Process.GetCurrentProcess().Id, false, true)))))
            {
                form.Show();
                System.Timers.Timer timer = new System.Timers.Timer(6000);
                form.SimulateTests();
                Task.Delay(6000).Wait();
            }
        }
    }
}
