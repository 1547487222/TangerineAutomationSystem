using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using TangerineBlazorApp.Models;

namespace TangerineBlazorApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            
            // Configure backend API settings
            var backendSettings = builder.Configuration.GetSection("BackendApi").Get<BackendApiSettings>() ?? new BackendApiSettings();
            builder.Services.AddSingleton(backendSettings);
            
            builder.Services.AddAntDesign();
            builder.Services.AddSingleton<FlowLaunchService>();
            builder.Services.AddSingleton<Smart_Lab_OS.SmartLabOSSever.SmartLabOSSeverClient>(p => 
            {
                var settings = p.GetRequiredService<BackendApiSettings>();
                var channel = GrpcChannel.ForAddress(settings.GrpcEndpoint, new GrpcChannelOptions
                {
                    HttpHandler = new GrpcWebHandler(new HttpClientHandler()),
                    
                });
                var client = new Smart_Lab_OS.SmartLabOSSever.SmartLabOSSeverClient(channel);
                return client;
            });
            builder.Services.AddSingleton<PlatformScheduleService>();
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            await builder.Build().RunAsync();
        }
    }
}
