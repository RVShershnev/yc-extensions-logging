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

            var message = new Mes();

            logger.Log(LogLevel.Trace, AppLogEvents.Read, new CustomLog("1", "Log", "About", "That"), null, (x,y) => x.Message);
            logger.Log(LogLevel.Information, "My log");     

            logger.LogTrace("log trace");        

            logger.LogDebug("log debug");        

            logger.LogInformation("log information");       

            logger.LogWarning("log warning");       

            logger.LogError("log error");      

            logger.LogCritical("log critical");                                

            await host.RunAsync();
        }
    }

    public class Mes
    {
        string Title { get; set; } = "Bug";
        string Description { get; set; } = "About bug.";
    }
}