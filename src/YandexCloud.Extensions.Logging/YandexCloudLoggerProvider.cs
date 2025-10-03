using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Yandex.Cloud;

namespace YandexCloud.Extensions.Logging
{
    [ProviderAlias("YandexCloudLogger")]    
    public sealed class YandexCloudLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable? _onChangeToken;
        private YandexCloudLoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, YandexCloudLogger> _loggers =
            new(StringComparer.OrdinalIgnoreCase);
         
        private readonly Sdk _sdk;
        public YandexCloudLoggerProvider(
            IOptionsMonitor<YandexCloudLoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;           
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);          
        }

        public YandexCloudLoggerProvider(Sdk sdk,
            IOptionsMonitor<YandexCloudLoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _sdk = sdk;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }

        public ILogger CreateLogger(string categoryName)
            => _loggers.GetOrAdd(categoryName, name => new YandexCloudLogger(name, GetCurrentConfig, _sdk));
        
                   

        private YandexCloudLoggerConfiguration GetCurrentConfig() => _currentConfig;

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken?.Dispose();
        }       

    }
}
