using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentServiceBus;

public interface IPath
{
    string Path { get; }
}

public interface IEntity : IPath
{
    public ServiceBusClient Client { get; }
    public ServiceBusAdministrationClient AdministrationClient { get; }
    public ServiceBusSender Sender { get; }
}