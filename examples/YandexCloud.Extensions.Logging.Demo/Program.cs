using Yandex.Cloud;
using System.Text.Json;
using YandexCloud.IamJwtCredentials;

namespace YandexCloud.Extensions.Logging.Demo
{
    public partial class Program
    {
        public static async Task Main(string[] args)
        {

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton(new Sdk(new IamJwtCredentialsProvider(JsonSerializer.Deserialize<IamJwtCredentialsConfiguration>(File.ReadAllText("../../../authorized_key.json")))));
                })
                .ConfigureLogging(builder =>
                    builder.ClearProviders()
                        .AddConsole()
                        .AddYandexCloudLogger())
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Worker1>();
                    services.AddHostedService<Worker2>();
                    services.AddHostedService<Worker3>();
                })
                .Build();
                       

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
                      
            logger.Log(LogLevel.Information, "My log");            

            await host.RunAsync();
        }
    }
}