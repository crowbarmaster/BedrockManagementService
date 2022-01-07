using BedrockManagementServiceASP.BedrockService.Core.Interfaces;
using BedrockManagementServiceASP.BedrockService.Management;
using BedrockManagementServiceASP.BedrockService.Networking;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
IProcessInfo processInfo = new ServiceProcessInfo("Test", Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), Process.GetCurrentProcess().Id, true, true);
builder.Services.AddHostedService<BedrockManagementServiceASP.BedrockService.Core.Service>()
    .AddSingleton(processInfo)
    //.AddSingleton<NetworkStrategyLookup>()
    .AddSingleton<ServerConfigurator>()
    .AddSingleton<IServiceConfiguration>(x => x.GetRequiredService<ServiceConfigurator>())
    .AddSingleton<IBedrockConfiguration>(x => x.GetRequiredService<ServiceConfigurator>())
    .AddSingleton<IBedrockLogger, BedrockLogger>()
    .AddSingleton<IBedrockService, BedrockManagementServiceASP.BedrockService.Core.BedrockService>()
    //.AddSingleton<ITCPListener, TCPListener>()
    .AddSingleton<IConfigurator, ConfigManager>()
    .AddSingleton<IUpdater, Updater>();

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseStatusCodePages();

app.UseDeveloperExceptionPage();

app.MapControllerRoute("home", "home", new { Controller = "Home", action = "Index" });

app.MapDefaultControllerRoute();

app.MapRazorPages();

app.Run();
