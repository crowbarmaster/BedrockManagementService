using BedrockService.Service;
using BedrockService.Service.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ServiceTests {
    public class ServiceTests {
        public class TestFixture : IDisposable {

            public static IHost Host = Program.CreateHostBuilder(new string[] { }).Build();
            public IBedrockService BedrockService = Host.Services.GetRequiredService<IBedrockService>();
            public JsonSerializerSettings SerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };

            public TestFixture() {
                BedrockService.TestStart();
                while (BedrockService.GetServiceStatus().ServiceStatus != ServiceStatus.Started) {
                    Task.Delay(100).Wait();
                }
            }

            public void Dispose() {
                BedrockService.TestStop();
                while (BedrockService != null && BedrockService.GetServiceStatus().ServiceStatus != ServiceStatus.Stopped) {
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
                Assert.True(_testFixture.BedrockService.GetServiceStatus().ServiceStatus == ServiceStatus.Started);
            }

            [Fact]
            public void Verify_Service_Stop() {
                _testFixture.Dispose();
                Assert.True(_testFixture.BedrockService.GetServiceStatus().ServiceStatus == ServiceStatus.Stopped);
            }
        }
    }
}