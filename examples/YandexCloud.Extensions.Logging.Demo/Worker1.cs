using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YandexCloud.Extensions.Logging.Demo
{
    public class Worker1 : BackgroundService
    {
        private readonly ILogger<Worker1> _logger;

        public Worker1(ILogger<Worker1> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int i= 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    i++;
                    if(i % 10 == 9)
                    {
                        throw new Exception("Exception message to Yandex Cloud");
                    }
                    _logger.LogInformation("Worker1: message {number}; time: {time}.", i, DateTimeOffset.Now);
                    await Task.Delay(500, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }         
            }
        }
    }
}
