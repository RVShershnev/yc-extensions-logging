namespace YandexCloud.Extensions.Logging.Demo
{
    public class Worker3 : BackgroundService
    {
        private readonly ILogger<Worker3> _logger;

        public Worker3(ILogger<Worker3> logger)
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
                    if (i % 10 == 4)
                    {
                        throw new Exception("Exception message to Yandex Cloud");
                    }
                    _logger.LogInformation("Worker3: message {number}; time: {time}.", i, DateTimeOffset.Now);
                    await Task.Delay(500, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, ex.Message);
                }
            }
        }
    }
}