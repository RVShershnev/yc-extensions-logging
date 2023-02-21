using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using Yandex.Cloud;
using Yandex.Cloud.Logging.V1;
using Microsoft.Extensions.DependencyInjection;
using static Yandex.Cloud.K8S.V1.NetworkPolicy.Types;
using static Yandex.Cloud.Logging.V1.LogLevel.Types;

namespace YandexCloud.Extensions.Logging
{
    public sealed class YandexCloudLogger : Microsoft.Extensions.Logging.ILogger
    {
        private const int MaxCountLogs = 100;
             
        private readonly string _name;
        private readonly Func<YandexCloudLoggerConfiguration> _getCurrentConfig;

        private CancellationTokenSource tokenSource = new CancellationTokenSource();
                
        private readonly LogIngestionService.LogIngestionServiceClient _yandexLogIngestionServiceClient;
                
        private readonly Sdk _sdk;
   
        private readonly int _seconds = 10;
        private readonly int _retryCount = 10;              

        ConcurrentQueue<IncomingLogEntry> _queue = new ConcurrentQueue<IncomingLogEntry>();
       
        public YandexCloudLogger(string name, Func<YandexCloudLoggerConfiguration> getCurrentConfig, Sdk? sdk = default) 
        {
            (_name, _getCurrentConfig, _sdk) = (name, getCurrentConfig, sdk);
            var config = _getCurrentConfig();
            if (getCurrentConfig().CredentialsProvider is not null)
            {
                _sdk = new Sdk(config.CredentialsProvider);
            }
            if(_sdk is null)
            {
                throw new Exception("Yandex cloud credentials provider is not found.");
            }
            _yandexLogIngestionServiceClient = _sdk.Services.Logging.LogIngestionService;
            Task.Run(() => Start(tokenSource.Token), tokenSource.Token);
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
            => logLevel != Microsoft.Extensions.Logging.LogLevel.None;             
        
        private Level? ConvertLogLevel(Microsoft.Extensions.Logging.LogLevel logLevel) => logLevel switch
        {
            Microsoft.Extensions.Logging.LogLevel.Trace => Level.Trace,
            Microsoft.Extensions.Logging.LogLevel.Debug => Level.Debug,
            Microsoft.Extensions.Logging.LogLevel.Information => Level.Info,
            Microsoft.Extensions.Logging.LogLevel.Warning => Level.Warn,
            Microsoft.Extensions.Logging.LogLevel.Error => Level.Error,
            Microsoft.Extensions.Logging.LogLevel.Critical => Level.Fatal,
            _ => null
        };             

        public async void Log<TState>(
            Timestamp dateTime,
            Microsoft.Extensions.Logging.LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var config = _getCurrentConfig();

            if (!IsEnabled(logLevel))
            {
                return;
            }

            Level? convertibleLogLevel = ConvertLogLevel(logLevel);
            if (convertibleLogLevel is null)
            {
                return;
            }

            IncomingLogEntry entry;                      
            if (typeof(TState).FullName == "Microsoft.Extensions.Logging.FormattedLogValues")
            {
                entry = new()
                {
                    Level = convertibleLogLevel.Value,
                    Message = state.ToString(),
                    Timestamp = dateTime,
                    StreamName = config.StreamName ?? _name
                };
            }
            else
            {                
                entry = new()
                {
                    JsonPayload = Struct.Parser.ParseJson(JsonSerializer.Serialize(state)),
                    Level = convertibleLogLevel.Value,
                    Message = $"{eventId.Id} {eventId.Name}",
                    Timestamp = dateTime,
                    StreamName = config.StreamName ?? _name
                };
            }
            _queue.Enqueue(entry);                            
        }

        public async void Log<TState>(
            Microsoft.Extensions.Logging.LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Log(DateTime.UtcNow.ToTimestamp(), logLevel, eventId, state, exception, formatter);
        }


        private async Task Start(CancellationToken cancellationToken = default) 
        {
            var time = TimeSpan.FromSeconds(5);
            TimeSpan timeAdd = new TimeSpan();
            while (!cancellationToken.IsCancellationRequested)
            {
                if(_queue.Count > 100 || IsTime(ref time, ref timeAdd))                
                {                   
                    var count = _queue.Count;
                    if (count > 0)
                    {
                        var array = new IncomingLogEntry[count];
                        for (var i = 0; i < count && i < MaxCountLogs; i++)
                        {
                            _queue.TryDequeue(out array[i]);
                        }
                        try 
                        {
                            var responce = await SendLogs(array);
                        }
                        catch(Exception exp)
                        {
                            System.Diagnostics.Debug.WriteLine(exp.Message);                          
                        }
                    }
                }
                await Task.Delay(100);
                timeAdd = timeAdd.Add(TimeSpan.FromMilliseconds(100));
            }
        }

        private bool IsTime(ref TimeSpan time, ref TimeSpan add)
        {
            if(time <= add)
            {
                add = new TimeSpan();
                return true;
            }
            return false;
        }

        private async Task<WriteResponse> SendLogs(IEnumerable<IncomingLogEntry> list, CancellationToken cancellationToken = default)
        {
            WriteResponse writeResponse = null;
            int currentRetry = 0;
            var config = _getCurrentConfig();
            while (true)
            {
                try
                {
                    WriteRequest request = new()
                    {
                        Destination = new Destination()
                        {
                            FolderId = config.FolderId,
                            LogGroupId = config.LogGroupId
                        },
                        Resource = new LogEntryResource()
                        {
                            Id = config.ResourceId,
                            Type = config.ResourceType
                        },
                    };
                    request.Entries.Add(list);
                    writeResponse = await _yandexLogIngestionServiceClient.WriteAsync(request);
                    break;

                }
                catch (Exception ex)
                {
                    if (currentRetry > this._retryCount || IsTransient(ex))
                    {
                        // If this is not a transient error
                        // or we should not retry re-throw the exception.
                        throw;
                    }
                    currentRetry++;
                    await Task.Delay(TimeSpan.FromSeconds(_seconds).Milliseconds);
                    Console.WriteLine(ex);
                }
            }
            return writeResponse;
        }        

        private bool IsTransient(Exception ex)
        {
            // Determine if the exception is transient.
            // In some cases this is as simple as checking the exception type, in other
            // cases it might be necessary to inspect other properties of the exception.
            if (ex is OperationCanceledException)
            {
                return true;
            }

            // Additional exception checking logic goes here.
            return false;
        }

    }
}
