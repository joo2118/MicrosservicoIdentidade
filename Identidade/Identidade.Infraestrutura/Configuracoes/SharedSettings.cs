using Microsoft.Extensions.Configuration;
using Identidade.Publico.Enumerations;
using Serilog.Events;

namespace Identidade.Infraestrutura.Configuracoes
{
    public abstract class SharedSettings : ISettings
    {
        public ConnectionStrings ConnectionStrings { get; }
        public RabbitMQ RabbitMQ { get; }
        public AzureServiceBus AzureServiceBus { get; }
        public Logging Logging { get; }
        public SettingsAuthenticationType AuthenticationType { get; }
        public MessageBroker MessageBroker { get; }
        public string CryptoKey { get; }
        public BaselineMetrics BaselineMetrics { get; }

        protected SharedSettings(IConfiguration configuration)
        {
            ConnectionStrings = new ConnectionStrings
            (
                defaultConnection: configuration.GetConnectionString("DefaultConnection")
            );

            var rabbitMQSection = configuration.GetSection("RabbitMQ");
            RabbitMQ = new RabbitMQ
            (
                host: rabbitMQSection.GetValue("Host", string.Empty),
                virtualHost: rabbitMQSection.GetValue("VirtualHost", string.Empty),
                username: rabbitMQSection.GetValue("Username", string.Empty),
                password: rabbitMQSection.GetValue("Password", string.Empty)
            );

            var azureServiceBusSection = configuration.GetSection("AzureServiceBus");
            AzureServiceBus = new AzureServiceBus
            (
                connectionString: azureServiceBusSection.GetValue("ConnectionString", string.Empty),
                queueName: azureServiceBusSection.GetValue("QueueName", string.Empty)
            );

            var loggingSection = configuration.GetSection("Logging");
            var logLevelsSection = loggingSection.GetSection("LogLevels");
            Logging = new Logging
            (
                writeToFile: loggingSection.GetValue("WriteToFile", false),
                filePath: loggingSection.GetValue("FilePath", string.Empty),
                writeToElasticSearch: loggingSection.GetValue("WriteToElasticSearch", false),
                elasticSearchAddress: loggingSection.GetValue("ElasticSearchAddress", string.Empty),
                logLevel: loggingSection.GetValue("LogLevel", LogEventLevel.Information)
            );

            var baselineMetricsSection = configuration.GetSection("BaselineMetrics");
            BaselineMetrics = new BaselineMetrics
            (
                enableMetricsCollection: baselineMetricsSection.GetValue("EnableMetricsCollection", false),
                metricsOutputPath: baselineMetricsSection.GetValue("MetricsOutputPath", "metrics\\baseline-{Date}.json"),
                metricsSamplingIntervalSeconds: baselineMetricsSection.GetValue("MetricsSamplingIntervalSeconds", 60)
            );

            MessageBroker = configuration.GetValue("MessageBroker", MessageBroker.AzureServiceBus);
            AuthenticationType = configuration.GetValue("AuthenticationType", SettingsAuthenticationType.ActiveDirectory);
            CryptoKey = configuration.GetValue("CryptoKey", string.Empty);
        }
    }

    public class ConnectionStrings
    {
        public string DefaultConnection { get; }

        public ConnectionStrings(string defaultConnection)
        {
            DefaultConnection = defaultConnection;
        }
    }

    public class RabbitMQ
    {
        public string Host { get; }
        public string VirtualHost { get; }
        public string UserName { get; }
        public string Password { get; }

        public RabbitMQ(string host, string virtualHost, string username, string password)
        {
            Host = host;
            VirtualHost = virtualHost;
            UserName = username;
            Password = password;
        }
    }

    public class AzureServiceBus
    {
        public string ConnectionString { get; }
        public string QueueName { get; }

        public AzureServiceBus(string connectionString, string queueName)
        {
            ConnectionString = connectionString;
            QueueName = queueName;
        }
    }

    public class Logging
    {
        public bool WriteToFile { get; }
        public string FilePath { get; }
        public string ElasticSearchAddress { get; }
        public bool WriteToElasticSearch { get; }
        public LogEventLevel LogLevel { get; }

        public Logging(bool writeToFile, string filePath, string elasticSearchAddress, bool writeToElasticSearch, LogEventLevel logLevel)
        {
            WriteToFile = writeToFile;
            FilePath = filePath;
            ElasticSearchAddress = elasticSearchAddress;
            WriteToElasticSearch = writeToElasticSearch;
            LogLevel = logLevel;
        }
    }
}