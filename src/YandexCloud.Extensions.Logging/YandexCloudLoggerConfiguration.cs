using Yandex.Cloud.Credentials;

namespace YandexCloud.Extensions.Logging
{
    /// <summary>
    /// Yandex Cloud Logger Configuration <see cref="https://cloud.yandex.ru/docs/logging/api-ref/grpc/log_ingestion_service"/>
    /// </summary>
    public class YandexCloudLoggerConfiguration 
    {
        /// <summary>
        /// Sdk to use services provided by Yandex.Cloud.
        /// </summary>
        public ICredentialsProvider CredentialsProvider { get; set; }

        /// <summary>
        /// Entry should be written to default log group for the folder. 
        /// Value must match the regular expression ([a-zA-Z][-a-zA-Z0-9_.]{0,63})?.
        /// </summary>
        public string FolderId { get;set;}

        /// <summary>
        /// Entry should be written to log group resolved by ID. 
        /// Value must match the regular expression ([a-zA-Z][-a-zA-Z0-9_.]{0,63})?.
        /// </summary>
        public string LogGroupId { get; set; }

        /// <summary>
        /// Entry stream name. 
        /// Value must match the regular expression |[a-zA-Z][-a-zA-Z0-9_.]{0,63}.
        /// </summary>
        public string? StreamName { get; set; }

        /// <summary>
        /// Resource type, i.e., serverless.function. 
        /// Value must match the regular expression ([a-zA-Z][-a-zA-Z0-9_.]{0,63})?.
        /// </summary>
        public string ResourceType { get; set; }

        /// <summary>
        /// Resource ID, i.e., ID of the function producing logs. 
        /// Value must match the regular expression ([a-zA-Z0-9][-a-zA-Z0-9_.]{0,63})?.
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Default entry severity.
        /// </summary>
        public Dictionary<string, Microsoft.Extensions.Logging.LogLevel> LogLevel { get; set; }

    }
}
