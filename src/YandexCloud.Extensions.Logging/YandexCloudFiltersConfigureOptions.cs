using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace YandexCloud.Extensions.Logging
{
    internal sealed class YandexCloudFiltersConfigureOptions : IConfigureOptions<LoggerFilterOptions>
    {
        IOptionsMonitor<YandexCloudLoggerConfiguration> _configuration;
        public YandexCloudFiltersConfigureOptions(IOptionsMonitor<YandexCloudLoggerConfiguration> configuration)
        {
            _configuration = configuration;
        }
        public void Configure(LoggerFilterOptions options)
        {         
            foreach (var logLevel in _configuration.CurrentValue.LogLevel)
            {
                var rule = new LoggerFilterRule("YandexCloudLogger", logLevel.Key, logLevel.Value,
                    (provider, category, logLevel) =>
                    {
                        return provider.Contains(provider)
                            && category.Contains(category)
                            && logLevel == logLevel;
                    });
                options.Rules.Add(rule);
            }
        }
    }
}
