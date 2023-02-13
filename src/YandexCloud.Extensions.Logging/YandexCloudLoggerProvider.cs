using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YandexCloud.Extensions.Logging
{
    [ProviderAlias("YandexCloudLoggerProvider")]    
    public sealed class YandexCloudLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable? _onChangeToken;
        private YandexCloudLoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, YandexCloudLogger> _loggers =
            new(StringComparer.OrdinalIgnoreCase);

        public YandexCloudLoggerProvider(
            IOptionsMonitor<YandexCloudLoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new YandexCloudLogger(name, GetCurrentConfig));

        private YandexCloudLoggerConfiguration GetCurrentConfig() => _currentConfig;

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken?.Dispose();
        }
    }
}
