using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace YandexCloud.Extensions.Logging
{
    public static class YandexCloudLoggerExtensions
    {
        public static ILoggingBuilder AddYandexCloudLogger(
            this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton<ILoggerProvider, YandexCloudLoggerProvider>());
                      
            builder.Services.TryAddEnumerable(Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton<IConfigureOptions<LoggerFilterOptions>, YandexCloudFiltersConfigureOptions>());

            LoggerProviderOptions.RegisterProviderOptions
                <YandexCloudLoggerConfiguration, YandexCloudLoggerProvider>(builder.Services);

            return builder;
        }               

        public static ILoggingBuilder AddYandexCloudLogger(
            this ILoggingBuilder builder,
            Action<YandexCloudLoggerConfiguration> configure)
        {
            builder.AddYandexCloudLogger();                       

            builder.Services.Configure(configure);

            return builder;
        }     
    }
}
