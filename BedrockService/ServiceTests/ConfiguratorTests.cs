using BedrockService.Service;
using BedrockService.Service.Management;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace ServiceTests
{
    public class ConfiguratorTests
    {
        readonly IServiceProvider _services =
        Program.CreateHostBuilder(new string[] { }).Build().Services; // one liner
        IConfigurator myService;

        [Fact]
        public void VerifyServiceInContainer()
        {
            myService = _services.GetRequiredService<IConfigurator>();
            Assert.NotNull(myService);
        }
    }
}