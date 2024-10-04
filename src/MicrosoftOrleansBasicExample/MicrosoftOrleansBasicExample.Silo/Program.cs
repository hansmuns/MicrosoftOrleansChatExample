using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using System.Net;

namespace MicrosoftOrleansBasicExample.Silo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IHostBuilder builder = Host.CreateDefaultBuilder(args)
                .UseOrleans((context, silo) =>
                {
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        silo.UseLocalhostClustering()
                            .AddMemoryGrainStorage("azure");
                    }
                    else
                    {
                        silo.Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "default";
                            options.ServiceId = "defaultService";
                        })
                        .UseAzureStorageClustering(options =>
                        {
                            options.TableServiceClient = new(context.Configuration.GetConnectionString("ORLEANS_AZURE_STORAGE_CONNECTION_STRING"));
                            options.TableName = $"defaultCluster";
                        })
                        .AddAzureTableGrainStorage("azure", options => {
                            options.TableServiceClient = new(context.Configuration.GetConnectionString("ORLEANS_AZURE_STORAGE_CONNECTION_STRING"));
                            options.TableName = $"defaultPersistence";
                        });

                        //The application is hosted in an Azure Web App
                        //This Web App has some pre defined environment variables, these are used to get the correct IP and ports
                        var endpointAddress = IPAddress.Parse(context.Configuration["WEBSITE_PRIVATE_IP"]!);
                        var strPorts = context.Configuration["WEBSITE_PRIVATE_PORTS"]!.Split(',');
                        if (strPorts.Length < 2)
                        {
                            throw new ArgumentException("Insufficient private ports configured.");
                        }
                        var (siloPort, gatewayPort) = (int.Parse(strPorts[0]), int.Parse(strPorts[1]));

                        silo.ConfigureEndpoints(endpointAddress, siloPort, gatewayPort);
                    }
                })
                .UseConsoleLifetime();

            using IHost host = builder.Build();
            await host.RunAsync();
        }
    }
}
