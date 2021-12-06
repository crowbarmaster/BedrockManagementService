// This Startup file is based on ASP.NET Core new project templates and is included
// as a starting point for DI registration and HTTP request processing pipeline configuration.
// This file will need updated according to the specific scenario of the application being upgraded.
// For more information on ASP.NET Core startup files, see https://docs.microsoft.com/aspnet/core/fundamentals/startup

using BedrockService.Service.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace BedrockService.Service
{
    public class Startup
    {
        private readonly bool _isDebug;
        private readonly bool _isConsole;
        public Startup(bool isDebug, bool isConsole)
        {
            _isDebug = isDebug;
            _isConsole = isConsole;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews(ConfigureMvcOptions)
                // Newtonsoft.Json is added for compatibility reasons
                // The recommended approach is to use System.Text.Json for serialization
                // Visit the following link for more guidance about moving away from Newtonsoft.Json to System.Text.Json
                // https://docs.microsoft.com/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to
                .AddNewtonsoftJson(options =>
                {
                    options.UseMemberCasing();
                });
            IProcessInfo processInfo = new ServiceProcessInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileName(Assembly.GetExecutingAssembly().Location), Process.GetCurrentProcess().Id, _isDebug, _isConsole);
            services.AddSingleton(processInfo);
            services.AddSingleton<IService, Core.Service>();
            services.AddSingleton<IServiceConfiguration, ServiceInfo>();
            services.AddSingleton<ITCPListener, TCPListener>();
            services.AddSingleton<IBedrockLogger, ServiceLogger>();
            services.AddSingleton<NetworkStrategyLookup>();
            services.AddSingleton<IConfigurator, ConfigManager>();
            services.AddSingleton<IBedrockService, Core.BedrockService>();
            services.AddSingleton<IUpdater, Updater>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void ConfigureMvcOptions(MvcOptions mvcOptions)
        {
        }
    }
}
