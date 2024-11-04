using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinecraftService.Service;
using MinecraftService.Service.Core;
using MinecraftService.Service.Core.Interfaces;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MMS_Tests.ServiceTests {
    public class ServiceTests {
        public class TestFixture : IDisposable {

            public IHost Host;
            public IMinecraftService MinecraftService;
            public JsonSerializerSettings SerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };

            public TestFixture() {
                Host = Program.CreateHostBuilder(new string[] { }).Build();
                MinecraftService = Host.Services.GetRequiredService<IMinecraftService>();
            }

            public async Task StartFixture() {
                await Task.Run(() => {
                    if (MinecraftService.Start(null)) {
                        while (MinecraftService != null && MinecraftService.GetServiceStatus().ServiceStatus != ServiceStatus.Started) {
                            Task.Delay(100).Wait();
                        }
                    }
                });
            }

            public void Dispose() {
                if (MinecraftService.Stop(null)) {
                    while (MinecraftService != null && MinecraftService.GetServiceStatus().ServiceStatus != ServiceStatus.Stopped) {
                        Task.Delay(100).Wait();
                    }
                }
            }
        }

        public class RunningServiceTests : IClassFixture<TestFixture> {
            readonly TestFixture _testFixture;

            public RunningServiceTests() {
                _testFixture = new();
            }

            [Fact]
            public async Task Verify_Service_Startup() {
                Assert.Equal(ServiceStatus.Stopped, _testFixture.MinecraftService.GetServiceStatus().ServiceStatus);
                await _testFixture.StartFixture().Wait();
                Assert.Equal(ServiceStatus.Started, _testFixture.MinecraftService.GetServiceStatus().ServiceStatus);
            }

            [Fact]
            public void Verify_Service_Stop() {
                Assert.Equal(ServiceStatus.Started, _testFixture.MinecraftService.GetServiceStatus().ServiceStatus);
                _testFixture.Dispose();
                Assert.Equal(ServiceStatus.Stopped, _testFixture.MinecraftService.GetServiceStatus().ServiceStatus);
            }
        }
    }
}