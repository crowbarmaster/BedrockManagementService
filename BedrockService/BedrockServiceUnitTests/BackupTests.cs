using BedrockService.Service;
using BedrockService.Service.Core;
using BedrockService.Service.Management;
using BedrockService.Service.Networking;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Forms;
using Topshelf;

namespace BedrockServiceUnitTests
{
    [TestClass]
    public class BackupTests
    {
        [TestMethod]
        public void TestAllFunctions()
        {
            IServiceCollection services = new ServiceCollection();
            Startup startup = new Startup(false, false);
            startup.ConfigureServices(services);
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<IConfigurator>().LoadAllConfigurations().Wait();
            serviceProvider.GetRequiredService<IUpdater>().CheckUpdates().Wait();
            IService service = serviceProvider.GetRequiredService<IService>();
            NetworkStrategyLookup networkStrategyLookup = serviceProvider.GetRequiredService<NetworkStrategyLookup>();
            IBedrockService bedrockService = serviceProvider.GetRequiredService<IBedrockService>();
            bedrockService.Start(null);
            BedrockService.Client.Management.FormManager.GetMainWindow.PerformBackupTests();
            
        }
    }
}
