# YandexCloud.Extensions.Logging
A Yandex Cloud Logging provider for Microsoft.Extensions.Logging, the logging subsystem used by ASP.NET Core.

## Install packege
`
dotnet add package YandexCloud.Extensions.Logging
`

## Creating a log group

[Create a log group](https://cloud.yandex.com/en-ru/docs/logging/operations/create-group) in Yandex Cloud.

## Log level conversion

|Microsoft|Yandex Cloud|
|:--------|:---|
||Unspecified|
|Trace|Trace|
|Debug|Debug|
|Information|Info|
|Warning|Warn|
|Error|Error|
|Critical|Fatal|
|None||


## Configuration

authorized_key.json
```json
{
  "id": "***",
  "service_account_id": "***",
  "created_at": "***",
  "key_algorithm": "RSA_2048",
  "public_key": "***",
  "private_key": "***"
}
```
appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    },
    "YandexCloudLogger": {
      "FolderId": "b2g7r20so8vkbaq4fr2f",
      "LogGroupId": "e336oju1edkvemcrc4fb",
      "ResourceType": "Demo",
      "ResourceId": "1",
      "LogLevel":{
        "Default": "Debug"
      }
    }
  }
}
```

Program.cs
```csharp
using Yandex.Cloud;
using System.Text.Json;
using YandexCloud.IamJwtCredentials;

public static async Task Main(string[] args)
{

    IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services.AddSingleton(new Sdk(new IamJwtCredentialsProvider(JsonSerializer.Deserialize<IamJwtCredentialsConfiguration>(File.ReadAllText("authorized_key.json")))));
        })
        .ConfigureLogging(builder =>
            builder.ClearProviders()
                .AddConsole()
                .AddYandexCloudLogger())        
        .Build();
                
    var logger = host.Services.GetRequiredService<ILogger<Program>>();         

    logger.Log(LogLevel.Debug, "my log");

    await host.RunAsync();
}
```

