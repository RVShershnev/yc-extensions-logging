using Yandex.Cloud;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using YandexCloud.IamJwtCredentials;

namespace YandexCloud.Extensions.Logging.Demo
{
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            Sdk yandexCloudSdk = new Sdk(new IamJwtCredentialsProvider(JsonSerializer.Deserialize<IamJwtCredentialsConfiguration>(File.ReadAllText("authorized_key.json"))));

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder =>
                    builder.ClearProviders()
                        .AddConsole()
                        .AddYandexCloudLogger(configuration =>
                        {
                            configuration.YandexCloudSdk = yandexCloudSdk;
                            configuration.FolderId = "b1g5g3933j1gg1tpj73r";
                            configuration.LogGroupId = "e23nlm1po5bkocikgb0c";
                            configuration.ResourceId = "1";
                            configuration.ResourceType = "MyResource";
                            configuration.StreamName = "MyStream";
                            configuration.LogLevel = LogLevel.Trace;
                        }))
                        .ConfigureServices(services =>
                        {
                            services.AddHostedService<Worker1>();
                            services.AddHostedService<Worker2>();
                            services.AddHostedService<Worker3>();
                        })
                        .Build();                      

            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            var MyLog = new CustomLog("1", "Part", "Sub", "Message");

            logger.LogTrace(AppLogEvents.Create, "Trace message");  
            logger.Log(LogLevel.Trace, AppLogEvents.Create, MyLog, exception: null, (x, y) => JsonSerializer.Serialize(x));
            logger.Log(LogLevel.Trace, AppLogEvents.Create, MyLog, exception: null, (x, y) => x.Id);
            logger.LogTrace(AppLogEvents.Create, "Trace message with formatter: {0} {1}", "formatter", 1);
            
            logger.LogDebug(AppLogEvents.Create, "Debug message");   
            logger.Log(LogLevel.Debug, AppLogEvents.Create, MyLog, exception: null, (x, y) => JsonSerializer.Serialize(x));
            logger.Log(LogLevel.Debug, AppLogEvents.Create, MyLog, exception: null, (x, y) => x.Id);
            logger.LogDebug(AppLogEvents.Create, "Debug message with formatter: {0} {1}", "formatter", 1);

            logger.LogInformation(AppLogEvents.Create, "Information message");
            logger.Log(LogLevel.Information, AppLogEvents.Create, MyLog, exception: null, (x, y) => JsonSerializer.Serialize(x));
            logger.Log(LogLevel.Information, AppLogEvents.Create, MyLog, exception: null, (x, y) => x.Id);
            logger.LogInformation(AppLogEvents.Create, "Trace message with formatter: {0} {1}", "formatter", 1);

            logger.LogWarning(AppLogEvents.Create, "Warning message");
            logger.Log(LogLevel.Warning, AppLogEvents.Create, MyLog, exception: null, (x, y) => JsonSerializer.Serialize(x));
            logger.Log(LogLevel.Warning, AppLogEvents.Create, MyLog, exception: null, (x, y) => x.Id);
            logger.LogWarning(AppLogEvents.Create, "Warning message with formatter: {0} {1}", "formatter", 1);

            logger.LogError(AppLogEvents.Create, "Error message");
            logger.Log(LogLevel.Error, AppLogEvents.Create, MyLog, exception: null, (x, y) => JsonSerializer.Serialize(x));
            logger.Log(LogLevel.Error, AppLogEvents.Create, MyLog, exception: null, (x, y) => x.Id);
            logger.LogError(AppLogEvents.Create, "Error message with formatter: {0} {1}", "formatter", 1);

            logger.LogCritical(AppLogEvents.Create, "Critical message");            
            logger.Log(LogLevel.Critical, AppLogEvents.Create, MyLog, exception: null, (x, y) => JsonSerializer.Serialize(x));
            logger.Log(LogLevel.Critical, AppLogEvents.Create, MyLog, exception: null, (x, y) => x.Id);
            logger.LogCritical(AppLogEvents.Create, "Critical message with formatter: {0} {1}", "formatter", 1);

            await host.RunAsync();
        }
    }
}