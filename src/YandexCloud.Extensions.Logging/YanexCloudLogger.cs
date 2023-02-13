using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using Yandex.Cloud.Logging.V1;
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
               
        private readonly Destination _destination;
        private readonly string _streamName;
        private readonly string _resourceId;
        private readonly string _resourceType;
        private readonly int _seconds = 10;
        private readonly int _retryCount = 10;

        ConcurrentQueue<IncomingLogEntry> _queue = new ConcurrentQueue<IncomingLogEntry>();
        public YandexCloudLogger(
            string name,
            Func<YandexCloudLoggerConfiguration> getCurrentConfig)
        {
            (_name, _getCurrentConfig) = (name, getCurrentConfig);
            _yandexLogIngestionServiceClient = _getCurrentConfig().YandexCloudSdk.Services.Logging.LogIngestionService;
            _destination = new Destination()
            {
                FolderId = getCurrentConfig().FolderId,
                LogGroupId = getCurrentConfig().LogGroupId
            };
            _streamName = _getCurrentConfig().StreamName;
            _resourceId = _getCurrentConfig().ResourceId;
            _resourceType = _getCurrentConfig().ResourceType;
            Task.Run(()=>Start(tokenSource.Token), tokenSource.Token);          
        }


        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {           
           return ((int)_getCurrentConfig().LogLevel <= (int)logLevel)? true : false;
        }

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
            Microsoft.Extensions.Logging.LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var dateTimeNow = DateTime.UtcNow;       

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

            // TODO Поменять на фул нейм
            if (typeof(TState).Name == "FormattedLogValues")
            {
                entry = new()
                {
                    Level = convertibleLogLevel.Value,
                    Message = state.ToString(),
                    Timestamp = new Timestamp()
                    {
                        Nanos = dateTimeNow.ToTimestamp().Nanos,
                        Seconds = dateTimeNow.ToTimestamp().Seconds
                    },
                    StreamName = _streamName
                };
            }
            else
            {
                entry = new()
                {
                    JsonPayload = Struct.Parser.ParseJson(JsonSerializer.Serialize(state)),
                    Level = convertibleLogLevel.Value,
                    Message = $"({eventId.Id}) {eventId.Name}",
                    Timestamp = new Timestamp()
                    {
                        Nanos = dateTimeNow.ToTimestamp().Nanos,
                        Seconds = dateTimeNow.ToTimestamp().Seconds
                    },
                    StreamName = _streamName
                };
            }
            _queue.Enqueue(entry);                            
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
            while (true)
            {
                try
                {
                    WriteRequest request = new()
                    {
                        Destination = _destination,
                        Resource = new LogEntryResource()
                        {
                            Id = _resourceId,
                            Type = _resourceType
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
