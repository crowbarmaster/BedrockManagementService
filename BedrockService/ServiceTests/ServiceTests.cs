using BedrockService.Service;
using BedrockService.Service.Core;
using BedrockService.Service.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ServiceTests {
    public class ServiceTests {
        IHost _host = Program.CreateHostBuilder(new string[] { }).Build();

        [Fact]
        public void Verify_Service_In_Container() {
            IBedrockService myService = _host.Services.GetRequiredService<IBedrockService>();
            Assert.NotNull(myService);
        }

        [Fact]
        public void Verify_Service_Startup() {
            _host.Run();
        }
    }
}