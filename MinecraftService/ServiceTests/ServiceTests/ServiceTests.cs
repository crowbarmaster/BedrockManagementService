using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinecraftService.Service;
using MinecraftService.Service.Core.Interfaces;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MMS_Tests.ServiceTests {
    public class ServiceTests {
        public class TestFixture : IDisposable {

            public static IHost Host = Program.CreateHostBuilder(new string[] { }).Build();
            public IMinecraftService MinecraftService = Host.Services.GetRequiredService<IMinecraftService>();
            public JsonSerializerSettings SerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };

            public TestFixture() {
                MinecraftService.TestStart();
                while (MinecraftService.GetServiceStatus().ServiceStatus != ServiceStatus.Started) {
                    Task.Delay(100).Wait();
                }
            }

            public void Dispose() {
                MinecraftService.TestStop();
                while (MinecraftService != null && MinecraftService.GetServiceStatus().ServiceStatus != ServiceStatus.Stopped) {
                    Task.Delay(100).Wait();
                }
            }
        }

        public class RunningServiceTests : IClassFixture<TestFixture> {
            readonly TestFixture _testFixture;

            public RunningServiceTests(TestFixture testFixture) {
                _testFixture = testFixture;
            }

            [Fact]
            public void Verify_Service_Startup() {
                Assert.True(_testFixture.MinecraftService.GetServiceStatus().ServiceStatus == ServiceStatus.Started);
            }

            [Fact]
            public void Verify_Service_Stop() {
                _testFixture.Dispose();
                Assert.True(_testFixture.MinecraftService.GetServiceStatus().ServiceStatus == ServiceStatus.Stopped);
            }
        }
    }
}