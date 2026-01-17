using Identidade.Publico.Enumerations;

namespace Identidade.Infraestrutura.Configuracoes
{
    public interface ISettings
    {
        ConnectionStrings ConnectionStrings { get; }
        RabbitMQ RabbitMQ { get; }
        AzureServiceBus AzureServiceBus { get; }
        Logging Logging { get; }
        SettingsAuthenticationType AuthenticationType { get; }
        MessageBroker MessageBroker { get; }
        string CryptoKey { get; }
        BaselineMetrics BaselineMetrics { get; }
        ApplicationInsightsSettings ApplicationInsights { get; }
    }

    public class BaselineMetrics
    {
        public bool EnableMetricsCollection { get; }
        public string MetricsOutputPath { get; }
        public int MetricsSamplingIntervalSeconds { get; }

        public BaselineMetrics(bool enableMetricsCollection, string metricsOutputPath, int metricsSamplingIntervalSeconds)
        {
            EnableMetricsCollection = enableMetricsCollection;
            MetricsOutputPath = metricsOutputPath;
            MetricsSamplingIntervalSeconds = metricsSamplingIntervalSeconds;
        }
    }
    public class ApplicationInsightsSettings
    {
        public string ConnectionString { get; }
        public ApplicationInsightsSettings(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}