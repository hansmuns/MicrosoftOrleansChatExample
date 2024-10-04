using Blazored.LocalStorage;
using MicrosoftOrleansBasicExample.Client.Components;
using MudBlazor.Services;
using Orleans.Configuration;

namespace MicrosoftOrleansBasicExample.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();


            // Add MudBlazor services
            builder.Services.AddMudServices();

            // Add Orleans client.
            builder.UseOrleansClient((client) =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    client.UseLocalhostClustering();
                }
                else
                {
                    client.Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "default";
                        options.ServiceId = "defaultService";
                    })
                    .UseAzureStorageClustering(options =>
                    {
                        options.TableServiceClient = new(builder.Configuration.GetConnectionString("ORLEANS_AZURE_STORAGE_CONNECTION_STRING"));
                        options.TableName = $"defaultCluster";
                    });
                }
            });

            // Add Localstorage
            builder.Services.AddBlazoredLocalStorage();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
