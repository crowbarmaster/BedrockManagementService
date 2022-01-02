using BedrockService.Service;
using BedrockService.Service.Core;
using BedrockService.Service.Core.Interfaces;
using BedrockService.Service.Networking;
using BedrockService.Service.Networking.MessageInterfaces;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;
using System.Text;
using Xunit.Abstractions;

namespace ServiceTests {
    public class ServiceTests {
        public class TestFixture : IDisposable {

            public static IHost Host = Program.CreateHostBuilder(new string[] { }).Build();
            public IBedrockService BedrockService = Host.Services.GetRequiredService<IBedrockService>();
            public JsonSerializerSettings SerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };
            
            public TestFixture() {
                BedrockService.TestStart();
                while(BedrockService.GetServiceStatus() != ServiceStatus.Started) {
                    Task.Delay(100).Wait();
                }
            }

            public void Dispose() {
                BedrockService.TestStop();
                while (BedrockService != null && BedrockService.GetServiceStatus() != ServiceStatus.Stopped) {
                    Task.Delay(100).Wait();
                }
            }
        }

        public class RunningServiceTests : IClassFixture<TestFixture> {
            TestFixture _testFixture;

            public RunningServiceTests(TestFixture testFixture) {
                _testFixture = testFixture;
            }

            [Fact]
            public void Verify_Service_Startup() {
                Assert.True(_testFixture.BedrockService.GetServiceStatus() == ServiceStatus.Started);
            }

            [Fact]
            public void Verify_Service_Stop() {
                _testFixture.Dispose();
                Assert.True(_testFixture.BedrockService.GetServiceStatus() == ServiceStatus.Stopped);
            }
        }
    }
}