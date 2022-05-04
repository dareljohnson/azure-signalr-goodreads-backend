using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SignalR.Server.Interfaces;
using SignalR.Server.Services;

[assembly: FunctionsStartup(typeof(SignalR.Server.Startup))]

namespace SignalR.Server
{
    public class Startup: FunctionsStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //Add Azure SignalR
            services.AddSignalR(srConfig => srConfig.EnableDetailedErrors = true)
                .AddAzureSignalR(azureConfig =>
                {
                    azureConfig.ConnectionString = Configuration.signalRConnection;
                });
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Add Function App Dependency Injection
            builder.Services.AddSingleton<IGoodReadsService, GoodReadsService>();
        }
    }
}
