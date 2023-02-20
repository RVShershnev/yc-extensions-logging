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
            Random random = new Random();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var n = random.Next(0, 7);
                    i++;
                    switch (n)
                    {
                        case 0:
                            _logger.LogTrace("Worker3: message {number}; time: {time}.", i, DateTimeOffset.Now);
                            break;
                        case 1:
                            _logger.LogDebug("Worker3: message {number}; time: {time}.", i, DateTimeOffset.Now);
                            break;
                        case 2:
                            _logger.LogInformation("Worker3: message {number}; time: {time}.", i, DateTimeOffset.Now);
                            break;
                        case 3:
                            _logger.LogWarning("Worker3: message {number}; time: {time}.", i, DateTimeOffset.Now);
                            break;
                        case 4:
                            _logger.LogError("Worker3: message {number}; time: {time}.", i, DateTimeOffset.Now);
                            break;
                        case 5:
                            _logger.LogCritical("Worker3: message {number}; time: {time}.", i, DateTimeOffset.Now);
                            break;
                        case 6:
                            throw new Exception("Exception message to Yandex Cloud");                            
                        default:
                            break;
                    }                   
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