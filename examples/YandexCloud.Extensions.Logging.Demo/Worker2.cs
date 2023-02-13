using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YandexCloud.Extensions.Logging.Demo
{
    public class Worker2 : BackgroundService
    {
        private readonly ILogger<Worker2> _logger;

        public Worker2(ILogger<Worker2> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int i = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    i++;
                    if (i % 10 == 5)
                    {
                        throw new Exception("Exception message to Yandex Cloud");
                    }
                    _logger.LogInformation("Worker2: message {number}; time: {time}.", i, DateTimeOffset.Now);
                    await Task.Delay(500, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, ex.Message);
                }
            }
        }
    }
}
