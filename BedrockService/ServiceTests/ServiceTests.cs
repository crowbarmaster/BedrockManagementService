using BedrockService.Service;
using BedrockService.Service.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace ServiceTests
{
    public class ServiceTests
    {
        readonly IServiceProvider _services =
    Program.CreateHostBuilder(new string[] { }).Build().Services; // one liner

        [Fact]
        public void VerifyServiceInContainer()
        {
            IBedrockService myService = _services.GetRequiredService<IBedrockService>();
            Assert.NotNull(myService);
        }
    }
}