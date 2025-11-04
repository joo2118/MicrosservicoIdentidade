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
    }
}
